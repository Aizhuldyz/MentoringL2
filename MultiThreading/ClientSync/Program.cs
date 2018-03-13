using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        private static Thread writeThread;
        private static Thread readThread;
        private static bool Stopped;
        private static Socket serviceSocket = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static Socket messageSocket = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private static readonly List<string> MessageList = new List<string>
        {
            "hello!", "I think i know the answer!",
            "no kidding"
        };
        static void Main(string[] args)
        {
            Console.Title = "Client";
            serviceSocket.Connect(IPAddress.Loopback, 5555);
            messageSocket.Connect(IPAddress.Loopback, 5556);
            Write(serviceSocket, "login");
            var s = Read(serviceSocket);
            if (s != "OK")
            {
                Console.WriteLine("Cannot login");
                Console.ReadLine();
                return;
            }
            writeThread = new Thread(WriteThread);
            writeThread.Start();
            readThread = new Thread(ReadThread);
            readThread.Start();
            Console.ReadLine();
            Stopped = true;
            readThread.Abort();
            writeThread.Abort();
        }

        private static void WriteThread()
        {
            while (!Stopped)
            {
                var index = new Random().Next(0, MessageList.Count -1);
                var message = MessageList.ElementAt(index);
                Write(messageSocket, message);
                Thread.Sleep(5000);
            }
        }

        private static void ReadThread()
        {
            while (!Stopped)
            {
                var s = Read(messageSocket);
                Console.WriteLine("Message: " + s);
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
