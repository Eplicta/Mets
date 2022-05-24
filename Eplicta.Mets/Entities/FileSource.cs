using System;

namespace Eplicta.Mets.Entities;

public record FileSource
{
    public string Id { get; set; }
    public string FilePath { get; set; }
    public string Use { get; set; }
    public ModsData.EChecksumType? ChecksumType { get; set; } = ModsData.EChecksumType.MD5;
    public string FileName { get; set; }
    public byte[] Data { get; set; }
    public DateTime? CreationTime { get; set; }

    public static implicit operator FileSource(string filePath)
    {
        return new FileSource { FilePath = filePath };
    }
}