using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Eplicta.Mets.Entities;
using Eplicta.Mets.Helpers;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// Validates an ERMS document offline: against the embedded ERMS 3.0 schema, and against the co-occurrence rules
/// of the ERMS Schematron, which .NET cannot run as XSLT 2.0 and are therefore checked here.
/// </summary>
public class ErmsValidator
{
    private static readonly (string Attribute, string Value, string Other)[] NamedOtherRules =
    [
        ("aggregationType", "own_aggregation_definition", "otherAggregationType"),
        ("dateType", "other", "otherDateType"),
        ("directionDefinition", "other", "otherDirectionDefinition"),
        ("addressType", "other", "otherAddressLineType"),
        ("contactType", "other", "otherContactLineType"),
        ("agentType", "other", "otherAgentType"),
        ("restrictionType", "other_type", "otherRestrictionType"),
        ("relationType", "own_relation_definition", "otherRelationType"),
        ("classificationSchemaStatus", "other", "otherClassificationSchemaStatus")
    ];

    public IEnumerable<XmlValidatorResult> Validate(XmlDocument document)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));

        var results = new List<XmlValidatorResult>();

        if (document.DocumentElement is not { LocalName: "erms", NamespaceURI: ErmsConstants.Namespace })
        {
            results.Add(Error($"The document element is not erms in the namespace {ErmsConstants.Namespace}."));
        }

        try
        {
            var schemas = new XmlSchemaSet { XmlResolver = null };
            using (var reader = new StringReader(Resource.Get(ErmsConstants.SchemaResource)))
            using (var xmlReader = XmlReader.Create(reader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse, XmlResolver = null }))
            {
                schemas.Add(ErmsConstants.Namespace, xmlReader);
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

        results.AddRange(CoOccurrence(document));
        return results;
    }

    private static IEnumerable<XmlValidatorResult> CoOccurrence(XmlDocument document)
    {
        var elements = document.GetElementsByTagName("*").OfType<XmlElement>().Where(x => x.NamespaceURI == ErmsConstants.Namespace).ToArray();

        foreach (var element in elements)
        {
            foreach (var (attribute, value, other) in NamedOtherRules)
            {
                if (element.GetAttribute(attribute) == value && string.IsNullOrWhiteSpace(element.GetAttribute(other)))
                {
                    yield return Error($"{element.LocalName}: if {attribute} is '{value}', then {other} must be used.");
                }
            }

            if (element.LocalName == "disposalDate" && element.GetAttribute("dateType") == "other_date" && string.IsNullOrWhiteSpace(element.GetAttribute("otherDisposalDateType")))
            {
                yield return Error("disposalDate: if dateType is 'other_date', then otherDisposalDateType must be used.");
            }

            if (element.LocalName == "addressContactInformation" && !element.ChildNodes.OfType<XmlElement>().Any())
            {
                yield return Error("addressContactInformation must hold at least one addressLine or contactLine.");
            }
        }
    }

    private static XmlValidatorResult Error(string message)
    {
        return new XmlValidatorResult(message, XmlSeverityType.Error, new XmlSchemaException(message));
    }
}
