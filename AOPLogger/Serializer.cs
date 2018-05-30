using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace AOPLogger
{
    public interface ISerialize
    {
        string Serialize(Object obj);
    }

    public class Serializer : ISerialize
    {
        public string Serialize(Object obj)
        {
            try
            {
                var serializer = new NetDataContractSerializer();
                using (var stream = new MemoryStream())
                {
                    serializer.Serialize(stream, obj);
                    stream.Seek(0, SeekOrigin.Begin);
                    using (var streamreader = new StreamReader(stream))
                    {
                        return streamreader.ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            {
                return $"Not serializable [{obj.GetType()}]";
            }           

        }
    }
}
