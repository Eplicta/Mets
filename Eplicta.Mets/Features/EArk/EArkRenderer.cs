using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Eplicta.Mets.Entities;

namespace Eplicta.Mets.Features.EArk;

/// <summary>
/// Renders an <see cref="EArkPackage"/> as the METS document of an E-ARK CSIP/SIP 2.1.0 information package.
/// </summary>
public class EArkRenderer
{
    private readonly EArkPackage _package;

    public EArkRenderer(EArkPackage package)
    {
        _package = package ?? throw new ArgumentNullException(nameof(package));
    }

    public XmlDocument Render()
    {
        var document = new XmlDocument();
        document.AppendChild(document.CreateXmlDeclaration("1.0", "UTF-8", null));

        var root = document.CreateElement("mets", EArkConstants.MetsNamespace);
        document.AppendChild(root);

        DeclareNamespaces(root);
        WriteRootAttributes(root);

        root.AppendChild(RenderHeader(document));

        foreach (var reference in _package.DescriptiveMetadata)
        {
            root.AppendChild(RenderMetadataSection(document, "dmdSec", reference));
        }

        if (_package.PreservationMetadata.Any())
        {
            var amdSec = document.CreateElement("amdSec", EArkConstants.MetsNamespace);
            foreach (var reference in _package.PreservationMetadata)
            {
                amdSec.AppendChild(RenderMetadataSection(document, "digiprovMD", reference));
            }

            root.AppendChild(amdSec);
        }

        var fileGroups = RenderFileSection(document, out var fileSection);
        if (fileSection != null) root.AppendChild(fileSection);

        root.AppendChild(RenderStructuralMap(document, fileGroups));

        return document;
    }

    internal static string ToUtc(DateTime value)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        return utc.ToString(EArkConstants.Iso8601UtcFormat);
    }

    internal static string ToChecksumType(MetsData.EChecksumType checksumType)
    {
        return checksumType.ToString().ToUpperInvariant().Replace("_", "-");
    }

    private static void SetPrefixed(XmlElement element, string prefix, string localName, string namespaceUri, string value)
    {
        var attribute = element.OwnerDocument!.CreateAttribute(prefix, localName, namespaceUri);
        attribute.Value = value;
        element.Attributes.Append(attribute);
    }

    private static void SetCsip(XmlElement element, string localName, string value)
    {
        SetPrefixed(element, EArkConstants.CsipPrefix, localName, EArkConstants.CsipNamespace, value);
    }

    private static void SetXlink(XmlElement element, string localName, string value)
    {
        SetPrefixed(element, EArkConstants.XlinkPrefix, localName, EArkConstants.XlinkNamespace, value);
    }

    private void DeclareNamespaces(XmlElement root)
    {
        root.SetAttribute($"xmlns:{EArkConstants.XsiPrefix}", EArkConstants.XsiNamespace);
        root.SetAttribute($"xmlns:{EArkConstants.CsipPrefix}", EArkConstants.CsipNamespace);
        root.SetAttribute($"xmlns:{EArkConstants.SipPrefix}", EArkConstants.SipNamespace);
        root.SetAttribute($"xmlns:{EArkConstants.XlinkPrefix}", EArkConstants.XlinkNamespace);

        var schemaLocation = string.Join(" ",
            EArkConstants.MetsNamespace, $"{EArkConstants.SchemasFolderName}/mets.xsd",
            EArkConstants.CsipNamespace, $"{EArkConstants.SchemasFolderName}/DILCISExtensionMETS.xsd",
            EArkConstants.SipNamespace, $"{EArkConstants.SchemasFolderName}/DILCISExtensionSIPMETS.xsd",
            EArkConstants.XlinkNamespace, $"{EArkConstants.SchemasFolderName}/xlink.xsd");

        SetPrefixed(root, EArkConstants.XsiPrefix, "schemaLocation", EArkConstants.XsiNamespace, schemaLocation);
    }

    private void WriteRootAttributes(XmlElement root)
    {
        root.SetAttribute("OBJID", _package.ObjId);
        if (!string.IsNullOrEmpty(_package.Label)) root.SetAttribute("LABEL", _package.Label);
        root.SetAttribute("TYPE", _package.ContentCategory);
        if (!string.IsNullOrEmpty(_package.OtherContentCategory)) SetCsip(root, "OTHERTYPE", _package.OtherContentCategory);
        root.SetAttribute("PROFILE", EArkConstants.SipProfile);
        SetCsip(root, "CONTENTINFORMATIONTYPE", _package.ContentInformationType.ToString());
        if (!string.IsNullOrEmpty(_package.OtherContentInformationType)) SetCsip(root, "OTHERCONTENTINFORMATIONTYPE", _package.OtherContentInformationType);
    }

    private XmlElement RenderHeader(XmlDocument document)
    {
        var header = document.CreateElement("metsHdr", EArkConstants.MetsNamespace);
        header.SetAttribute("CREATEDATE", ToUtc(_package.CreateDate));
        if (_package.LastModDate != null) header.SetAttribute("LASTMODDATE", ToUtc(_package.LastModDate.Value));
        header.SetAttribute("RECORDSTATUS", _package.RecordStatus.ToString().ToUpperInvariant());
        SetCsip(header, "OAISPACKAGETYPE", _package.OaisPackageType.ToString());

        foreach (var agent in _package.Agents)
        {
            header.AppendChild(RenderAgent(document, agent));
        }

        foreach (var altRecordId in _package.AltRecordIds)
        {
            var element = document.CreateElement("altRecordID", EArkConstants.MetsNamespace);
            element.SetAttribute("TYPE", altRecordId.Type.ToString().ToUpperInvariant());
            element.InnerText = altRecordId.Value;
            header.AppendChild(element);
        }

        return header;
    }

    private static XmlElement RenderAgent(XmlDocument document, EArkAgent agent)
    {
        var element = document.CreateElement("agent", EArkConstants.MetsNamespace);
        element.SetAttribute("ROLE", agent.Role.ToString().ToUpperInvariant());
        if (!string.IsNullOrEmpty(agent.OtherRole)) element.SetAttribute("OTHERROLE", agent.OtherRole);
        element.SetAttribute("TYPE", agent.Type.ToString().ToUpperInvariant());
        if (!string.IsNullOrEmpty(agent.OtherType)) element.SetAttribute("OTHERTYPE", agent.OtherType);

        var name = document.CreateElement("name", EArkConstants.MetsNamespace);
        name.InnerText = agent.Name;
        element.AppendChild(name);

        if (!string.IsNullOrEmpty(agent.Note))
        {
            var note = document.CreateElement("note", EArkConstants.MetsNamespace);
            note.InnerText = agent.Note;
            if (agent.NoteType != null) SetCsip(note, "NOTETYPE", ToNoteType(agent.NoteType.Value));
            element.AppendChild(note);
        }

        return element;
    }

    private static string ToNoteType(EArkAgent.ENoteType noteType)
    {
        return noteType == EArkAgent.ENoteType.SoftwareVersion ? EArkConstants.SoftwareVersionNoteType : EArkConstants.IdentificationCodeNoteType;
    }

    private static XmlElement RenderMetadataSection(XmlDocument document, string sectionName, EArkMetadataReference reference)
    {
        var section = document.CreateElement(sectionName, EArkConstants.MetsNamespace);
        section.SetAttribute("ID", reference.Id);
        if (sectionName == "dmdSec") section.SetAttribute("CREATED", ToUtc(reference.Created));
        section.SetAttribute("STATUS", reference.Status.ToString().ToUpperInvariant());

        var mdRef = document.CreateElement("mdRef", EArkConstants.MetsNamespace);
        mdRef.SetAttribute("LOCTYPE", EArkConstants.LocType);
        SetXlink(mdRef, "type", EArkConstants.XlinkSimple);
        SetXlink(mdRef, "href", reference.Href);
        mdRef.SetAttribute("MDTYPE", reference.MdType.ToString());
        mdRef.SetAttribute("MIMETYPE", reference.MimeType);
        mdRef.SetAttribute("SIZE", reference.Size.ToString());
        mdRef.SetAttribute("CREATED", ToUtc(reference.Created));
        if (!string.IsNullOrEmpty(reference.Checksum))
        {
            mdRef.SetAttribute("CHECKSUM", reference.Checksum);
            mdRef.SetAttribute("CHECKSUMTYPE", ToChecksumType(reference.ChecksumType));
        }

        section.AppendChild(mdRef);
        return section;
    }

    private Dictionary<string, string> RenderFileSection(XmlDocument document, out XmlElement fileSection)
    {
        var groups = new Dictionary<string, string>();
        var candidates = new (string Use, IReadOnlyList<EArkFile> Files)[]
        {
            (EArkConstants.DocumentationUse, _package.DocumentationFiles),
            (EArkConstants.SchemasUse, _package.SchemaFiles),
            (EArkConstants.RepresentationsUse, _package.RepresentationFiles)
        };

        fileSection = null;
        if (candidates.All(x => !x.Files.Any())) return groups;

        fileSection = document.CreateElement("fileSec", EArkConstants.MetsNamespace);
        fileSection.SetAttribute("ID", EArkPackageBuilder.NewId());

        foreach (var (use, files) in candidates.Where(x => x.Files.Any()))
        {
            var groupId = EArkPackageBuilder.NewId();
            groups[use] = groupId;

            var fileGrp = document.CreateElement("fileGrp", EArkConstants.MetsNamespace);
            fileGrp.SetAttribute("ID", groupId);
            fileGrp.SetAttribute("USE", use);
            if (use == EArkConstants.RepresentationsUse) SetCsip(fileGrp, "CONTENTINFORMATIONTYPE", _package.ContentInformationType.ToString());

            foreach (var file in files)
            {
                fileGrp.AppendChild(RenderFile(document, file));
            }

            fileSection.AppendChild(fileGrp);
        }

        return groups;
    }

    private static XmlElement RenderFile(XmlDocument document, EArkFile file)
    {
        var element = document.CreateElement("file", EArkConstants.MetsNamespace);
        element.SetAttribute("ID", file.Id);
        if (!string.IsNullOrEmpty(file.MimeType)) element.SetAttribute("MIMETYPE", file.MimeType);
        element.SetAttribute("SIZE", file.Size.ToString());
        element.SetAttribute("CREATED", ToUtc(file.Created));
        if (!string.IsNullOrEmpty(file.Checksum))
        {
            element.SetAttribute("CHECKSUM", file.Checksum);
            element.SetAttribute("CHECKSUMTYPE", ToChecksumType(file.ChecksumType));
        }

        var fLocat = document.CreateElement("FLocat", EArkConstants.MetsNamespace);
        fLocat.SetAttribute("LOCTYPE", EArkConstants.LocType);
        SetXlink(fLocat, "type", EArkConstants.XlinkSimple);
        SetXlink(fLocat, "href", file.Href);
        element.AppendChild(fLocat);

        return element;
    }

    private XmlElement RenderStructuralMap(XmlDocument document, IReadOnlyDictionary<string, string> fileGroups)
    {
        var structMap = document.CreateElement("structMap", EArkConstants.MetsNamespace);
        structMap.SetAttribute("ID", EArkPackageBuilder.NewId());
        structMap.SetAttribute("TYPE", EArkConstants.StructMapType);
        structMap.SetAttribute("LABEL", EArkConstants.StructMapLabel);

        var root = document.CreateElement("div", EArkConstants.MetsNamespace);
        root.SetAttribute("ID", EArkPackageBuilder.NewId());
        structMap.AppendChild(root);

        root.AppendChild(RenderMetadataDivision(document));

        foreach (var use in new[] { EArkConstants.DocumentationUse, EArkConstants.SchemasUse, EArkConstants.RepresentationsUse })
        {
            if (!fileGroups.TryGetValue(use, out var fileGrpId)) continue;

            var division = document.CreateElement("div", EArkConstants.MetsNamespace);
            division.SetAttribute("ID", EArkPackageBuilder.NewId());
            division.SetAttribute("LABEL", use);

            var pointer = document.CreateElement("fptr", EArkConstants.MetsNamespace);
            pointer.SetAttribute("FILEID", fileGrpId);
            division.AppendChild(pointer);

            root.AppendChild(division);
        }

        return structMap;
    }

    private XmlElement RenderMetadataDivision(XmlDocument document)
    {
        var division = document.CreateElement("div", EArkConstants.MetsNamespace);
        division.SetAttribute("ID", EArkPackageBuilder.NewId());
        division.SetAttribute("LABEL", EArkConstants.MetadataLabel);

        var descriptive = CurrentIds(_package.DescriptiveMetadata);
        if (!string.IsNullOrEmpty(descriptive)) division.SetAttribute("DMDID", descriptive);

        var preservation = CurrentIds(_package.PreservationMetadata);
        if (!string.IsNullOrEmpty(preservation)) division.SetAttribute("ADMID", preservation);

        return division;
    }

    private static string CurrentIds(IEnumerable<EArkMetadataReference> references)
    {
        return string.Join(" ", references.Where(x => x.Status == EArkMetadataReference.EStatus.Current).Select(x => x.Id));
    }
}
