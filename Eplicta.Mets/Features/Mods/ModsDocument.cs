using System;
using System.Collections.Generic;

namespace Eplicta.Mets.Features.Mods;

/// <summary>
/// A standalone MODS document, the kind an E-ARK package references from its dmdSec under metadata/descriptive.
/// Render it with <see cref="ModsDocumentRenderer"/>.
/// </summary>
public record ModsDocument
{
    public required string Identifier { get; init; }

    /// <summary>The identifier's type attribute; <c>local</c> by default.</summary>
    public string IdentifierType { get; init; } = "local";

    public string Title { get; init; }
    public Uri Url { get; init; }

    /// <summary>Written as a UTC instant; a value of <see cref="DateTimeKind.Unspecified"/> is taken to be UTC.</summary>
    public DateTime? DateIssued { get; init; }

    /// <summary>One genre element each, such as <see cref="EArk.EplictaPackageTypes.CaseArchive"/>.</summary>
    public IReadOnlyList<string> Genres { get; init; } = [];

    public IReadOnlyList<ModsDocumentNote> Notes { get; init; } = [];
}
