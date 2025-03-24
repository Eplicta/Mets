namespace Eplicta.Mets.Entities;

public record FileSource : SourceBase
{
    public string FilePath { get; set; }
    public byte[] Data { get; set; }
    public string FileName { get; set; }

    public static implicit operator FileSource(string filePath)
    {
        return new FileSource { FilePath = filePath };
    }
}