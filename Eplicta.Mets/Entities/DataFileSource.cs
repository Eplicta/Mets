namespace Eplicta.Mets.Entities;

public record DataFileSource : SourceBase
{

    /// <summary>
    /// The resource Data.
    /// </summary>
    public byte[] Data { get; set; }
}