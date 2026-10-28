using System.Collections.Generic;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// The files belonging to an entity and the values the ERMS vocabulary has no element for.
/// </summary>
public record ErmsAdditionalInformation
{
    public IReadOnlyList<ErmsAppendix> Appendices { get; init; } = [];
    public IReadOnlyList<ErmsOwnElementSet> OwnElements { get; init; } = [];
}
