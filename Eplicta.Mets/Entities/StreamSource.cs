using System.IO;

namespace Eplicta.Mets.Entities;

public record StreamSource : SourceBase
{
    /// <summary>
    /// Stream to the resource.
    /// </summary>
    public Stream Stream { get; set; }

    /// <summary>
    /// Name of the resource is a mandatory field.
    /// </summary>
    public string Name { get; set; }
}