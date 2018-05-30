using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace XmlLib
{
    public class SchemaValidator
    {
        private readonly XmlReaderSettings _readerSettings = new XmlReaderSettings();
        private readonly IList<string> _errors = new List<string>();
        public SchemaValidator(string targetNamespace, string xmlSchema)
        {

            _readerSettings.Schemas.Add(targetNamespace, xmlSchema);
            _readerSettings.ValidationEventHandler +=
                delegate (object sender, ValidationEventArgs e)
                {
                    var error =
                        $"Line number: {e.Exception.LineNumber}, Line position: {e.Exception.LinePosition}, Error: {e.Message}";
                    _errors.Add(error);
                };

            _readerSettings.ValidationFlags = _readerSettings.ValidationFlags | XmlSchemaValidationFlags.ReportValidationWarnings;
            _readerSettings.ValidationType = ValidationType.Schema;
        }

        public IList<string> Validate(string xml)
        {
            _errors.Clear();
            var reader = XmlReader.Create(xml, _readerSettings);
            while (reader.Read())
            {
            }
            return _errors;
        }

    }

}
