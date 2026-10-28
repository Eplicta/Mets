using System.Collections.Generic;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// Something done in the handling of an entity, such as a case being opened or a decision taken, with when and by whom.
/// </summary>
public record ErmsAction
{
    public string ActionText { get; init; }
    public string ActionDue { get; init; }
    public string ActionMotivation { get; init; }
    public string ActionType { get; init; }
    public IReadOnlyList<ErmsDate> Dates { get; init; } = [];
    public IReadOnlyList<ErmsAgent> Agents { get; init; } = [];
}
