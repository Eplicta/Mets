namespace Eplicta.Mets.Features.Mods;

/// <summary>A MODS note, its type written as the note's type attribute.</summary>
public record ModsDocumentNote
{
    public string Type { get; init; }
    public string Value { get; init; }
}
