using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messages
{
    [Serializable]
    public class PdfDocument
    {
        public string Name { get; set; }
        public byte[] PdfDocumentBlob { get; set; }
    }

    [Serializable]
    public class ClientSetting
    {
        public int NextFileTimeout { get; set; }
        public string CurrentAction { get; set; }
        public string ClientName { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
