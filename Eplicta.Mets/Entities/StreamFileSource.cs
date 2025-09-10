using System.IO;

namespace Eplicta.Mets.Entities;

public record StreamFileSource : SourceBase
{
    /// <summary>
    /// Stream to the resource.
    /// </summary>
    public Stream Stream { get; set; }
}