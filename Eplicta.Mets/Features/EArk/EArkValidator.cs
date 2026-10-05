using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Eplicta.Mets.Entities;
using Eplicta.Mets.Helpers;

namespace Eplicta.Mets.Features.EArk;

/// <summary>
/// Validates an E-ARK METS document offline, against the METS 1.12, xlink and DILCIS extension schemas that
/// Riksarkivet's application of E-ARK CSIP and SIP names, all embedded in this package.
/// </summary>
public class EArkValidator
{
    public IEnumerable<XmlValidatorResult> Validate(XmlDocument document)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));

        var results = new List<XmlValidatorResult>();

        try
        {
            var schemas = new XmlSchemaSet { XmlResolver = null };

            LoadSchema(schemas, "xml.xsd", "http://www.w3.org/XML/1998/namespace");
            LoadSchema(schemas, "xlink.xsd", EArkConstants.XlinkNamespace);
            LoadSchema(schemas, "EArk.mets.xsd", EArkConstants.MetsNamespace);
            LoadSchema(schemas, "EArk.DILCISExtensionMETS.xsd", EArkConstants.CsipNamespace);
            LoadSchema(schemas, "EArk.DILCISExtensionSIPMETS.xsd", EArkConstants.SipNamespace);

            schemas.Compile();

            using var reader = new StringReader(document.OuterXml);
            using var xmlReader = XmlReader.Create(reader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });

            XDocument.Load(xmlReader, LoadOptions.SetLineInfo).Validate(schemas, (_, e) => { results.Add(new XmlValidatorResult(e.Message, e.Severity, e.Exception)); }, true);
        }
        catch (Exception e) when (e is XmlException or XmlSchemaException)
        {
            results.Add(new XmlValidatorResult(e.Message, XmlSeverityType.Error, e as XmlSchemaException ?? new XmlSchemaException(e.Message, e)));
        }

        return results;
    }

    private static void LoadSchema(XmlSchemaSet schemas, string name, string schemaNamespace)
    {
        using var reader = new StringReader(Resource.Get(name));
        using var xmlReader = XmlReader.Create(reader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse, XmlResolver = null });

        schemas.Add(schemaNamespace, xmlReader);
    }
}
