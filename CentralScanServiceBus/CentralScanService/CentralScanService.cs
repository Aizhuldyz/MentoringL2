using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Messages;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace CentralScanService
{
    public class CentralScanService
    {
        private readonly List<Thread> _workerThreads;
        private readonly ManualResetEvent _stopEvent;
        private readonly FileSystemWatcher _settingsWatcher;
        private readonly string PdfDocQueue = "ScannedPdfQueue";
        private readonly string ClientSettingsQueue = "ClientSettingsQueue";
        private readonly string ClientSettingstopic = "ClientSettingsTopic";
        private readonly string ClientSettingsSubs = "ScannerSettings";

        public CentralScanService()
        {
            _workerThreads = new List<Thread>();
            _stopEvent = new ManualResetEvent(false);
            _settingsWatcher = new FileSystemWatcher();
        }

        private void SetupQueues()
        {
            var nsManager = NamespaceManager.Create();
            if (!nsManager.QueueExists(PdfDocQueue))
                nsManager.CreateQueue(PdfDocQueue);
            if (!nsManager.QueueExists(ClientSettingsQueue))
                nsManager.CreateQueue(ClientSettingsQueue);

            if (!nsManager.TopicExists(ClientSettingstopic))
                nsManager.CreateTopic(ClientSettingstopic);
            var subscription = new SubscriptionDescription(ClientSettingstopic, ClientSettingsSubs);
            if (!nsManager.SubscriptionExists(ClientSettingstopic, ClientSettingsSubs))
                nsManager.CreateSubscription(subscription);
        }

        private void SetupSettingsWatcher()
        {

            _settingsWatcher.Path = ConfigurationManager.AppSettings["SettingsPath"];
            _settingsWatcher.NotifyFilter = NotifyFilters.LastWrite;
            _settingsWatcher.Filter = "ClientSettings.xml";
            var fileChangeEvent = new AutoResetEvent(false);

            _settingsWatcher.Changed += (senderObj, fileSysArgs) =>
                SettingsChanged(senderObj, fileSysArgs,
                    fileChangeEvent);
            _settingsWatcher.EnableRaisingEvents = true;

            var workThread = new Thread(() => SendSettingsToSubs(fileChangeEvent));
            workThread.Start();
            _workerThreads.Add(workThread);

        }

        private void SendSettingsToSubs(AutoResetEvent fileChangeEvent)
        {
            do
            {
                if (fileChangeEvent.WaitOne())
                {
                    var fileNameXml = ConfigurationManager.AppSettings["SettingsFile"];
                    XmlSerializer deserializer =
                        new XmlSerializer(typeof(ClientSetting));

                    if (TryOpen(fileNameXml, 3))
                    {
                        TextReader reader = new StreamReader(fileNameXml);

                        object obj = deserializer.Deserialize(reader);
                        reader.Close();

                        var clientSetting = obj as ClientSetting;
                        var client = TopicClient.Create(ClientSettingstopic);
                        client.Send(new BrokeredMessage(clientSetting));
                        client.Close();
                    }
                }
            } while (WaitHandle.WaitAny(new WaitHandle[] {_stopEvent, fileChangeEvent}, 1000) != 0);
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

        private void SettingsChanged(object senderObj, FileSystemEventArgs fileSysArgs, AutoResetEvent fileChangeEvent)
        {
            fileChangeEvent.Set();
        }

        private void PdfDocListen(string saveLocation)
        {
            QueueClient client = QueueClient.Create(PdfDocQueue);
            do
            {
                BrokeredMessage message;
                while ((message = client.Receive(TimeSpan.FromSeconds(5))) != null)
                {
                    if (_stopEvent.WaitOne(TimeSpan.Zero))
                        return;
                    var data = message.GetBody<PdfDocument>();
                    var docData = Path.Combine(saveLocation, data.Name);
                    File.WriteAllBytes(docData, data.PdfDocumentBlob);
                    client.Complete(message.LockToken);
                }               
            }
            while (WaitHandle.WaitAny(new WaitHandle[] { _stopEvent }, 1000) != 0);
            client.Close();
        }

        private void ReceiveSettings()
        {           
            do
            {
                QueueClient client = QueueClient.Create(ClientSettingsQueue);
                BrokeredMessage message;
                while ((message = client.Receive(TimeSpan.FromSeconds(5))) != null)
                {
                    if (_stopEvent.WaitOne(TimeSpan.Zero))
                        return;
                    var data = message.GetBody<ClientSetting>();                    
                    client.Complete(message.LockToken);
                    UpdateSettingsXml(data);
                }
                client.Close();
            }
            while (WaitHandle.WaitAny(new WaitHandle[] { _stopEvent }, 1000) != 0);
        }

        private void UpdateSettingsXml(ClientSetting setting)
        {
            XmlSerializer deserializer =
                new XmlSerializer(typeof(ClientSetting));

            TextWriter writer = new StreamWriter(ConfigurationManager.AppSettings["SettingsReceived"]);

            deserializer.Serialize(writer, setting);
            writer.Close();         
        }

        private void SetupSettingsListener()
        {
            var workThread = new Thread(ReceiveSettings);
            workThread.Start();
            _workerThreads.Add(workThread);
        }


        public void Start()
        {
            SetupQueues();
            var pdfSaveLocation = ConfigurationManager.AppSettings["PdfSaveLocation"];
            var workThread = new Thread(() => PdfDocListen(pdfSaveLocation));
            workThread.Start();
            _workerThreads.Add(workThread);
            SetupSettingsWatcher();
            SetupSettingsListener();
        }
       
        
        public void Stop()
        {
            _stopEvent.Set();
            foreach (var wt in _workerThreads)
            {
                wt.Join();
            }
            _settingsWatcher.EnableRaisingEvents = false;
        }
    }
}
