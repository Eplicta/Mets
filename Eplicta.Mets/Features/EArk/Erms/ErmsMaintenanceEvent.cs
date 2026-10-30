using System;
using System.Xml.Serialization;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// An event in the life of the ERMS document itself, such as its creation by an export.
/// </summary>
public record ErmsMaintenanceEvent
{
    public enum EEventType
    {
        [XmlEnum("created")] Created,
        [XmlEnum("revised")] Revised,
        [XmlEnum("deleted")] Deleted,
        [XmlEnum("cancelled")] Cancelled,
        [XmlEnum("derived")] Derived,
        [XmlEnum("updated")] Updated,
        [XmlEnum("unknown")] Unknown
    }

    public EEventType EventType { get; init; } = EEventType.Created;
    public DateTime DateTime { get; init; }
    public ErmsAgent Agent { get; init; }
}
