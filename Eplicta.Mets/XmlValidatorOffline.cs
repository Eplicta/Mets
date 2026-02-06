using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Eplicta.Mets.Entities;
using Eplicta.Mets.Helpers;

namespace Eplicta.Mets;

public class XmlValidatorOffline
{
    public IEnumerable<XmlValidatorResult> Validate(XmlDocument document, XmlDocument modsSchema, MetsSchema metsSchema = null)
    {
        metsSchema ??= MetsSchema.Default;
        if (document == null) throw new ArgumentNullException(nameof(document));
        if (modsSchema == null) throw new ArgumentNullException(nameof(modsSchema));

        var results = new List<XmlValidatorResult>();

        try
        {
            var schemas = new XmlSchemaSet();

            AddSchemaXml(schemas, modsSchema.OuterXml);

            LoadSchema(schemas, "xmldsig-core-schema.xsd", "http://www.w3.org/2000/09/xmldsig#");
            LoadSchema(schemas, "xml.xsd", "http://www.w3.org/XML/1998/namespace");
            LoadSchema(schemas, "xlink.xsd", "http://www.w3.org/1999/xlink");
            LoadSchema(schemas, metsSchema.Name, "http://www.loc.gov/METS/");

            schemas.Compile();

            using var sr = new StringReader(document.OuterXml);
            using var xr = XmlReader.Create(sr, new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit
            });

            var xdoc = XDocument.Load(xr, LoadOptions.SetLineInfo);

            xdoc.Validate(schemas, (o, e) => { results.Add(new XmlValidatorResult(e.Message, e.Severity, e.Exception)); }, true);
        }
        catch (Exception e) when (e is XmlException || e is XmlSchemaException)
        {
            var schemaEx = e as XmlSchemaException ?? new XmlSchemaException(e.Message, e);
            results.Add(new XmlValidatorResult(e.Message, XmlSeverityType.Error, schemaEx));
        }

        return results;
    }

    private static void AddSchemaXml(XmlSchemaSet schemas, string xsdXml)
    {
        using var sr = new StringReader(xsdXml);
        using var xr = XmlReader.Create(sr, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Parse,
            XmlResolver = null
        });

        schemas.Add(null, xr);
    }

    private static void LoadSchema(XmlSchemaSet schemas, string name, string schemaNamespace)
    {
        using var sr = new StringReader(Resource.Get(name));
        using var xr = XmlReader.Create(sr, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Parse,
            XmlResolver = null
        });

        schemas.Add(schemaNamespace, xr);
    }
}