using System;
using Eplicta.Mets.Entities;

namespace Eplicta.Mets.Features.EArk;

/// <summary>
/// A file described by a mets/fileSec/fileGrp/file entry, located by its package-relative path.
/// </summary>
public record EArkFile
{
    public string Id { get; init; }
    public string Href { get; init; }
    public string MimeType { get; init; }
    public long Size { get; init; }
    public DateTime Created { get; init; }
    public string Checksum { get; init; }
    public MetsData.EChecksumType ChecksumType { get; init; } = MetsData.EChecksumType.SHA_256;

    /// <summary>
    /// The file content held in memory. Mutually exclusive with <see cref="SourcePath"/>.
    /// </summary>
    public byte[] Content { get; init; }

    /// <summary>
    /// A path on disk to stream the content from, so a large payload never has to be held in memory.
    /// Mutually exclusive with <see cref="Content"/>.
    /// </summary>
    public string SourcePath { get; init; }
}
