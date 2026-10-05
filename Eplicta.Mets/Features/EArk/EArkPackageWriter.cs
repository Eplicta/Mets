using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Eplicta.Mets.Entities;
using Eplicta.Mets.Helpers;

namespace Eplicta.Mets.Features.EArk;

/// <summary>
/// Writes an <see cref="EArkPackage"/> as a zip archive laid out the way E-ARK CSIP requires: a single root
/// folder holding METS.xml, metadata, representations, schemas and documentation.
/// </summary>
public class EArkPackageWriter
{
    private static readonly string[] SchemaResources = ["mets.xsd", "DILCISExtensionMETS.xsd", "DILCISExtensionSIPMETS.xsd", "xlink.xsd"];
    private readonly EArkPackage _package;

    public EArkPackageWriter(EArkPackage package)
    {
        _package = package ?? throw new ArgumentNullException(nameof(package));
    }

    /// <summary>
    /// The name of the single folder everything unpacks into. CSIPSTR2 asks for the package identifier, which
    /// is the default, but an OBJID may hold characters a file system rejects.
    /// </summary>
    public string RootFolderName { get; init; }

    public void Write(Stream target)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));

        var resolved = Resolve();
        var root = RootFolderName ?? resolved.ObjId;
        var document = new EArkRenderer(resolved).Render();

        using var archive = new ZipArchive(target, ZipArchiveMode.Create, true);

        WriteDocument(archive, $"{root}/{EArkConstants.MetsFileName}", document);

        foreach (var reference in resolved.DescriptiveMetadata.Concat(resolved.PreservationMetadata))
        {
            if (reference.Content != null) WriteBytes(archive, $"{root}/{reference.Href}", reference.Content);
        }

        foreach (var file in resolved.DocumentationFiles.Concat(resolved.SchemaFiles).Concat(resolved.RepresentationFiles))
        {
            WriteFile(archive, $"{root}/{file.Href}", file);
        }
    }

    /// <summary>
    /// Fills in the sizes, checksums and carried schemas that the METS document has to describe, so the
    /// document and the bytes beside it cannot disagree.
    /// </summary>
    public EArkPackage Resolve()
    {
        return _package with
        {
            DescriptiveMetadata = _package.DescriptiveMetadata.Select(Measure).ToArray(),
            PreservationMetadata = _package.PreservationMetadata.Select(Measure).ToArray(),
            DocumentationFiles = _package.DocumentationFiles.Select(Measure).ToArray(),
            SchemaFiles = SchemaFiles().Select(Measure).ToArray(),
            RepresentationFiles = _package.RepresentationFiles.Select(Measure).ToArray()
        };
    }

    private IEnumerable<EArkFile> SchemaFiles()
    {
        foreach (var file in _package.SchemaFiles)
        {
            yield return file;
        }

        var carried = _package.SchemaFiles.Select(x => x.Href).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var name in SchemaNames())
        {
            var href = $"{EArkConstants.SchemasFolderName}/{name}";
            if (carried.Contains(href)) continue;

            yield return new EArkFile
            {
                Id = EArkPackageBuilder.NewId(),
                Href = href,
                MimeType = "text/xml",
                Created = _package.CreateDate,
                Content = Encoding.UTF8.GetBytes(Resource.Get(ResourceName(name)))
            };
        }
    }

    private IEnumerable<string> SchemaNames()
    {
        foreach (var name in SchemaResources)
        {
            yield return name;
        }

        if (_package.AdditionalPackageInfo != null) yield return "SNAadditionalPackageInfo.xsd";
    }

    private static string ResourceName(string name)
    {
        return name == "xlink.xsd" ? name : $"EArk.{name}";
    }

    private static EArkFile Measure(EArkFile file)
    {
        if (!string.IsNullOrEmpty(file.Checksum) && file.Size > 0) return file;

        var (size, checksum) = Measure(() => Open(file));
        return file with
        {
            Size = file.Size > 0 ? file.Size : size,
            Checksum = string.IsNullOrEmpty(file.Checksum) ? checksum : file.Checksum
        };
    }

    private static EArkMetadataReference Measure(EArkMetadataReference reference)
    {
        if (!string.IsNullOrEmpty(reference.Checksum) && reference.Size > 0) return reference;
        if (reference.Content == null) return reference;

        var (size, checksum) = Measure(() => new MemoryStream(reference.Content));
        return reference with
        {
            Size = reference.Size > 0 ? reference.Size : size,
            Checksum = string.IsNullOrEmpty(reference.Checksum) ? checksum : reference.Checksum
        };
    }

    private static (long Size, string Checksum) Measure(Func<Stream> open)
    {
        using var stream = open();
        if (stream == null) return (0, null);

        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(stream);
        return (stream.Length, Convert.ToHexString(hash).ToLowerInvariant());
    }

    private static Stream Open(EArkFile file)
    {
        if (file.Content != null) return new MemoryStream(file.Content);
        if (!string.IsNullOrEmpty(file.SourcePath)) return File.OpenRead(file.SourcePath);
        return null;
    }

    private static void WriteDocument(ZipArchive archive, string entryName, XmlDocument document)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        using var stream = entry.Open();
        using var writer = new XmlTextWriter(stream, new UTF8Encoding(false)) { Formatting = Formatting.Indented };
        document.WriteTo(writer);
    }

    private static void WriteBytes(ZipArchive archive, string entryName, byte[] content)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        using var stream = entry.Open();
        stream.Write(content, 0, content.Length);
    }

    private static void WriteFile(ZipArchive archive, string entryName, EArkFile file)
    {
        using var source = Open(file);
        if (source == null) return;

        var level = IsAlreadyCompressed(file.MimeType) ? CompressionLevel.NoCompression : CompressionLevel.Optimal;
        var entry = archive.CreateEntry(entryName, level);
        using var stream = entry.Open();
        source.CopyTo(stream);
    }

    private static bool IsAlreadyCompressed(string mimeType)
    {
        return mimeType is "application/zip" or "application/x-tar" or "application/gzip";
    }
}
