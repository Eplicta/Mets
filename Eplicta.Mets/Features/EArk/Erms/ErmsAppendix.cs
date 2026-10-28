namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// A file belonging to an entity, located by its path relative to the ERMS document.
/// </summary>
public record ErmsAppendix
{
    /// <summary>The name the file is known by, such as its original file name.</summary>
    public string Name { get; init; }

    public string Path { get; init; }
    public string Description { get; init; }
    public string FileFormat { get; init; }
    public string OriginalFileFormat { get; init; }
    public bool? Disposable { get; init; }
}
