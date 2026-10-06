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
/// Validates an additionalPackageInfo document against Riksarkivet's SNAadditionalPackageInfo schema.
/// </summary>
public class AdditionalPackageInfoValidator
{
    public IEnumerable<XmlValidatorResult> Validate(XmlDocument document)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));

        var results = new List<XmlValidatorResult>();

        try
        {
            var schemas = new XmlSchemaSet { XmlResolver = null };

            using (var reader = new StringReader(Resource.Get("EArk.SNAadditionalPackageInfo.xsd")))
            using (var xmlReader = XmlReader.Create(reader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse, XmlResolver = null }))
            {
                schemas.Add(EArkConstants.AdditionalPackageInfoNamespace, xmlReader);
            }

            schemas.Compile();

            using var documentReader = new StringReader(document.OuterXml);
            using var documentXmlReader = XmlReader.Create(documentReader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });

            XDocument.Load(documentXmlReader, LoadOptions.SetLineInfo).Validate(schemas, (_, e) => { results.Add(new XmlValidatorResult(e.Message, e.Severity, e.Exception)); }, true);
        }
        catch (Exception e) when (e is XmlException or XmlSchemaException)
        {
            results.Add(new XmlValidatorResult(e.Message, XmlSeverityType.Error, e as XmlSchemaException ?? new XmlSchemaException(e.Message, e)));
        }

        return results;
    }
}
