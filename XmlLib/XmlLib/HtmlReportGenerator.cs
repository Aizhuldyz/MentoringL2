using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace XmlLib
{
    public class HtmlReportGenerator
    {
        private readonly string _xsltSchema;

        public HtmlReportGenerator(string xsltSchema)
        {
            _xsltSchema = xsltSchema;
        }

        public bool CreateReport(string inputXml, XsltArgumentList xsltArgumentList, string outputHtml)
        {
            

            try
            {
                var xslSettings = new XsltSettings { EnableScript = true };
                var xsl = new XslCompiledTransform();
                var xsltArgs = xsltArgumentList;
                xsl.Load(_xsltSchema, xslSettings, null);

                using (var stream = new FileStream(outputHtml, FileMode.Create, FileAccess.Write))
                {
                    xsl.Transform(inputXml, xsltArgs, stream);
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
