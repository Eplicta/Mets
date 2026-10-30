using System.Collections.Generic;
using System.Xml.Serialization;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// An aggregation in an ERMS export, such as a case file holding its records, or a series holding case files.
/// </summary>
public record ErmsAggregation : ErmsEntity
{
    public enum EAggregationType
    {
        [XmlEnum("caseFile")] CaseFile,
        [XmlEnum("class")] Class,
        [XmlEnum("component")] Component,
        [XmlEnum("file")] File,
        [XmlEnum("subfile")] Subfile,
        [XmlEnum("volume")] Volume,
        [XmlEnum("own_aggregation_definition")] OwnAggregationDefinition
    }

    public EAggregationType AggregationType { get; init; } = EAggregationType.CaseFile;
    public string OtherAggregationType { get; init; }
    public IReadOnlyList<ErmsAggregation> Aggregations { get; init; } = [];
    public IReadOnlyList<ErmsRecord> Records { get; init; } = [];
}
