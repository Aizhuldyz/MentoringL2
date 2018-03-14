using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace ConsoleApplication2
{
    class Program
    {
        private static readonly Socket serviceSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly Socket messageSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly List<Socket> clientSockets = new List<Socket>();
        private const int readPort = 5555;
        private const int writePort = 5556;
        private static readonly int messageCount = 10;
        private static bool Stopped;
        private static Thread serviceThread;
        private static Thread messageThread;

        static void Main(string[] args)
        {
            Console.Title = "Server";
            serviceSocket.Bind(new IPEndPoint(IPAddress.Any, readPort));
            serviceSocket.Listen(10);
            messageSocket.Bind(new IPEndPoint(IPAddress.Any, writePort));
            messageSocket.Listen(10);
            serviceThread = new Thread(ServiceThread);
            serviceThread.Start();
            messageThread = new Thread(MessageAcceptThread);
            messageThread.Start();
            Console.ReadLine();
            Stopped = true;
            messageThread.Abort();
            serviceThread.Abort();
        }

        private static void ServiceThread()
        {
            while (!Stopped)
            {
                var socket = serviceSocket.Accept();
                var login = Read(socket);
                if (login == "login")
                {
                    Write(socket, "OK");

                }
                else
                {
                    Write(socket, "Access denied");
                }
            }
        }

        private static void MessageAcceptThread()
        {
            while (!Stopped)
            {
                var socket = messageSocket.Accept();
                clientSockets.Add(socket);
                new Thread(MessageThread).Start(socket);
            }
            
        }

        private static void MessageThread(Object obj)
        {
            var socket = (Socket) obj;
            while (!Stopped)
            {
                var message = Read(socket);
                foreach (var client in clientSockets)
                {
                    if (client != socket)
                    {
                        Write(client, message);
                    }

                }
            }
        }

        private static void Write(Socket socket, string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private static string Read(Socket socket)
        {
            byte[] buffer = new byte[10240];
            int received = socket.Receive(buffer, SocketFlags.None);
            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);
            return Encoding.UTF8.GetString(recBuf);
        }
    }
}
