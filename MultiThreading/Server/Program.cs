using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ChatAPI.Model;

namespace Server
{
    class Program
    {

        static void Main()
        {
            var socketServer = new Server();
            socketServer.StartServer();
        }
    }
}
