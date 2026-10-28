using System.Collections.Generic;
using System.Xml.Serialization;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// What an aggregation and a record have in common in ERMS: identity, titles, classification, restrictions,
/// disposal, agents, dates and actions.
/// </summary>
public abstract record ErmsEntity
{
    public enum EStatus
    {
        [XmlEnum("ad_acta")] AdActa,
        [XmlEnum("closed")] Closed,
        [XmlEnum("expedited")] Expedited,
        [XmlEnum("initiated")] Initiated,
        [XmlEnum("in_progress")] InProgress,
        [XmlEnum("obliterated")] Obliterated,
        [XmlEnum("on_hold")] OnHold,
        [XmlEnum("open")] Open,
        [XmlEnum("prepared")] Prepared,
        [XmlEnum("received")] Received
    }

    public string SystemIdentifier { get; init; }

    /// <summary>The id of the <see cref="ErmsClassificationSchema"/> this entity is classified by.</summary>
    public string ClassificationSchemaUsed { get; init; }

    public string ObjectId { get; init; }
    public IReadOnlyList<ErmsExtraId> ExtraIds { get; init; } = [];
    public IReadOnlyList<ErmsClassification> Classifications { get; init; } = [];

    /// <summary>The object id of the aggregation this entity belongs to, such as a record's case.</summary>
    public string ParentAggregationId { get; init; }

    public IReadOnlyList<string> Keywords { get; init; } = [];
    public string Title { get; init; }
    public IReadOnlyList<ErmsOtherTitle> OtherTitles { get; init; } = [];
    public EStatus? Status { get; init; }
    public IReadOnlyList<ErmsRelation> Relations { get; init; } = [];
    public IReadOnlyList<ErmsRestriction> Restrictions { get; init; } = [];
    public ErmsDisposal Disposal { get; init; }
    public IReadOnlyList<ErmsAgent> Agents { get; init; } = [];
    public string Description { get; init; }
    public IReadOnlyList<ErmsDate> Dates { get; init; } = [];
    public IReadOnlyList<ErmsAction> Actions { get; init; } = [];
    public IReadOnlyList<ErmsNote> Notes { get; init; } = [];
    public ErmsAdditionalInformation AdditionalInformation { get; init; }
}
