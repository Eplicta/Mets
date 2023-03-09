using System;

namespace Eplicta.Mets.Entities;

public record FileSource
{
    public string Id { get; set; }
    public string FilePath { get; set; }
    public string Use { get; set; }
    public string Checksum { get; set; }
    public MetsData.EChecksumType? ChecksumType { get; set; } = MetsData.EChecksumType.MD5;
    public string FileName { get; set; }
    public byte[] Data { get; set; }
    public DateTime? CreationTime { get; set; }
    public long? Size { get; set; }
    public string MimeType { get; set; }
    //public string Ns2Href { get; set; }

    public static implicit operator FileSource(string filePath)
    {
        return new FileSource { FilePath = filePath };
    }
}