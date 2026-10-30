using System.Collections.Generic;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// The changes made to one case and its records, keyed by the case's object id.
/// </summary>
public record ErmsCaseLog
{
    public string Id { get; init; }
    public IReadOnlyList<ErmsLogModification> Modifications { get; init; } = [];
}
