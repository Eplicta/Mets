using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Eplicta.Mets.Entities;

namespace Eplicta.Mets.Features.EArk;

/// <summary>
/// Reads E-ARK METS documents as the exact inverse of <see cref="EArkRenderer"/>, and tolerates Riksarkivet's own examples.
/// </summary>
public class EArkReader : IEArkReader
{
    private const string ModsNamespace = "http://www.loc.gov/mods/v3";
    private const string ModsRootName = "mods";
    private const string MetsRootName = "mets";

    private static readonly XNamespace Mets = EArkConstants.MetsNamespace;
    private static readonly XNamespace Csip = EArkConstants.CsipNamespace;
    private static readonly XNamespace Xlink = EArkConstants.XlinkNamespace;
    private static readonly XName Mods = XName.Get(ModsRootName, ModsNamespace);
    private static readonly XmlSerializer ModsSerializer = new(typeof(DeserializedMets.DmdSecElement.MdWrapElement.XmlDataElement.ModsBaseElement), new XmlRootAttribute(ModsRootName) { Namespace = ModsNamespace });
    private static readonly char[] HrefSeparators = ['/', '\\'];

    private static readonly IReadOnlyDictionary<string, EArkPackage.EOaisPackageType> OaisPackageTypes = Lookup<EArkPackage.EOaisPackageType>(x => x.ToString());
    private static readonly IReadOnlyDictionary<string, EArkPackage.ERecordStatus> RecordStatuses = Lookup<EArkPackage.ERecordStatus>(x => x.ToString().ToUpperInvariant());
    private static readonly IReadOnlyDictionary<string, EArkPackage.EContentInformationType> ContentInformationTypes = Lookup<EArkPackage.EContentInformationType>(x => x.ToString());
    private static readonly IReadOnlyDictionary<string, EArkAgent.ERole> AgentRoles = Lookup<EArkAgent.ERole>(x => x.ToString().ToUpperInvariant());
    private static readonly IReadOnlyDictionary<string, EArkAgent.EType> AgentTypes = Lookup<EArkAgent.EType>(x => x.ToString().ToUpperInvariant());
    private static readonly IReadOnlyDictionary<string, EArkAgent.ENoteType> NoteTypes = Lookup<EArkAgent.ENoteType>(EArkRenderer.ToNoteType);
    private static readonly IReadOnlyDictionary<string, EArkAltRecordId.EType> AltRecordIdTypes = Lookup<EArkAltRecordId.EType>(x => x.ToString().ToUpperInvariant());
    private static readonly IReadOnlyDictionary<string, EArkMetadataReference.EMdType> MdTypes = Lookup<EArkMetadataReference.EMdType>(x => x.ToString());
    private static readonly IReadOnlyDictionary<string, EArkMetadataReference.EStatus> Statuses = Lookup<EArkMetadataReference.EStatus>(x => x.ToString().ToUpperInvariant());
    private static readonly IReadOnlyDictionary<string, MetsData.EChecksumType> ChecksumTypes = Lookup<MetsData.EChecksumType>(EArkRenderer.ToChecksumType);

    public EArkPackage Read(string metsXml)
    {
        var root = Parse(metsXml).Root!;
        if (root.Name != Mets + MetsRootName) throw new InvalidDataException($"The document is not a METS document. Expected root element '{{{EArkConstants.MetsNamespace}}}{MetsRootName}' but found '{root.Name}'.");

        var objId = (string)root.Attribute("OBJID");
        if (string.IsNullOrEmpty(objId)) throw new InvalidDataException("The METS document lacks the required root attribute OBJID.");

        var header = root.Element(Mets + "metsHdr");
        var (contentInformationType, otherContentInformationType) = ReadContentInformationType(root);
        var fileGroups = root.Elements(Mets + "fileSec").Elements(Mets + "fileGrp").ToArray();
        var representationFiles = ReadFiles(fileGroups, EArkConstants.RepresentationsUse);

        return new EArkPackage
        {
            ObjId = objId,
            Label = (string)root.Attribute("LABEL"),
            ContentCategory = (string)root.Attribute("TYPE"),
            OtherContentCategory = (string)root.Attribute(Csip + "OTHERTYPE"),
            ContentInformationType = contentInformationType,
            OtherContentInformationType = otherContentInformationType,
            OaisPackageType = ParseEnum(header?.Attribute(Csip + "OAISPACKAGETYPE"), OaisPackageTypes, EArkPackage.EOaisPackageType.SIP),
            RecordStatus = ParseEnum(header?.Attribute("RECORDSTATUS"), RecordStatuses, EArkPackage.ERecordStatus.New),
            CreateDate = ParseDate(header?.Attribute("CREATEDATE")) ?? default,
            LastModDate = ParseDate(header?.Attribute("LASTMODDATE")),
            RepresentationName = ReadRepresentationName(representationFiles) ?? EArkConstants.DefaultRepresentationName,
            Agents = header?.Elements(Mets + "agent").Select(ReadAgent).ToArray() ?? [],
            AltRecordIds = header?.Elements(Mets + "altRecordID").Select(ReadAltRecordId).ToArray() ?? [],
            DescriptiveMetadata = root.Elements(Mets + "dmdSec").Select(ReadMetadataReference).ToArray(),
            PreservationMetadata = root.Elements(Mets + "amdSec").Elements(Mets + "digiprovMD").Select(ReadMetadataReference).ToArray(),
            DocumentationFiles = ReadFiles(fileGroups, EArkConstants.DocumentationUse),
            SchemaFiles = ReadFiles(fileGroups, EArkConstants.SchemasUse),
            RepresentationFiles = representationFiles
        };
    }

    public DeserializedMets.DmdSecElement.MdWrapElement.XmlDataElement.ModsBaseElement ReadMods(string modsXml)
    {
        var document = Parse(modsXml);
        if (document.Root!.Name != Mods) throw new InvalidDataException($"The document is not a MODS document. Expected root element '{Mods}' but found '{document.Root.Name}'.");

        try
        {
            using var reader = document.CreateReader();
            return (DeserializedMets.DmdSecElement.MdWrapElement.XmlDataElement.ModsBaseElement)ModsSerializer.Deserialize(reader);
        }
        catch (InvalidOperationException e)
        {
            throw new InvalidDataException($"The MODS document could not be read. {e.InnerException?.Message ?? e.Message}", e);
        }
    }

    private static XDocument Parse(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml)) throw new InvalidDataException("The document is empty.");

        try
        {
            return XDocument.Parse(xml);
        }
        catch (XmlException e)
        {
            throw new InvalidDataException($"The document is not well-formed XML. {e.Message}", e);
        }
    }

    private static (EArkPackage.EContentInformationType Type, string Other) ReadContentInformationType(XElement root)
    {
        var value = (string)root.Attribute(Csip + "CONTENTINFORMATIONTYPE");
        var other = (string)root.Attribute(Csip + "OTHERCONTENTINFORMATIONTYPE");
        if (string.IsNullOrEmpty(value)) return (EArkPackage.EContentInformationType.OTHER, other);
        if (ContentInformationTypes.TryGetValue(Normalize(value), out var type)) return (type, other);

        return (EArkPackage.EContentInformationType.OTHER, other ?? value);
    }

    private static string ReadRepresentationName(IEnumerable<EArkFile> representationFiles)
    {
        var href = representationFiles.FirstOrDefault()?.Href;
        if (href == null) return null;

        var segments = href.Split(HrefSeparators, StringSplitOptions.RemoveEmptyEntries);
        var index = Array.FindIndex(segments, x => string.Equals(x, EArkConstants.RepresentationsFolderName, StringComparison.OrdinalIgnoreCase));
        return index >= 0 && index + 1 < segments.Length ? segments[index + 1] : null;
    }

    private static EArkAgent ReadAgent(XElement agent)
    {
        var note = agent.Element(Mets + "note");

        return new EArkAgent
        {
            Role = ParseOpenEnum(agent.Attribute("ROLE"), AgentRoles, EArkAgent.ERole.Other),
            OtherRole = (string)agent.Attribute("OTHERROLE"),
            Type = ParseOpenEnum(agent.Attribute("TYPE"), AgentTypes, EArkAgent.EType.Other),
            OtherType = (string)agent.Attribute("OTHERTYPE"),
            Name = (string)agent.Element(Mets + "name"),
            Note = (string)note,
            NoteType = ParseOptionalEnum(note?.Attribute(Csip + "NOTETYPE"), NoteTypes)
        };
    }

    private static EArkAltRecordId ReadAltRecordId(XElement altRecordId)
    {
        return new EArkAltRecordId
        {
            Type = ParseEnum(altRecordId.Attribute("TYPE"), AltRecordIdTypes, default),
            Value = altRecordId.Value
        };
    }

    private static EArkMetadataReference ReadMetadataReference(XElement section)
    {
        var mdRef = section.Element(Mets + "mdRef");

        return new EArkMetadataReference
        {
            Id = (string)section.Attribute("ID"),
            Status = ParseEnum(section.Attribute("STATUS"), Statuses, EArkMetadataReference.EStatus.Current),
            Href = (string)mdRef?.Attribute(Xlink + "href"),
            MdType = ParseOpenEnum(mdRef?.Attribute("MDTYPE"), MdTypes, EArkMetadataReference.EMdType.OTHER),
            MimeType = (string)mdRef?.Attribute("MIMETYPE"),
            Size = ParseSize(mdRef?.Attribute("SIZE")),
            Created = ParseDate(mdRef?.Attribute("CREATED")) ?? default,
            Checksum = (string)mdRef?.Attribute("CHECKSUM"),
            ChecksumType = ParseEnum(mdRef?.Attribute("CHECKSUMTYPE"), ChecksumTypes, MetsData.EChecksumType.SHA_256)
        };
    }

    private static EArkFile[] ReadFiles(IEnumerable<XElement> fileGroups, string use)
    {
        return fileGroups.Where(x => (string)x.Attribute("USE") == use).Elements(Mets + "file").Select(ReadFile).ToArray();
    }

    private static EArkFile ReadFile(XElement file)
    {
        return new EArkFile
        {
            Id = (string)file.Attribute("ID"),
            Href = (string)file.Element(Mets + "FLocat")?.Attribute(Xlink + "href"),
            MimeType = (string)file.Attribute("MIMETYPE"),
            Size = ParseSize(file.Attribute("SIZE")),
            Created = ParseDate(file.Attribute("CREATED")) ?? default,
            Checksum = (string)file.Attribute("CHECKSUM"),
            ChecksumType = ParseEnum(file.Attribute("CHECKSUMTYPE"), ChecksumTypes, MetsData.EChecksumType.SHA_256)
        };
    }

    private static DateTime? ParseDate(XAttribute attribute)
    {
        if (string.IsNullOrEmpty(attribute?.Value)) return null;
        if (DateTime.TryParse(attribute.Value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var value)) return value;

        throw new InvalidDataException($"The value '{attribute.Value}' of attribute {attribute.Name.LocalName} on element {attribute.Parent?.Name.LocalName} is not a valid date.");
    }

    private static long ParseSize(XAttribute attribute)
    {
        if (string.IsNullOrEmpty(attribute?.Value)) return 0;
        if (long.TryParse(attribute.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)) return value;

        throw new InvalidDataException($"The value '{attribute.Value}' of attribute {attribute.Name.LocalName} on element {attribute.Parent?.Name.LocalName} is not a valid size.");
    }

    private static TEnum ParseEnum<TEnum>(XAttribute attribute, IReadOnlyDictionary<string, TEnum> lookup, TEnum fallback) where TEnum : struct, Enum
    {
        return ParseOptionalEnum(attribute, lookup) ?? fallback;
    }

    // For vocabularies whose model ends in an OTHER escape, where a value outside the model is still valid METS.
    private static TEnum ParseOpenEnum<TEnum>(XAttribute attribute, IReadOnlyDictionary<string, TEnum> lookup, TEnum other) where TEnum : struct, Enum
    {
        if (string.IsNullOrEmpty(attribute?.Value)) return other;

        return lookup.TryGetValue(Normalize(attribute.Value), out var value) ? value : other;
    }

    private static TEnum? ParseOptionalEnum<TEnum>(XAttribute attribute, IReadOnlyDictionary<string, TEnum> lookup) where TEnum : struct, Enum
    {
        if (string.IsNullOrEmpty(attribute?.Value)) return null;
        if (lookup.TryGetValue(Normalize(attribute.Value), out var value)) return value;

        throw new InvalidDataException($"The value '{attribute.Value}' of attribute {attribute.Name.LocalName} on element {attribute.Parent?.Name.LocalName} is not one of {string.Join(", ", Enum.GetNames<TEnum>())}.");
    }

    private static IReadOnlyDictionary<string, TEnum> Lookup<TEnum>(Func<TEnum, string> format) where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>().ToDictionary(x => Normalize(format(x)), x => x);
    }

    private static string Normalize(string value)
    {
        return new string(value.Where(x => x != ' ' && x != '-' && x != '_').ToArray()).ToUpperInvariant();
    }
}
