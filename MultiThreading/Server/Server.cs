﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ChatAPI.Model;
using ChatAPI.Command;

namespace Server
{
    public class Server
    {
        private static readonly Socket ServiceSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly Socket MessageSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly List<SocketServer> ClientSockets = new List<SocketServer>();
        private const int BufferSize = 2048;
        private const int ServicePort = 5555;
        private const int MessagePort = 5556;

        private const int MessageCount = 10;
        private static FixedSizedQueue<string> _chatHistoryFixedSizedQueue = new FixedSizedQueue<string>(MessageCount);


        public void StartServer()
        {
            Console.Title = "Server";
            SetupServer();
            Console.ReadLine();
            CloseAllSockets();
        }

        private static void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            ServiceSocket.Bind(new IPEndPoint(IPAddress.Any, ServicePort));
            ServiceSocket.Listen(10);
            MessageSocket.Bind(new IPEndPoint(IPAddress.Any, MessagePort));
            MessageSocket.Listen(10);
            ServiceSocket.BeginAccept(AcceptServiceCallback, null);
            MessageSocket.BeginAccept(AcceptMessageCallback, null);
            Console.WriteLine("Server setup complete");
        }


        private static void AcceptServiceCallback(IAsyncResult ar)
        {
            Socket socket;

            try
            {
                socket = ServiceSocket.EndAccept(ar);

            }
            catch (ObjectDisposedException)
            {
                return;
            }
            var clientSocket = SocketCommands.Read(socket);
            clientSocket.Message = _chatHistoryFixedSizedQueue.ToList();
            clientSocket.Guid = Guid.NewGuid();
            SocketCommands.Write(socket, clientSocket);
            ServiceSocket.BeginAccept(AcceptServiceCallback, null);
        }


        private static void AcceptMessageCallback(IAsyncResult ar)
        {
            Socket socket;

            try
            {
                socket = MessageSocket.EndAccept(ar);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            var socketServer = new SocketServer
            {
                Buffer = new byte[BufferSize],
                Socket = socket                
            };
            ClientSockets.Add(socketServer);
            socket.BeginReceive(socketServer.Buffer, 0, BufferSize, SocketFlags.None, ReceiveCallback, socketServer);
            MessageSocket.BeginAccept(AcceptMessageCallback, null);
        }


        private static void ReceiveCallback(IAsyncResult ar)
        {
            SocketServer current = (SocketServer)ar.AsyncState;
            int received;

            try
            {
                received = current.Socket.EndReceive(ar);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                current.Socket.Close();
                var index = ClientSockets.FindIndex(x => x.Socket == current.Socket);
                ClientSockets.RemoveAt(index);
                return;
            }
            var data = new byte[received];
            Array.Copy(current.Buffer, data, received);
            var socketModel = (SocketClient)SocketCommands.ByteArrayToObject(data);            
            foreach (var message in socketModel.Message)
            {
                _chatHistoryFixedSizedQueue.Enqueue(socketModel.Name + ": " + message);
            }
            foreach (var client in ClientSockets)
            {
                if (client.Socket != current.Socket)
                {
                    var message = SocketCommands.ObjectToByteArray(socketModel);
                    client.Socket.Send(message);
                }

            }
            current.Socket.BeginReceive(current.Buffer, 0, BufferSize, SocketFlags.None, ReceiveCallback, current);
        }



        /// <summary>
        /// Close all connected client (we do not need to shutdown the server socket as its connections
        /// are already closed with the clients).
        /// </summary>
        private static void CloseAllSockets()
        {
            foreach (var socket in ClientSockets)
            {
                socket.Socket.Shutdown(SocketShutdown.Both);
                socket.Socket.Close();
            }

            ServiceSocket.Close();
            MessageSocket.Close();
        }

    }
}
