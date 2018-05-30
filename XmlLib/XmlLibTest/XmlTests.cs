using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XmlLib;
using System.Xml.Xsl;

namespace XmlLibTest
{
    [TestClass]
    public class XmlTests
    {
        private SchemaValidator _schemaValidator;

        [TestInitialize]
        public void Init()
        {
            var xsdSchema = @"..\..\Xsds\books.xsd";
            _schemaValidator = new SchemaValidator("http://library.by/catalog", xsdSchema);
        }

        [TestMethod]
        public void WhenXmlIsValidShouldReturnNoError()
        {
            var errors = _schemaValidator.Validate(@"..\..\Sources\books.xml");
            Assert.AreEqual(errors.Count, 0);
        }

        [TestMethod]
        public void WhenXmlIsInValidShouldReturnError()
        {
            var errors = _schemaValidator.Validate(@"..\..\InvalidXmls\Books1.xml");
            Assert.AreEqual(errors.Count, 2);
        }

        [TestMethod]
        public void XslCompiledTransform()
        {
            var rssFeedGenerator = new RssFeedGenerator(@"..\..\Xslts\BooksRssFeed.xslt");
            var result = rssFeedGenerator.GenerateRssFeed(@"..\..\Sources\books.xml", @"..\..\Generated\booksrss.xml");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void HtmlGeneratedReport()
        {
            var htmlGenerator = new HtmlReportGenerator(@"..\..\Xslts\BooksHtmlReport.xslt");
            var args = new XsltArgumentList();
            args.AddExtensionObject("http://library.by/ext", new XsltExtensions());
            var result = htmlGenerator.CreateReport(@"..\..\Sources\books.xml", args, @"..\..\Generated\books.html");
            Assert.IsTrue(result);
        }

    }
}
