using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAPI.Model
{
    [Serializable]
    public class SocketClient
    {
        public string Name { get; set; }
        public Guid Guid { get; set; }
        public DateTime ConnectionTime { get; set; }
        public List<string> Message { get; set; }
    }
}
