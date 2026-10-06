using System;
using Eplicta.Mets.Entities;

namespace Eplicta.Mets.Features.EArk;

/// <summary>
/// A metadata file referenced by a dmdSec or an amdSec/digiprovMD through mdRef. CSIP describes referencing
/// rather than embedding, so the file itself lives under metadata/ in the package.
/// </summary>
public record EArkMetadataReference
{
    public enum EMdType
    {
        MODS,
        EAD,
        DC,
        PREMIS,
        METSRIGHTS,
        OTHER
    }

    public enum EStatus
    {
        Current,
        Superseded
    }

    public string Id { get; init; }
    public string Href { get; init; }
    public EMdType MdType { get; init; }
    public string MimeType { get; init; } = "text/xml";
    public long Size { get; init; }
    public DateTime Created { get; init; }
    public string Checksum { get; init; }
    public MetsData.EChecksumType ChecksumType { get; init; } = MetsData.EChecksumType.SHA_256;
    public EStatus Status { get; init; } = EStatus.Current;
    public byte[] Content { get; init; }
}
