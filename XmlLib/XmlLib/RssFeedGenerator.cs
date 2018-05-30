using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace XmlLib
{
    public class RssFeedGenerator
    {
        private readonly string _xsltSchema;

        public RssFeedGenerator(string xsltSchema)
        {
            _xsltSchema = xsltSchema;
        }

        public bool GenerateRssFeed(string inputXml, string outputXml)
        {

            try
            {
                var xslSettings = new XsltSettings();

                var xsl = new XslCompiledTransform();
                xsl.Load(_xsltSchema, xslSettings, null);

                using (var stream = new FileStream(outputXml, FileMode.Create, FileAccess.Write))
                {
                    xsl.Transform(inputXml, null, stream);
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

    }
}
