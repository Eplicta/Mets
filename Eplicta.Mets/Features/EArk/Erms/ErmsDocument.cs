using System.Collections.Generic;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// An ERMS export: a control section and either aggregations or records, rendered by <see cref="ErmsRenderer"/>.
/// </summary>
public record ErmsDocument
{
    public ErmsControl Control { get; init; }
    public IReadOnlyList<ErmsAggregation> Aggregations { get; init; } = [];
    public IReadOnlyList<ErmsRecord> Records { get; init; } = [];
    public ErmsAdditionalInformation AdditionalInformation { get; init; }
}
