using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Eplicta.Mets.Features.EArk.Premis;
using Eplicta.Mets.Helpers;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// Builds an E-ARK package that carries an ERMS export, laid out as FGS Paketstruktur 2.0 with FGS Ärendehantering
/// 3.0: ERMS.xml, its logs and its files in the representation's data folder, PREMIS under metadata/preservation,
/// and every schema they validate against. Write the result with <see cref="ErmsPackageWriter"/>.
/// </summary>
public class ErmsPackageBuilder
{
    private const string XmlMimeType = "text/xml";

    private readonly List<Action<EArkPackageBuilder>> _settings = [];
    private readonly List<EArkFile> _dataFiles = [];
    private readonly List<EArkFile> _documentationFiles = [];
    private DateTime? _createDate;
    private ErmsDocument _erms;
    private ErmsLogDocument _logs;
    private PremisDocument _preservation;

    /// <summary>The appendix path an ERMS record gives a file added with <see cref="AddDataFile"/>.</summary>
    public static string AppendixPath(string fileName)
    {
        return $"{EArkConstants.DataFolderName}/{fileName}";
    }

    public ErmsPackageBuilder SetObjId(string objId)
    {
        _settings.Add(x => x.SetObjId(objId));
        return this;
    }

    public ErmsPackageBuilder SetLabel(string label)
    {
        _settings.Add(x => x.SetLabel(label));
        return this;
    }

    public ErmsPackageBuilder SetCreateDate(DateTime createDate)
    {
        _createDate = createDate;
        return this;
    }

    public ErmsPackageBuilder SetRecordStatus(EArkPackage.ERecordStatus recordStatus)
    {
        _settings.Add(x => x.SetRecordStatus(recordStatus));
        return this;
    }

    public ErmsPackageBuilder SetSoftware(string name, string version)
    {
        _settings.Add(x => x.SetSoftware(name, version));
        return this;
    }

    public ErmsPackageBuilder AddAgent(EArkAgent agent)
    {
        _settings.Add(x => x.AddAgent(agent));
        return this;
    }

    public ErmsPackageBuilder AddAltRecordId(EArkAltRecordId altRecordId)
    {
        _settings.Add(x => x.AddAltRecordId(altRecordId));
        return this;
    }

    /// <summary>Riksarkivet's additional package information, for a delivery to Riksarkivet itself.</summary>
    public ErmsPackageBuilder SetAdditionalPackageInfo(AdditionalPackageInfo additionalPackageInfo)
    {
        _settings.Add(x => x.SetAdditionalPackageInfo(additionalPackageInfo));
        return this;
    }

    public ErmsPackageBuilder SetErms(ErmsDocument erms)
    {
        _erms = erms;
        return this;
    }

    public ErmsPackageBuilder SetLogs(ErmsLogDocument logs)
    {
        _logs = logs;
        return this;
    }

    public ErmsPackageBuilder SetPreservation(PremisDocument preservation)
    {
        _preservation = preservation;
        return this;
    }

    /// <summary>Adds a file to the representation's data folder; its href is the file name, as the ERMS appendix names it.</summary>
    public ErmsPackageBuilder AddDataFile(EArkFile file)
    {
        _dataFiles.Add(file);
        return this;
    }

    /// <summary>Adds a file under documentation/, its href given from the package root.</summary>
    public ErmsPackageBuilder AddDocumentationFile(EArkFile file)
    {
        _documentationFiles.Add(file);
        return this;
    }

    public EArkPackage Build()
    {
        if (_erms == null) throw new InvalidOperationException("An ERMS package needs its ERMS document.");

        var created = _createDate ?? DateTime.UtcNow;
        var builder = new EArkPackageBuilder()
            .SetContentCategory(EArkConstants.ContentCategory.Datasets)
            .SetContentInformationType(EArkPackage.EContentInformationType.OTHER, ErmsConstants.ContentInformationType)
            .SetCreateDate(created);

        foreach (var setting in _settings)
        {
            setting(builder);
        }

        builder.AddRepresentationFile(Generated(Data(ErmsConstants.FileName), new ErmsRenderer(_erms).Render(), created));
        builder.AddSchemaFile(Schema(ErmsConstants.SchemaFileName, ErmsConstants.SchemaResource, created));
        builder.AddSchemaFile(Schema(ErmsConstants.SchematronFileName, ErmsConstants.SchematronResource, created));

        if (_logs != null)
        {
            builder.AddRepresentationFile(Generated(Data(ErmsConstants.LogFileName), new ErmsLogRenderer(_logs).Render(), created));
            builder.AddSchemaFile(Schema(ErmsConstants.LogSchemaFileName, ErmsConstants.LogSchemaResource, created));
        }

        if (_preservation != null)
        {
            builder.AddPreservationMetadata(new EArkMetadataReference
            {
                Href = $"{EArkConstants.MetadataFolderName}/{EArkConstants.PreservationFolderName}/{PremisConstants.FileName}",
                MdType = EArkMetadataReference.EMdType.PREMIS,
                MimeType = XmlMimeType,
                Created = created,
                Content = Bytes(new PremisRenderer(_preservation).Render())
            });
            builder.AddSchemaFile(Schema(PremisConstants.SchemaFileName, PremisConstants.SchemaResource, created));
        }

        foreach (var file in _dataFiles)
        {
            builder.AddRepresentationFile(file with { Href = Data(file.Href) });
        }

        foreach (var file in _documentationFiles)
        {
            builder.AddDocumentationFile(file);
        }

        return builder.Build();
    }

    private static string Data(string fileName)
    {
        return $"{EArkConstants.RepresentationsFolderName}/{EArkConstants.DefaultRepresentationName}/{EArkConstants.DataFolderName}/{fileName}";
    }

    private static EArkFile Generated(string href, XmlDocument document, DateTime created)
    {
        return new EArkFile { Href = href, MimeType = XmlMimeType, Created = created, Content = Bytes(document) };
    }

    private static EArkFile Schema(string fileName, string resource, DateTime created)
    {
        return new EArkFile { Href = $"{EArkConstants.SchemasFolderName}/{fileName}", MimeType = XmlMimeType, Created = created, Content = Encoding.UTF8.GetBytes(Resource.Get(resource)) };
    }

    private static byte[] Bytes(XmlDocument document)
    {
        using var buffer = new MemoryStream();
        using (var writer = new XmlTextWriter(buffer, new UTF8Encoding(false)) { Formatting = Formatting.Indented })
        {
            document.WriteTo(writer);
        }

        return buffer.ToArray();
    }
}
