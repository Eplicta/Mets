using System.Collections.Generic;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// The fields one change altered on a record of the case, naming the record by its object id and title.
/// </summary>
public record ErmsLogRecordChange
{
    public string Id { get; init; }
    public string Name { get; init; }

    /// <summary>A short list of what changed, which Riksarkivet writes as the changed field names.</summary>
    public string Summary { get; init; }

    public IReadOnlyList<ErmsLogField> Fields { get; init; } = [];
}
