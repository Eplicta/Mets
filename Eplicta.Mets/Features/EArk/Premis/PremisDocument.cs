using System.Collections.Generic;

namespace Eplicta.Mets.Features.EArk.Premis;

/// <summary>
/// The files a package holds, what was done to them, and by whom, rendered by <see cref="PremisRenderer"/>.
/// </summary>
public record PremisDocument
{
    public IReadOnlyList<PremisFile> Files { get; init; } = [];
    public IReadOnlyList<PremisEvent> Events { get; init; } = [];
    public IReadOnlyList<PremisAgent> Agents { get; init; } = [];
}
