using System;
using System.Linq;
using System.Xml;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// Renders an <see cref="ErmsLogDocument"/> in Riksarkivet's ermsLogs format (SNAermsLogs.xsd).
/// </summary>
public class ErmsLogRenderer
{
    private const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";

    private readonly ErmsLogDocument _logDocument;
    private XmlDocument _document;

    public ErmsLogRenderer(ErmsLogDocument logDocument)
    {
        _logDocument = logDocument ?? throw new ArgumentNullException(nameof(logDocument));
    }

    public XmlDocument Render()
    {
        _document = new XmlDocument();
        _document.AppendChild(_document.CreateXmlDeclaration("1.0", "UTF-8", null));

        var root = Element("ermsLogs");
        root.SetAttribute("xmlns:xsi", XsiNamespace);
        var schemaLocation = _document.CreateAttribute("xsi", "schemaLocation", XsiNamespace);
        schemaLocation.Value = $"{ErmsConstants.LogNamespace} {ErmsConstants.SchemasPathFromRepresentationData}/{ErmsConstants.LogSchemaFileName}";
        root.Attributes.Append(schemaLocation);
        _document.AppendChild(root);

        var control = Element("control");
        var identification = Text("identification", _logDocument.Identification);
        identification.SetAttribute("type", _logDocument.IdentificationType ?? string.Empty);
        control.AppendChild(identification);
        control.AppendChild(Text("status", _logDocument.Status));
        control.AppendChild(Text("creationDate", EArkRenderer.ToUtc(_logDocument.CreationDate)));

        var creator = Element("creator");
        creator.AppendChild(Text("creatorName", _logDocument.CreatorName));
        if (!string.IsNullOrEmpty(_logDocument.CreatorCode))
        {
            var code = Text("creatorCode", _logDocument.CreatorCode);
            code.SetAttribute("type", ErmsVocabulary.Term(_logDocument.CreatorCodeType));
            creator.AppendChild(code);
        }

        if (!string.IsNullOrEmpty(_logDocument.CreatorDescription)) creator.AppendChild(Text("creatorDesc", _logDocument.CreatorDescription));
        control.AppendChild(creator);
        root.AppendChild(control);

        var logs = Element("logs");
        foreach (var caseLog in _logDocument.CaseLogs ?? [])
        {
            logs.AppendChild(RenderCaseLog(caseLog));
        }

        root.AppendChild(logs);
        return XmlCharacters.Clean(_document);
    }

    private XmlElement RenderCaseLog(ErmsCaseLog caseLog)
    {
        var element = Element("caseLog");
        element.SetAttribute("id", caseLog.Id ?? string.Empty);

        foreach (var modification in caseLog.Modifications ?? [])
        {
            var modificationElement = Element("modification");
            modificationElement.SetAttribute("number", modification.Number ?? string.Empty);
            modificationElement.AppendChild(Text("modDate", EArkRenderer.ToUtc(modification.Date)));
            modificationElement.AppendChild(Text("modAgent", modification.Agent));

            if (modification.CaseFields?.Any() == true)
            {
                var caseMod = Element("caseMod");
                foreach (var field in modification.CaseFields)
                {
                    caseMod.AppendChild(RenderField(field));
                }

                modificationElement.AppendChild(caseMod);
            }

            foreach (var recordChange in modification.RecordChanges ?? [])
            {
                var recordMod = Element("recordMod");
                recordMod.SetAttribute("id", recordChange.Id ?? string.Empty);
                recordMod.SetAttribute("name", recordChange.Name ?? string.Empty);
                if (!string.IsNullOrEmpty(recordChange.Summary)) recordMod.AppendChild(Text("modifications", recordChange.Summary));
                foreach (var field in recordChange.Fields ?? [])
                {
                    recordMod.AppendChild(RenderField(field));
                }

                modificationElement.AppendChild(recordMod);
            }

            element.AppendChild(modificationElement);
        }

        return element;
    }

    private XmlElement RenderField(ErmsLogField field)
    {
        var element = Element("modField");
        element.SetAttribute("name", field.Name ?? string.Empty);
        element.AppendChild(Text("originalValueField", field.OriginalValue));
        element.AppendChild(Text("newValueField", field.NewValue));
        return element;
    }

    private XmlElement Element(string name)
    {
        return _document.CreateElement(name, ErmsConstants.LogNamespace);
    }

    private XmlElement Text(string name, string value)
    {
        var element = Element(name);
        element.InnerText = value ?? string.Empty;
        return element;
    }
}
