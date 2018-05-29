using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AOPLogger;
using Autofac.Extras.DynamicProxy2;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

namespace ScannedFileProcessor
{
    public interface IProcessScannedFiles
    {
        void Start();
        void Stop();
    }
    
   
    public class ScannedFileProcessingService : IProcessScannedFiles
    {
        private List<CustomFolderSettings> _foldersWatched;
        private readonly List<FileSystemWatcher> _fileSystemWatchers;
        private readonly List<Thread> _workerThreads;
        private readonly ManualResetEvent _stopEvent;

        public ScannedFileProcessingService()
        {
            _foldersWatched = new List<CustomFolderSettings>();
            _fileSystemWatchers = new List<FileSystemWatcher>();
            _workerThreads = new List<Thread>();
            _stopEvent = new ManualResetEvent(false);
        }

        [CRLogger]
        public void StartFileSystemWatcher()
        {
            foreach (var customFolder in _foldersWatched)
            {
                var dir = new DirectoryInfo(customFolder.FolderPath);
                if (!dir.Exists) continue;
                var fileSWatch = new FileSystemWatcher
                {
                    Path = customFolder.FolderPath
                };
                var newFileEvent = new AutoResetEvent(false);

                fileSWatch.Created += (senderObj, fileSysArgs) =>
                    FileSWatch_Created(senderObj, fileSysArgs,
                        newFileEvent); 
                fileSWatch.EnableRaisingEvents = true;
                _fileSystemWatchers.Add(fileSWatch);
                
                if (!new DirectoryInfo(Path.Combine(customFolder.FolderPath, "out")).Exists)
                {
                   Directory.CreateDirectory(Path.Combine(customFolder.FolderPath, "out"));
                }
                if (!new DirectoryInfo(Path.Combine(customFolder.FolderPath, "scanned")).Exists)
                {
                    Directory.CreateDirectory(Path.Combine(customFolder.FolderPath, "scanned"));
                }
                if (!new DirectoryInfo(Path.Combine(customFolder.FolderPath, "error")).Exists)
                {
                    Directory.CreateDirectory(Path.Combine(customFolder.FolderPath, "error"));
                }
                var workThread = new Thread(() => WorkProcedure(
                    fileSWatch.Path, Path.Combine(customFolder.FolderPath, "out"), Path.Combine(customFolder.FolderPath, "scanned"),
                    Path.Combine(customFolder.FolderPath, "error"), _stopEvent, newFileEvent));
                workThread.Start();
                _workerThreads.Add(workThread);
            }
        }

        [CRLogger]
        public void WorkProcedure(string inDir, string outDir, string scannedDir, string errorDir, ManualResetEvent stopWorkEvent,
            AutoResetEvent newFileEvent)
        {
            do
            {
                while(Directory.EnumerateFileSystemEntries(inDir).Any())
                foreach (var file in Directory.EnumerateFiles(inDir))
                {
                    if (stopWorkEvent.WaitOne(TimeSpan.Zero))
                        return;

                    var inFile = file;
                    if (TryOpen(inFile, 3))
                    {
                        ProcessScannedFile(inDir, outDir, scannedDir, errorDir, inFile);
                    }
                }

            }
            while (WaitHandle.WaitAny(new WaitHandle[] { stopWorkEvent, newFileEvent }, 1000) != 0);
        }

        [CRLogger]
        private void FileSWatch_Created(object sender, FileSystemEventArgs e, AutoResetEvent newFileEvent)
        {
            newFileEvent.Set();
        }

        [CRLogger]
        public void PopulateListFileSystemWatchers()
        {
            var fileNameXml = ConfigurationManager.AppSettings["XMLFileFolderSettings"];

            XmlSerializer deserializer =
                new XmlSerializer(typeof(List<CustomFolderSettings>));

            TextReader reader = new StreamReader(fileNameXml);

            object obj = deserializer.Deserialize(reader);
            reader.Close();

            _foldersWatched = obj as List<CustomFolderSettings>;
        }

        [CRLogger]
        public void ProcessScannedFile(string inDir, string outDir, string scannedDir, string errorDir, string fileName)
        {
            Match match = Regex.Match(fileName, @"([A-Za-z]+)_([0-9]+)\.(jpg|png)$");
            if (!match.Success)
            {
                var outFile = Path.Combine(errorDir, Path.GetFileName(fileName));
                File.Move(fileName, outFile);
                return;
            }
            var file_number = int.Parse(match.Groups[2].Value) + 1;
            var file_name = match.Groups[1].Value;
            var file_ext = match.Groups[3].Value;
            
            var next_page = file_name + "_" + file_number + "." + file_ext;
            var docList = new List<string> { fileName };
            while (true)
            {
                if (File.Exists(Path.Combine(inDir, next_page)))
                {
                    docList.Add(Path.Combine(inDir, next_page));
                    file_number++;
                    next_page = file_name + "_" + file_number + "." + file_ext;
                }
                else
                {
                    Thread.Sleep(int.Parse(ConfigurationManager.AppSettings["NextPageTimeOut"]));
                    if (!File.Exists(Path.Combine(inDir, next_page)))
                        break;
                }
            }


            if (MigraDocSample(inDir, outDir, docList, 3)) {
                foreach (var file in docList)
                {
                    if (TryOpen(file, 3))
                    {
                        File.Move(file, Path.Combine(scannedDir, Path.GetFileName(file)));
                        File.Delete(file);
                    }
                }
            }
            else{
                foreach (var file in docList)
                {
                    if (TryOpen(file, 3))
                    {
                        File.Move(file, Path.Combine(errorDir, Path.GetFileName(file)));
                        File.Delete(file);
                    }
                }
            }
        }

        private bool TryOpen(string fileName, int tryCount)
        {
            for (int i = 0; i < tryCount; i++)
            {
                try
                {
                    var file = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
                    file.Close();

                    return true;
                }
                catch (IOException)
                {
                    Thread.Sleep(5000);
                }
            }

            return false;
        }

        [CRLogger]
        public bool MigraDocSample(string indir, string outdir, List<string> files, int tryCount)
        {
            try
            {
                var document = new Document();
                var section = document.AddSection();
                var resultName = Path.GetFileNameWithoutExtension(files[0]);
                foreach (var file in files)
                {
                    if (document.Sections.Count != 1)
                        section.AddPageBreak();
                    var img = section.AddImage(file);
                    img.Height = document.DefaultPageSetup.PageHeight;
                    img.Width = document.DefaultPageSetup.PageWidth;


                }

                var render = new PdfDocumentRenderer();
                render.Document = document;

                render.RenderDocument();
                render.Save(Path.Combine(outdir, resultName + ".pdf"));
                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        [CRLogger]
        public void Start()
        {
            PopulateListFileSystemWatchers();
            StartFileSystemWatcher();
        }

        [CRLogger]
        public void Stop()
        {
            _stopEvent.Set();
            foreach (var  fsw in _fileSystemWatchers)
            {
                fsw.EnableRaisingEvents = false;
            }
            foreach (var wt in _workerThreads)
            {
                wt.Join();
            }
            _fileSystemWatchers.Clear();
        }
    }

}

