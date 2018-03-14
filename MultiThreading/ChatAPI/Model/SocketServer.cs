using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatAPI.Model
{
    public class SocketServer
    {
        public Socket Socket { get; set; }
        public SocketClient SocketClient { get; set; }
    }
}
