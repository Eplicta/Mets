using System;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// A free-text note, optionally typed and dated.
/// </summary>
public record ErmsNote
{
    public string NoteType { get; init; }
    public DateTime? NoteDate { get; init; }
    public string Value { get; init; }
}
