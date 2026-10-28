using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// A classification schema the exported material was classified by, referenced from an aggregation or record by its id.
/// </summary>
public record ErmsClassificationSchema
{
    public enum EStatus
    {
        [XmlEnum("current")] Current,
        [XmlEnum("previous")] Previous,
        [XmlEnum("other")] Other
    }

    /// <summary>An xs:ID, so it must be an NCName: no spaces, and not starting with a digit.</summary>
    public string Id { get; init; }

    public EStatus? Status { get; init; }
    public string OtherStatus { get; init; }
    public DateTime? UsedFrom { get; init; }
    public DateTime? UsedTo { get; init; }
    public IReadOnlyList<string> Description { get; init; } = [];
}
