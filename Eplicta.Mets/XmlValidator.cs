using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Eplicta.Mets
{
    public class XmlValidator
    {
        public IEnumerable<string> Validate(XmlDocument document, XmlDocument schema)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            var s = new XmlSchemaSet();
            using var schemaStream = new StringReader(schema.OuterXml);
            var xmlns = document.FirstChild?.Attributes?["xmlns"];
            s.Add(xmlns?.InnerText, XmlReader.Create(schemaStream));

            using var documentStream = new StringReader(document.OuterXml);
            var rd = XmlReader.Create(documentStream);
            var doc = XDocument.Load(rd);
            doc.Validate(s, ValidationEventHandler);

            yield break;
        }

        static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            XmlSeverityType type = XmlSeverityType.Warning;
            if (Enum.TryParse<XmlSeverityType>("Error", out type))
            {
                if (type == XmlSeverityType.Error) throw new Exception(e.Message);
            }
        }
    }
}