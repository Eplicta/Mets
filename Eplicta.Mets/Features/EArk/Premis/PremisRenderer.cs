using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Eplicta.Mets.Helpers;

namespace Eplicta.Mets.Features.EArk.Premis;

/// <summary>
/// Renders a <see cref="PremisDocument"/> as PREMIS 3.0 XML, in the element order the schema requires.
/// </summary>
public class PremisRenderer
{
    private const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";

    private readonly PremisDocument _premisDocument;
    private XmlDocument _document;

    public PremisRenderer(PremisDocument premisDocument)
    {
        _premisDocument = premisDocument ?? throw new ArgumentNullException(nameof(premisDocument));
    }

    /// <summary>The PREMIS 3.0 schema the rendered document validates against.</summary>
    public static string Schema()
    {
        return Resource.Get(PremisConstants.SchemaResource);
    }

    public XmlDocument Render()
    {
        _document = new XmlDocument();
        _document.AppendChild(_document.CreateXmlDeclaration("1.0", "UTF-8", null));

        var root = Element("premis");
        root.SetAttribute($"xmlns:{PremisConstants.Prefix}", PremisConstants.Namespace);
        root.SetAttribute("xmlns:xsi", XsiNamespace);
        var schemaLocation = _document.CreateAttribute("xsi", "schemaLocation", XsiNamespace);
        schemaLocation.Value = $"{PremisConstants.Namespace} {PremisConstants.SchemasPathFromPreservationMetadata}/{PremisConstants.SchemaFileName}";
        root.Attributes.Append(schemaLocation);
        root.SetAttribute("version", PremisConstants.Version);
        _document.AppendChild(root);

        foreach (var file in _premisDocument.Files ?? [])
        {
            root.AppendChild(RenderFile(file));
        }

        foreach (var premisEvent in _premisDocument.Events ?? [])
        {
            root.AppendChild(RenderEvent(premisEvent));
        }

        foreach (var agent in _premisDocument.Agents ?? [])
        {
            root.AppendChild(RenderAgent(agent));
        }

        return XmlCharacters.Clean(_document);
    }

    private XmlElement RenderFile(PremisFile file)
    {
        var element = Element("object");
        var type = _document.CreateAttribute("xsi", "type", XsiNamespace);
        type.Value = $"{PremisConstants.Prefix}:file";
        element.Attributes.Append(type);

        element.AppendChild(Identifier("objectIdentifier", "objectIdentifier", file.Identifier));

        var characteristics = Element("objectCharacteristics");
        foreach (var fixity in file.Fixities ?? [])
        {
            var fixityElement = Element("fixity");
            fixityElement.AppendChild(Text("messageDigestAlgorithm", fixity.Algorithm));
            fixityElement.AppendChild(Text("messageDigest", fixity.Digest));
            Append(fixityElement, OptionalText("messageDigestOriginator", fixity.Originator));
            characteristics.AppendChild(fixityElement);
        }

        if (file.Size != null) characteristics.AppendChild(Text("size", file.Size.Value.ToString(CultureInfo.InvariantCulture)));
        Append(characteristics, RenderFormat(file));
        element.AppendChild(characteristics);

        Append(element, OptionalText("originalName", file.OriginalName));

        if (!string.IsNullOrEmpty(file.ContentLocation))
        {
            var location = Element("contentLocation");
            location.AppendChild(Text("contentLocationType", file.ContentLocationType));
            location.AppendChild(Text("contentLocationValue", file.ContentLocation));
            element.AppendChild(Wrap("storage", [location]));
        }

        foreach (var eventIdentifier in file.LinkingEventIdentifiers ?? [])
        {
            element.AppendChild(Identifier("linkingEventIdentifier", "linkingEventIdentifier", eventIdentifier));
        }

        return element;
    }

    private XmlElement RenderFormat(PremisFile file)
    {
        var hasName = !string.IsNullOrEmpty(file.FormatName);
        var hasKey = !string.IsNullOrEmpty(file.FormatRegistryKey);
        if (!hasName && !hasKey) return null;

        var format = Element("format");

        if (hasName)
        {
            var designation = Element("formatDesignation");
            designation.AppendChild(Text("formatName", file.FormatName));
            Append(designation, OptionalText("formatVersion", file.FormatVersion));
            format.AppendChild(designation);
        }

        if (hasKey)
        {
            var registry = Element("formatRegistry");
            registry.AppendChild(Text("formatRegistryName", file.FormatRegistryName));
            registry.AppendChild(Text("formatRegistryKey", file.FormatRegistryKey));
            format.AppendChild(registry);
        }

        return format;
    }

    private XmlElement RenderEvent(PremisEvent premisEvent)
    {
        var element = Element("event");
        element.AppendChild(Identifier("eventIdentifier", "eventIdentifier", premisEvent.Identifier));
        element.AppendChild(Text("eventType", premisEvent.EventType));
        element.AppendChild(Text("eventDateTime", EArkRenderer.ToUtc(premisEvent.DateTime)));

        if (!string.IsNullOrEmpty(premisEvent.Detail)) element.AppendChild(Wrap("eventDetailInformation", [Text("eventDetail", premisEvent.Detail)]));

        if (!string.IsNullOrEmpty(premisEvent.Outcome) || !string.IsNullOrEmpty(premisEvent.OutcomeDetailNote))
        {
            var outcome = Element("eventOutcomeInformation");
            Append(outcome, OptionalText("eventOutcome", premisEvent.Outcome));
            if (!string.IsNullOrEmpty(premisEvent.OutcomeDetailNote)) outcome.AppendChild(Wrap("eventOutcomeDetail", [Text("eventOutcomeDetailNote", premisEvent.OutcomeDetailNote)]));
            element.AppendChild(outcome);
        }

        foreach (var agent in premisEvent.LinkingAgents ?? [])
        {
            var linking = Identifier("linkingAgentIdentifier", "linkingAgentIdentifier", agent.Identifier);
            Append(linking, OptionalText("linkingAgentRole", agent.Role));
            element.AppendChild(linking);
        }

        foreach (var objectIdentifier in premisEvent.LinkingObjects ?? [])
        {
            element.AppendChild(Identifier("linkingObjectIdentifier", "linkingObjectIdentifier", objectIdentifier));
        }

        return element;
    }

    private XmlElement RenderAgent(PremisAgent agent)
    {
        var element = Element("agent");
        element.AppendChild(Identifier("agentIdentifier", "agentIdentifier", agent.Identifier));
        Append(element, OptionalText("agentName", agent.Name));
        Append(element, OptionalText("agentType", agent.Type));
        Append(element, OptionalText("agentVersion", agent.Version));

        foreach (var note in agent.Notes ?? [])
        {
            element.AppendChild(Text("agentNote", note));
        }

        return element;
    }

    private XmlElement Identifier(string name, string prefix, PremisIdentifier identifier)
    {
        var element = Element(name);
        element.AppendChild(Text($"{prefix}Type", identifier?.Type));
        element.AppendChild(Text($"{prefix}Value", identifier?.Value));
        return element;
    }

    private XmlElement Element(string name)
    {
        return _document.CreateElement(PremisConstants.Prefix, name, PremisConstants.Namespace);
    }

    private XmlElement Text(string name, string value)
    {
        var element = Element(name);
        element.InnerText = value ?? string.Empty;
        return element;
    }

    private XmlElement OptionalText(string name, string value)
    {
        return string.IsNullOrEmpty(value) ? null : Text(name, value);
    }

    private XmlElement Wrap(string name, IEnumerable<XmlElement> children)
    {
        var element = Element(name);
        foreach (var child in children)
        {
            element.AppendChild(child);
        }

        return element;
    }

    private static void Append(XmlElement parent, XmlElement child)
    {
        if (child != null) parent.AppendChild(child);
    }
}
