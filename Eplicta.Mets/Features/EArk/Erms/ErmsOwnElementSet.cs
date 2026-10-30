using System.Collections.Generic;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// A group of own elements with an optional description of what they hold.
/// </summary>
public record ErmsOwnElementSet
{
    public string Description { get; init; }
    public IReadOnlyList<ErmsOwnElement> Elements { get; init; } = [];
}
