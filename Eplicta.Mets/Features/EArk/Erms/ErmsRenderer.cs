using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Eplicta.Mets.Helpers;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// Renders an <see cref="ErmsDocument"/> as an ERMS 3.0 XML document, in the element order the schema requires.
/// </summary>
public class ErmsRenderer
{
    private const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";
    private const string DateFormat = "yyyy-MM-dd";

    private readonly ErmsDocument _ermsDocument;
    private XmlDocument _document;

    public ErmsRenderer(ErmsDocument ermsDocument)
    {
        _ermsDocument = ermsDocument ?? throw new ArgumentNullException(nameof(ermsDocument));
    }

    /// <summary>The ERMS 3.0 schema (3.0.2) the rendered document validates against.</summary>
    public static string Schema()
    {
        return Resource.Get(ErmsConstants.SchemaResource);
    }

    /// <summary>The ERMS 3.0 Schematron rules that travel with the schema.</summary>
    public static string Schematron()
    {
        return Resource.Get(ErmsConstants.SchematronResource);
    }

    public XmlDocument Render()
    {
        var hasAggregations = _ermsDocument.Aggregations?.Any() == true;
        var hasRecords = _ermsDocument.Records?.Any() == true;
        if (hasAggregations == hasRecords) throw new InvalidOperationException("An ERMS document holds either aggregations or records, and exactly one of the two.");

        _document = new XmlDocument();
        _document.AppendChild(_document.CreateXmlDeclaration("1.0", "UTF-8", null));
        _document.AppendChild(_document.CreateProcessingInstruction("xml-model", $"href=\"{ErmsConstants.SchemasPathFromRepresentationData}/{ErmsConstants.SchematronFileName}\" type=\"application/xml\" schematypens=\"{ErmsConstants.SchematronNamespace}\""));

        var root = Element("erms");
        root.SetAttribute("xmlns:xsi", XsiNamespace);
        var schemaLocation = _document.CreateAttribute("xsi", "schemaLocation", XsiNamespace);
        schemaLocation.Value = $"{ErmsConstants.Namespace} {ErmsConstants.SchemasPathFromRepresentationData}/{ErmsConstants.SchemaFileName}";
        root.Attributes.Append(schemaLocation);
        _document.AppendChild(root);

        root.AppendChild(RenderControl(_ermsDocument.Control ?? new ErmsControl()));
        root.AppendChild(hasAggregations ? Wrap("aggregations", _ermsDocument.Aggregations.Select(RenderAggregation)) : Wrap("records", _ermsDocument.Records.Select(RenderRecord)));
        Append(root, RenderAdditionalInformation(_ermsDocument.AdditionalInformation));

        return _document;
    }

    private XmlElement RenderControl(ErmsControl control)
    {
        var element = Element("control");

        foreach (var identification in control.Identifications ?? [])
        {
            element.AppendChild(TypedText("identification", "identificationType", identification.Type, identification.Value));
        }

        foreach (var schema in control.ClassificationSchemas ?? [])
        {
            element.AppendChild(RenderClassificationSchema(schema));
        }

        var maintenance = Element("maintenanceInformation");
        maintenance.AppendChild(Valued("maintenanceStatus", ErmsVocabulary.Term(control.MaintenanceStatus)));

        var agency = Element("maintenanceAgency");
        if (!string.IsNullOrEmpty(control.AgencyCode)) agency.AppendChild(TypedText("agencyCode", "type", control.AgencyCodeType ?? string.Empty, control.AgencyCode));
        foreach (var name in control.AgencyNames ?? [])
        {
            agency.AppendChild(Text("agencyName", name));
        }

        maintenance.AppendChild(agency);

        var history = Element("maintenanceHistory");
        foreach (var maintenanceEvent in control.MaintenanceEvents ?? [])
        {
            var eventElement = Element("maintenanceEvent");
            eventElement.AppendChild(Valued("eventType", ErmsVocabulary.Term(maintenanceEvent.EventType)));
            eventElement.AppendChild(Text("eventDateTime", Instant(maintenanceEvent.DateTime)));
            if (maintenanceEvent.Agent != null) eventElement.AppendChild(RenderAgent(maintenanceEvent.Agent));
            history.AppendChild(eventElement);
        }

        maintenance.AppendChild(history);
        element.AppendChild(maintenance);

        return element;
    }

    private XmlElement RenderClassificationSchema(ErmsClassificationSchema schema)
    {
        var element = Element("classificationSchema");
        Attribute(element, "classificationSchemaId", schema.Id);
        if (schema.Status != null) element.SetAttribute("classificationSchemaStatus", ErmsVocabulary.Term(schema.Status.Value));
        Attribute(element, "otherClassificationSchemaStatus", schema.OtherStatus);

        if (schema.UsedFrom != null)
        {
            var period = Element("useDatePeriod");
            period.AppendChild(Text("fromDate", Day(schema.UsedFrom.Value)));
            if (schema.UsedTo != null) period.AppendChild(Text("toDate", Day(schema.UsedTo.Value)));
            element.AppendChild(period);
        }

        if (schema.Description?.Any() == true) element.AppendChild(Wrap("textualDescriptionOfClassificationSchema", schema.Description.Select(x => Text("p", x))));

        return element;
    }

    private XmlElement RenderAggregation(ErmsAggregation aggregation)
    {
        var element = Element("aggregation");
        Attribute(element, "systemIdentifier", aggregation.SystemIdentifier);
        element.SetAttribute("aggregationType", ErmsVocabulary.Term(aggregation.AggregationType));
        Attribute(element, "otherAggregationType", aggregation.OtherAggregationType);
        Attribute(element, "classificationSchemaUsed", aggregation.ClassificationSchemaUsed);

        AppendIdentity(element, aggregation);
        AppendTitles(element, aggregation);
        AppendStatus(element, aggregation);
        AppendRelations(element, aggregation);
        Append(element, RenderAdditionalInformation(aggregation.AdditionalInformation));
        AppendRestrictions(element, aggregation);
        Append(element, RenderDisposal(aggregation.Disposal));
        AppendAgentsToNotes(element, aggregation);

        foreach (var child in aggregation.Aggregations ?? [])
        {
            element.AppendChild(RenderAggregation(child));
        }

        foreach (var record in aggregation.Records ?? [])
        {
            element.AppendChild(RenderRecord(record));
        }

        return element;
    }

    private XmlElement RenderRecord(ErmsRecord record)
    {
        var element = Element("record");
        Attribute(element, "systemIdentifier", record.SystemIdentifier);
        Attribute(element, "recordType", record.RecordType);
        Attribute(element, "classificationSchemaUsed", record.ClassificationSchemaUsed);

        AppendIdentity(element, record);
        AppendTitles(element, record);
        AppendStatus(element, record);
        if (record.RunningNumber != null) element.AppendChild(Text("runningNumber", record.RunningNumber.Value.ToString(CultureInfo.InvariantCulture)));
        AppendRelations(element, record);
        AppendRestrictions(element, record);
        Append(element, RenderDisposal(record.Disposal));

        if (record.Direction != null)
        {
            var direction = Element("direction");
            direction.SetAttribute("directionDefinition", ErmsVocabulary.Term(record.Direction.Definition));
            Attribute(direction, "otherDirectionDefinition", record.Direction.OtherDefinition);
            element.AppendChild(direction);
        }

        AppendAgentsToNotes(element, record);
        Append(element, RenderAdditionalInformation(record.AdditionalInformation));

        return element;
    }

    private void AppendIdentity(XmlElement element, ErmsEntity entity)
    {
        Append(element, OptionalText("objectId", entity.ObjectId));

        foreach (var extraId in entity.ExtraIds ?? [])
        {
            element.AppendChild(TypedText("extraId", "extraIdType", extraId.Type, extraId.Value));
        }

        foreach (var classification in entity.Classifications ?? [])
        {
            var classificationElement = Text("classification", classification.Value);
            Attribute(classificationElement, "classificationId", classification.ClassificationId);
            Attribute(classificationElement, "classificationCode", classification.ClassificationCode);
            Attribute(classificationElement, "fullyQualifiedClassificationCode", classification.FullyQualifiedClassificationCode);
            element.AppendChild(classificationElement);
        }

        Append(element, OptionalText("parentAggregationId", entity.ParentAggregationId));

        if (entity.Keywords?.Any() == true) element.AppendChild(Wrap("keywords", entity.Keywords.Select(x => Text("keyword", x))));
    }

    private void AppendTitles(XmlElement element, ErmsEntity entity)
    {
        Append(element, OptionalText("title", entity.Title));

        foreach (var otherTitle in entity.OtherTitles ?? [])
        {
            element.AppendChild(TypedText("otherTitle", "titleType", otherTitle.TitleType, otherTitle.Value));
        }
    }

    private void AppendStatus(XmlElement element, ErmsEntity entity)
    {
        if (entity.Status != null) element.AppendChild(Valued("status", ErmsVocabulary.Term(entity.Status.Value)));
    }

    private void AppendRelations(XmlElement element, ErmsEntity entity)
    {
        foreach (var relation in entity.Relations ?? [])
        {
            var relationElement = Text("relation", relation.Value);
            relationElement.SetAttribute("relationType", ErmsVocabulary.Term(relation.RelationType));
            Attribute(relationElement, "otherRelationType", relation.OtherRelationType);
            element.AppendChild(relationElement);
        }
    }

    private void AppendRestrictions(XmlElement element, ErmsEntity entity)
    {
        foreach (var restriction in entity.Restrictions ?? [])
        {
            var restrictionElement = Element("restriction");
            restrictionElement.SetAttribute("restrictionType", ErmsVocabulary.Term(restriction.RestrictionType));
            Attribute(restrictionElement, "otherRestrictionType", restriction.OtherRestrictionType);
            Append(restrictionElement, OptionalText("explanatoryText", restriction.ExplanatoryText));
            restrictionElement.AppendChild(Text("regulation", restriction.Regulation));
            foreach (var agent in restriction.Agents ?? [])
            {
                restrictionElement.AppendChild(RenderAgent(agent));
            }

            Append(restrictionElement, RenderDates("dates", "date", restriction.Dates));
            element.AppendChild(restrictionElement);
        }
    }

    private void AppendAgentsToNotes(XmlElement element, ErmsEntity entity)
    {
        if (entity.Agents?.Any() == true) element.AppendChild(Wrap("agents", entity.Agents.Select(RenderAgent)));
        Append(element, OptionalText("description", entity.Description));
        Append(element, RenderDates("dates", "date", entity.Dates));

        foreach (var action in entity.Actions ?? [])
        {
            element.AppendChild(RenderAction(action));
        }

        Append(element, RenderNotes(entity.Notes));
    }

    private XmlElement RenderDisposal(ErmsDisposal disposal)
    {
        if (disposal == null) return null;

        var element = Element("disposal");
        element.SetAttribute("disposable", Boolean(disposal.Disposable));
        Append(element, OptionalText("disposalScheduleId", disposal.DisposalScheduleId));
        Append(element, OptionalText("disposalAction", disposal.DisposalAction));
        Append(element, OptionalText("disposalPeriod", disposal.DisposalPeriod));
        Append(element, OptionalText("disposalMandate", disposal.DisposalMandate));
        Append(element, OptionalText("disposalDescription", disposal.DisposalDescription));

        var dates = Element("dates");
        if (disposal.Date != null)
        {
            var date = Text("disposalDate", Instant(disposal.Date.Value));
            date.SetAttribute("dateType", ErmsVocabulary.Term(disposal.Date.DateType));
            Attribute(date, "otherDisposalDateType", disposal.Date.OtherDateType);
            dates.AppendChild(date);
        }

        element.AppendChild(dates);
        return element;
    }

    private XmlElement RenderAction(ErmsAction action)
    {
        var element = Element("action");
        element.AppendChild(Text("actionText", action.ActionText));
        Append(element, OptionalText("actionDue", action.ActionDue));
        Append(element, OptionalText("actionMotivation", action.ActionMotivation));
        Append(element, OptionalText("actionType", action.ActionType));
        Append(element, RenderDates("dates", "actionDate", action.Dates));
        if (action.Agents?.Any() == true) element.AppendChild(Wrap("agents", action.Agents.Select(RenderAgent)));
        return element;
    }

    private XmlElement RenderAgent(ErmsAgent agent)
    {
        var element = Element("agent");
        element.SetAttribute("agentType", ErmsVocabulary.Term(agent.AgentType));
        Attribute(element, "otherAgentType", agent.OtherAgentType);

        element.AppendChild(Text("name", agent.Name));
        Append(element, OptionalText("organisation", agent.Organisation));
        Append(element, OptionalText("unitName", agent.UnitName));

        if (!string.IsNullOrEmpty(agent.IdNumber))
        {
            var idNumber = Text("idNumber", agent.IdNumber);
            Attribute(idNumber, "idNumberType", agent.IdNumberType);
            element.AppendChild(idNumber);
        }

        Append(element, OptionalText("role", agent.Role));

        var lines = (agent.AddressLines ?? []).Select(RenderAddressLine).Concat((agent.ContactLines ?? []).Select(RenderContactLine)).ToArray();
        if (lines.Any()) element.AppendChild(Wrap("addressContactInformation", lines));

        if (agent.ProtectedIdentity != null) element.AppendChild(Text("protectedIdentity", Boolean(agent.ProtectedIdentity.Value)));
        Append(element, RenderNotes(agent.Notes));

        return element;
    }

    private XmlElement RenderAddressLine(ErmsAddressLine line)
    {
        var element = Text("addressLine", line.Value);
        element.SetAttribute("addressType", ErmsVocabulary.Term(line.AddressType));
        Attribute(element, "otherAddressLineType", line.OtherAddressType);
        return element;
    }

    private XmlElement RenderContactLine(ErmsContactLine line)
    {
        var element = Text("contactLine", line.Value);
        element.SetAttribute("contactType", ErmsVocabulary.Term(line.ContactType));
        Attribute(element, "otherContactLineType", line.OtherContactType);
        return element;
    }

    private XmlElement RenderNotes(IReadOnlyList<ErmsNote> notes)
    {
        if (notes?.Any() != true) return null;

        return Wrap("notes", notes.Select(note =>
        {
            var element = Text("note", note.Value);
            Attribute(element, "noteType", note.NoteType);
            if (note.NoteDate != null) element.SetAttribute("noteDate", Instant(note.NoteDate.Value));
            return element;
        }));
    }

    private XmlElement RenderDates(string wrapperName, string dateName, IReadOnlyList<ErmsDate> dates)
    {
        if (dates?.Any() != true) return null;

        return Wrap(wrapperName, dates.Select(date =>
        {
            var element = Text(dateName, Instant(date.Value));
            element.SetAttribute("dateType", ErmsVocabulary.Term(date.DateType));
            Attribute(element, "otherDateType", date.OtherDateType);
            return element;
        }));
    }

    private XmlElement RenderAdditionalInformation(ErmsAdditionalInformation information)
    {
        if (information == null || (information.Appendices?.Any() != true && information.OwnElements?.Any() != true)) return null;

        var element = Element("additionalInformation");

        foreach (var appendix in information.Appendices ?? [])
        {
            var appendixElement = Element("appendix");
            if (appendix.Disposable != null) appendixElement.SetAttribute("disposable", Boolean(appendix.Disposable.Value));
            appendixElement.SetAttribute("name", appendix.Name ?? string.Empty);
            Attribute(appendixElement, "description", appendix.Description);
            Attribute(appendixElement, "fileFormat", appendix.FileFormat);
            Attribute(appendixElement, "originalFileFormat", appendix.OriginalFileFormat);
            appendixElement.SetAttribute("path", appendix.Path ?? string.Empty);
            element.AppendChild(appendixElement);
        }

        foreach (var set in information.OwnElements ?? [])
        {
            var setElement = Element("ownElement");
            Append(setElement, OptionalText("ownElementDescription", set.Description));
            foreach (var ownElement in set.Elements ?? [])
            {
                setElement.AppendChild(RenderOwnElement(ownElement));
            }

            element.AppendChild(setElement);
        }

        return element;
    }

    private XmlElement RenderOwnElement(ErmsOwnElement ownElement)
    {
        var element = Element("ownElement");
        element.SetAttribute("name", ownElement.Name ?? string.Empty);
        Attribute(element, "dataType", ownElement.DataType);
        Attribute(element, "format", ownElement.Format);
        Append(element, OptionalText("value", ownElement.Value));

        if (ownElement.Attributes?.Any() == true)
        {
            element.AppendChild(Wrap("property", ownElement.Attributes.Select(attribute =>
            {
                var attributeElement = Element("attribute");
                attributeElement.SetAttribute("name", attribute.Name ?? string.Empty);
                Attribute(attributeElement, "dataType", attribute.DataType);
                Attribute(attributeElement, "format", attribute.Format);
                Append(attributeElement, OptionalText("value", attribute.Value));
                return attributeElement;
            })));
        }

        foreach (var child in ownElement.Children ?? [])
        {
            element.AppendChild(RenderOwnElement(child));
        }

        return element;
    }

    private XmlElement Element(string name)
    {
        return _document.CreateElement(name, ErmsConstants.Namespace);
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

    private XmlElement TypedText(string name, string typeAttribute, string type, string value)
    {
        var element = Text(name, value);
        element.SetAttribute(typeAttribute, type ?? string.Empty);
        return element;
    }

    private XmlElement Valued(string name, string value)
    {
        var element = Element(name);
        element.SetAttribute("value", value);
        return element;
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

    private static void Attribute(XmlElement element, string name, string value)
    {
        if (!string.IsNullOrEmpty(value)) element.SetAttribute(name, value);
    }

    private static string Instant(DateTime value)
    {
        return EArkRenderer.ToUtc(value);
    }

    private static string Day(DateTime value)
    {
        return value.ToString(DateFormat, CultureInfo.InvariantCulture);
    }

    private static string Boolean(bool value)
    {
        return value ? "true" : "false";
    }
}
