using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Eplicta.Mets.Entities;
using Eplicta.Mets.Features.EArk.Premis;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// Writes an ERMS package as a zip laid out the way Riksarkivet's application of E-ARK CSIP and SIP asks: a single
/// root folder in which every standard folder exists, METS describing the representation as ERMS, and every streamed
/// file verified against its declared SHA-256 as it is written. Reuses <see cref="EArkPackageWriter.Resolve"/> and
/// <see cref="EArkRenderer"/>, so the METS is the one the general writer would produce.
/// </summary>
public class ErmsPackageWriter
{
    private const int BufferSize = 81920;

    private static readonly string[] AlreadyCompressed = ["application/zip", "application/gzip", "application/x-7z-compressed", "application/x-rar-compressed", "image/jpeg", "image/png", "image/gif", "image/webp"];
    private static readonly string[] AlreadyCompressedPrefixes = ["video/", "audio/", "application/vnd.openxmlformats-officedocument."];

    private readonly EArkPackage _package;

    public ErmsPackageWriter(EArkPackage package)
    {
        _package = package ?? throw new ArgumentNullException(nameof(package));
    }

    /// <summary>The name of the single folder everything unpacks into; the package's OBJID by default.</summary>
    public string RootFolderName { get; init; }

    public async Task WriteAsync(Stream target, CancellationToken cancellationToken = default)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));

        var measured = _package with
        {
            DocumentationFiles = await MeasureAsync(_package.DocumentationFiles, cancellationToken),
            RepresentationFiles = await MeasureAsync(_package.RepresentationFiles, cancellationToken)
        };
        var resolved = new EArkPackageWriter(measured).Resolve();
        var root = RootFolderName ?? resolved.ObjId;
        var document = XmlCharacters.Clean(new EArkRenderer(resolved).Render());
        DeclareRepresentationContent(document, resolved);
        Validate(document, resolved);

        using var archive = new ZipArchive(target, ZipArchiveMode.Create, true);

        foreach (var folder in StandardFolders(resolved.RepresentationName))
        {
            archive.CreateEntry($"{root}/{folder}/");
        }

        await WriteBytesAsync(archive, $"{root}/{EArkConstants.MetsFileName}", Bytes(document), cancellationToken);

        foreach (var reference in resolved.DescriptiveMetadata.Concat(resolved.PreservationMetadata).Where(x => x.Content != null))
        {
            await WriteBytesAsync(archive, $"{root}/{reference.Href}", reference.Content, cancellationToken);
        }

        foreach (var file in resolved.DocumentationFiles.Concat(resolved.SchemaFiles).Concat(resolved.RepresentationFiles))
        {
            await WriteFileAsync(archive, $"{root}/{file.Href}", file, cancellationToken);
        }
    }

    // Checked before a byte is written, so an invalid package fails whole rather than reaching anyone half-written.
    private static void Validate(XmlDocument mets, EArkPackage package)
    {
        var failures = Failures(EArkConstants.MetsFileName, new EArkValidator().Validate(mets)).ToList();
        var data = $"{EArkConstants.RepresentationsFolderName}/{package.RepresentationName}/{EArkConstants.DataFolderName}";
        var validators = new Dictionary<string, Func<XmlDocument, IEnumerable<XmlValidatorResult>>>(StringComparer.Ordinal)
        {
            [$"{data}/{ErmsConstants.FileName}"] = x => new ErmsValidator().Validate(x),
            [$"{data}/{ErmsConstants.LogFileName}"] = x => new ErmsLogValidator().Validate(x),
            [$"{EArkConstants.MetadataFolderName}/{EArkConstants.PreservationFolderName}/{PremisConstants.FileName}"] = x => new PremisValidator().Validate(x)
        };

        var generated = package.RepresentationFiles.Select(x => (x.Href, x.Content)).Concat(package.PreservationMetadata.Select(x => (x.Href, x.Content)));
        foreach (var (href, content) in generated.Where(x => x.Content != null && validators.ContainsKey(x.Href)))
        {
            failures.AddRange(Check(href, content, validators[href]));
        }

        if (failures.Count > 0) throw new ErmsPackageValidationException(failures);
    }

    private static IEnumerable<string> Check(string href, byte[] content, Func<XmlDocument, IEnumerable<XmlValidatorResult>> validator)
    {
        var document = new XmlDocument { XmlResolver = null };
        try
        {
            using var stream = new MemoryStream(content);
            document.Load(stream);
        }
        catch (XmlException exception)
        {
            return [$"{href}: {exception.Message}"];
        }

        return Failures(href, validator(document));
    }

    private static IEnumerable<string> Failures(string href, IEnumerable<XmlValidatorResult> results) =>
        results.Where(x => x.XmlSeverityType == XmlSeverityType.Error).Select(x => $"{href}: {x.Message}");

    private static IEnumerable<string> StandardFolders(string representationName)
    {
        var representation = $"{EArkConstants.RepresentationsFolderName}/{representationName}";

        yield return EArkConstants.MetadataFolderName;
        yield return $"{EArkConstants.MetadataFolderName}/{EArkConstants.DescriptiveFolderName}";
        yield return $"{EArkConstants.MetadataFolderName}/{EArkConstants.PreservationFolderName}";
        yield return $"{EArkConstants.MetadataFolderName}/other";
        yield return EArkConstants.RepresentationsFolderName;
        yield return representation;
        yield return $"{representation}/{EArkConstants.DataFolderName}";
        yield return EArkConstants.SchemasFolderName;
        yield return EArkConstants.DocumentationFolderName;
    }

    private static void DeclareRepresentationContent(XmlDocument document, EArkPackage package)
    {
        if (package.ContentInformationType != EArkPackage.EContentInformationType.OTHER || string.IsNullOrEmpty(package.OtherContentInformationType)) return;

        foreach (var fileGroup in document.GetElementsByTagName("fileGrp", EArkConstants.MetsNamespace).OfType<XmlElement>().Where(x => x.GetAttribute("USE") == EArkConstants.RepresentationsUse))
        {
            var attribute = document.CreateAttribute(EArkConstants.CsipPrefix, "OTHERCONTENTINFORMATIONTYPE", EArkConstants.CsipNamespace);
            attribute.Value = package.OtherContentInformationType;
            fileGroup.Attributes.Append(attribute);
        }
    }

    private static async Task<IReadOnlyList<EArkFile>> MeasureAsync(IReadOnlyList<EArkFile> files, CancellationToken cancellationToken)
    {
        var measured = new List<EArkFile>();

        foreach (var file in files)
        {
            if (file is EArkStreamedFile streamed && (string.IsNullOrEmpty(streamed.Checksum) || streamed.Size <= 0))
            {
                await using var source = await OpenAsync(streamed, cancellationToken);
                var (size, checksum) = await CopyAsync(source, Stream.Null, cancellationToken);
                measured.Add(streamed with { Size = size, Checksum = checksum, ChecksumType = MetsData.EChecksumType.SHA_256 });
            }
            else
            {
                measured.Add(file);
            }
        }

        return measured;
    }

    private static async Task WriteFileAsync(ZipArchive archive, string entryName, EArkFile file, CancellationToken cancellationToken)
    {
        if (file is EArkStreamedFile streamed)
        {
            await using var source = await OpenAsync(streamed, cancellationToken);
            await using var entry = archive.CreateEntry(entryName, LevelFor(file.MimeType)).Open();
            var (size, checksum) = await CopyAsync(source, entry, cancellationToken);
            Verify(file, size, checksum);
            return;
        }

        if (file.Content != null)
        {
            await WriteBytesAsync(archive, entryName, file.Content, cancellationToken, LevelFor(file.MimeType));
            return;
        }

        if (!string.IsNullOrEmpty(file.SourcePath))
        {
            await using var source = File.OpenRead(file.SourcePath);
            await using var entry = archive.CreateEntry(entryName, LevelFor(file.MimeType)).Open();
            await source.CopyToAsync(entry, BufferSize, cancellationToken);
            return;
        }

        throw new InvalidOperationException($"{file.Href} is described in the package but has no content to write.");
    }

    private static void Verify(EArkFile file, long size, string checksum)
    {
        if (file.ChecksumType == MetsData.EChecksumType.SHA_256 && !string.Equals(checksum, file.Checksum, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException($"The bytes of {file.Href} do not match the SHA-256 the package declares for it.");
        }

        if (file.Size > 0 && size != file.Size)
        {
            throw new InvalidDataException($"{file.Href} holds {size} bytes where the package declares {file.Size}.");
        }
    }

    private static async Task<Stream> OpenAsync(EArkStreamedFile file, CancellationToken cancellationToken)
    {
        if (file.Open == null) throw new InvalidOperationException($"{file.Href} is a streamed file with nothing to open.");
        return await file.Open(cancellationToken) ?? throw new InvalidOperationException($"{file.Href} opened no stream.");
    }

    private static async Task<(long Size, string Checksum)> CopyAsync(Stream source, Stream destination, CancellationToken cancellationToken)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[BufferSize];
        long size = 0;
        int read;

        while ((read = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
        {
            hash.AppendData(buffer, 0, read);
            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            size += read;
        }

        return (size, Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());
    }

    private static async Task WriteBytesAsync(ZipArchive archive, string entryName, byte[] content, CancellationToken cancellationToken, CompressionLevel level = CompressionLevel.Optimal)
    {
        await using var entry = archive.CreateEntry(entryName, level).Open();
        await entry.WriteAsync(content, cancellationToken);
    }

    private static CompressionLevel LevelFor(string mimeType)
    {
        var type = mimeType ?? string.Empty;
        return AlreadyCompressed.Contains(type, StringComparer.OrdinalIgnoreCase) || AlreadyCompressedPrefixes.Any(x => type.StartsWith(x, StringComparison.OrdinalIgnoreCase)) ? CompressionLevel.NoCompression : CompressionLevel.Optimal;
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
