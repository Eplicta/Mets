using System;

namespace Eplicta.Mets.Entities;

public abstract record SourceBase
{
    /// <summary>
    /// Optional id. If not provided an ID based on the MD5-hash with hex format will be generated.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Optional parameter. If not provided this field will be omitted.
    /// </summary>
    public string Use { get; set; }

    /// <summary>
    /// Optional parameter. If not provided this field will be omitted.
    /// </summary>
    public string MimeType { get; set; }

    /// <summary>
    /// Optional parameter. If not provided the size of the stream will be used.
    /// </summary>
    public long? Size { get; set; }

    /// <summary>
    /// Optional parameter. If not provided this field will be omitted.
    /// </summary>
    public DateTime? CreationTime { get; set; }

    /// <summary>
    /// Optional parameter. If not provided a MD5-hash with hex format will be generated.
    /// </summary>
    public string Checksum { get; set; }

    /// <summary>
    /// Optional parameter that specifies the format of the Checksum, if provided.
    /// </summary>
    public MetsData.EChecksumType? ChecksumType { get; set; } = MetsData.EChecksumType.MD5;
}