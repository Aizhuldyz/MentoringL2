using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using Messages;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace ScannedFileProcessor
{

    public class ScannedFileProcessingService
    {
        private CustomFolderSettings _folderWatched;
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly List<Thread> _workerThreads;
        private readonly ManualResetEvent _stopEvent;
        private  string _scannerStatus;
        private readonly string PdfDocQueue = "ScannedPdfQueue";
        private readonly string ClientSettingsQueue = "ClientSettingsQueue";
        private readonly string ClientSettingstopic = "ClientSettingsTopic";
        private readonly string ClientSettingsSubs = "ScannerSettings";

        public ScannedFileProcessingService()
        {
            _folderWatched = new CustomFolderSettings();
            _fileSystemWatcher = new FileSystemWatcher();
            _workerThreads = new List<Thread>();
            _stopEvent = new ManualResetEvent(false);
        }
        private void StartFileSystemWatcher()
        {
            var dir = new DirectoryInfo(_folderWatched.FolderPath);
            if (!dir.Exists) return;
            _fileSystemWatcher.Path = _folderWatched.FolderPath;
            var newFileEvent = new AutoResetEvent(false);

            _fileSystemWatcher.Created += (senderObj, fileSysArgs) =>
                    FileSWatch_Created(senderObj, fileSysArgs,
                        newFileEvent);
            _fileSystemWatcher.EnableRaisingEvents = true;

            if (!new DirectoryInfo(Path.Combine(_folderWatched.FolderPath, "scanned")).Exists)
            {
                Directory.CreateDirectory(Path.Combine(_folderWatched.FolderPath, "scanned"));
            }
            if (!new DirectoryInfo(Path.Combine(_folderWatched.FolderPath, "error")).Exists)
            {
                Directory.CreateDirectory(Path.Combine(_folderWatched.FolderPath, "error"));
            }
            var workThread = new Thread(() => WorkProcedure(
                _fileSystemWatcher.Path, Path.Combine(_folderWatched.FolderPath, "scanned"),
                Path.Combine(_folderWatched.FolderPath, "error"), _stopEvent, newFileEvent));
            workThread.Start();
            _workerThreads.Add(workThread);
        }

        private void StartListeningForSettingsUpdate()
        {
            var workThread = new Thread(SubscribeToSettings);
            _workerThreads.Add(workThread);
            workThread.Start();
        }

        private void StartSendingStatuses()
        {
            var workThread = new Thread(SendStatus);
            _workerThreads.Add(workThread);
            workThread.Start();
        }

        private void SendStatus()
        {
            do
            {
                QueueClient client = QueueClient.Create(ClientSettingsQueue);
                var status = new ClientSetting
                {
                    NextFileTimeout = int.Parse(ConfigurationManager.AppSettings["NextPageTimeout"]),
                    CurrentAction = _scannerStatus,
                    ClientName = Environment.MachineName,
                    TimeStamp = DateTime.Now
                };
                var message = new BrokeredMessage(status);
                client.Send(message);
                client.Close();
                Thread.Sleep(30000);
            } while (WaitHandle.WaitAny(new WaitHandle[] { _stopEvent }, 1000) != 0);
        }

        private void SubscribeToSettings()
        {
            var client = SubscriptionClient.Create(ClientSettingstopic, ClientSettingsSubs);
            do
            {
                BrokeredMessage message;
                while ((message = client.Receive(TimeSpan.FromSeconds(5))) != null)
                {
                    if (_stopEvent.WaitOne(TimeSpan.Zero))
                        return;
                    var data = message.GetBody<ClientSetting>();
                    ConfigurationManager.AppSettings["NextPageTimeout"] = data.NextFileTimeout + "";                 
                    client.Complete(message.LockToken);
                }
            }
            while (WaitHandle.WaitAny(new WaitHandle[] { _stopEvent }, 1000) != 0);
            client.Close();
        }

        public void WorkProcedure(string inDir, string scannedDir, string errorDir, ManualResetEvent stopWorkEvent,
            AutoResetEvent newFileEvent)
        {
            do
            {
                _scannerStatus = "Waiting";
                while (Directory.EnumerateFileSystemEntries(inDir).Any())
                    foreach (var file in Directory.EnumerateFiles(inDir))
                    {
                        if (stopWorkEvent.WaitOne(TimeSpan.Zero))
                            return;

                        var inFile = file;
                        if (TryOpen(inFile, 3))
                        {
                            _scannerStatus = "Scanning";
                            ProcessScannedFile(inDir, scannedDir, errorDir, inFile);
                        }
                    }

            }
            while (WaitHandle.WaitAny(new WaitHandle[] { stopWorkEvent, newFileEvent }, 1000) != 0);
        }

        private void FileSWatch_Created(object sender, FileSystemEventArgs e, AutoResetEvent newFileEvent)
        {
            newFileEvent.Set();
        }

        private void PopulateListFileSystemWatchers()
        {
            var fileNameXml = ConfigurationManager.AppSettings["XMLFileFolderSettings"];

            XmlSerializer deserializer =
                new XmlSerializer(typeof(CustomFolderSettings));

            TextReader reader = new StreamReader(fileNameXml);

            object obj = deserializer.Deserialize(reader);
            reader.Close();

            _folderWatched = obj as CustomFolderSettings;
        }

        private void ProcessScannedFile(string inDir, string scannedDir, string errorDir, string fileName)
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


            if (MigraDocSample(inDir, scannedDir, docList, 3))
            {
                var queueClient = QueueClient.Create(PdfDocQueue);
                foreach (var file in docList)
                {
                    if (TryOpen(file, 3))
                    {                        
                        File.Delete(file);
                    }
                }
                var docPath = Path.Combine(scannedDir, Path.GetFileNameWithoutExtension(docList[0]) + ".pdf");
                var pdfDocMessage = new PdfDocument()
                {
                    Name = Path.GetFileNameWithoutExtension(docList[0]) + ".pdf",
                    PdfDocumentBlob = File.ReadAllBytes(docPath)
                };

                var message = new BrokeredMessage(pdfDocMessage);
                queueClient.Send(message);
                queueClient.Close();
            }
            else
            {
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


        public bool MigraDocSample(string indir, string outdir, List<string> files, int tryCount)
        {
            try
            {
                var document = new Document();
                var section = document.AddSection();
                var resultName = Path.GetFileNameWithoutExtension(files[0]);
                foreach (var file in files)
                {
                    _scannerStatus = "Saving to pdf";
                    if (document.Sections.Count != 1)
                        section.AddPageBreak();
                    var img = section.AddImage(file);
                    img.Height = document.DefaultPageSetup.PageHeight;
                    img.Width = document.DefaultPageSetup.PageWidth;
                }
                var render = new PdfDocumentRenderer {Document = document};

                render.RenderDocument();
                render.Save(Path.Combine(outdir, resultName + ".pdf"));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Start()
        {
            var nsManager = NamespaceManager.Create();
            if (!nsManager.QueueExists(PdfDocQueue))
                nsManager.CreateQueue(PdfDocQueue);
            if (!nsManager.QueueExists(ClientSettingsQueue))
                nsManager.CreateQueue(ClientSettingsQueue);
            if (!nsManager.TopicExists(ClientSettingstopic))
                nsManager.CreateTopic(ClientSettingstopic);
            PopulateListFileSystemWatchers();
            StartFileSystemWatcher();
            StartListeningForSettingsUpdate();
            StartSendingStatuses();
        }

        public void Stop()
        {
            _stopEvent.Set();
            _fileSystemWatcher.EnableRaisingEvents = false;
            foreach (var wt in _workerThreads)
            {
                wt.Join();
            }
        }
    }

}

