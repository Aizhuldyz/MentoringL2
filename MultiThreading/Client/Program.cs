using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChatAPI.Command;
using ChatAPI.Model;



namespace Client
{
    class Program
    {
        private static readonly Socket ServiceSocket = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly Socket MessageSocket = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private const int ServicePort = 5555;
        private const int MessagePort = 5556;
        private const string Name = "Ivan Ivanov";
        private static readonly List<string> MessageList = new List<string>
        {
            "hello!", "I think i know the answer!",
            "no kidding",
            "pfff", "dont call me"
        };
        private static SocketClient _client = new SocketClient
        {
            Name = Name + new Random().Next(0, 1200),
            Guid = Guid.Empty,
            Message = new List<string>()
        };
        private static readonly int BetweenMessagePauseTime = 10000;


        static void Main()
        {
            Console.Title = "Client " + _client.Name;
            ServiceSocket.BeginConnect(IPAddress.Loopback, ServicePort, ConnectServiceCallback, null);
            MessageSocket.BeginConnect(IPAddress.Loopback, MessagePort, ConnectMessageCallback, null);
            Task.Factory.StartNew(MessageWriteCallback);
            Task.Factory.StartNew(MessageReadCallback);
            Console.ReadLine();
            Exit();
        }



        private static void ConnectServiceCallback(IAsyncResult ar)
        {
            while (!ServiceSocket.Connected)
            {
                try
                {
                    ServiceSocket.EndConnect(ar);                   
                }
                catch (SocketException)
                {
                    Console.WriteLine("Connection was not established!");
                }
            }
            Console.WriteLine("ServiceSocket Connected");
            SocketCommands.Write(ServiceSocket, _client);
            while (_client.Guid == Guid.Empty)
            {
                var socketModel = SocketCommands.Read(ServiceSocket);
                _client.Guid = socketModel.Guid;
                foreach (var message in socketModel.Message)
                {
                    Console.WriteLine(socketModel.Name + ":" + message);
                }
            }
           
        }

        private static void ConnectMessageCallback(IAsyncResult ar)
        {
            while (!MessageSocket.Connected)
            {
                try
                {
                    MessageSocket.EndConnect(ar);
                    Console.WriteLine("MessageSocket Connected");
                }
                catch (SocketException)
                {
                    Console.WriteLine("Connection was not established!");
                }
                            
            }

        }

        private static void MessageWriteCallback()
        {
            while (true)
            {
                if (_client.Guid != Guid.Empty && MessageSocket.Connected)
                {
                    var socketMessage = new SocketClient
                    {
                        Name = _client.Name,
                        Guid = _client.Guid,
                        Message = new List<string>()
                    };
                    socketMessage.Message.Add(
                        MessageList.ElementAt(new Random().Next(0, MessageList.Count - 1)));
                    try
                    {
                        SocketCommands.Write(MessageSocket, socketMessage);
                    }
                    catch (SocketException)
                    {
                        Exit();
                    }
                    Thread.Sleep(BetweenMessagePauseTime);
                }
            }
        }

        private static void MessageReadCallback()
        {
            while (true)
            {
                if (_client.Guid != Guid.Empty && MessageSocket.Connected)
                {
                    
                    try
                    {
                        var socketModel = SocketCommands.Read(MessageSocket);
                        foreach (var message in socketModel.Message)
                        {
                            Console.WriteLine(socketModel.Name + ":" + message);
                        }
                        
                    }
                    catch (SocketException)
                    {
                        Exit();
                    }
                }
            }
        }


        /// <summary>
        /// Close socket and exit program.
        /// </summary>
        private static void Exit()
        {
            ServiceSocket.Shutdown(SocketShutdown.Both);
            ServiceSocket.Close();
            MessageSocket.Shutdown(SocketShutdown.Both);
            MessageSocket.Close();
            Environment.Exit(0);
        }

        
    }
}
