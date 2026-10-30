using System.Collections.Generic;

namespace Eplicta.Mets.Features.EArk.Premis;

/// <summary>
/// A person, organisation or program that took part in an event.
/// </summary>
public record PremisAgent
{
    public PremisIdentifier Identifier { get; init; }
    public string Name { get; init; }

    /// <summary>A term such as "software", "organization" or "person".</summary>
    public string Type { get; init; }

    public string Version { get; init; }
    public IReadOnlyList<string> Notes { get; init; } = [];
}
