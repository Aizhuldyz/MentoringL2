using ChatAPI.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ChatAPI.Command
{
    public class SocketCommands
    {
        public static void Write(Socket socket, SocketClient message)
        {
            byte[] buffer = ObjectToByteArray(message);
            socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        public static SocketClient Read(Socket socket)
        {
            var buffer = new byte[2048];
            int received = socket.Receive(buffer, SocketFlags.None);

            var data = new byte[received];
            Array.Copy(buffer, data, received);
            var socketModel = (SocketClient)ByteArrayToObject(data);
            return socketModel;
        }


        public static byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);
            return obj;
        }
    }
}
