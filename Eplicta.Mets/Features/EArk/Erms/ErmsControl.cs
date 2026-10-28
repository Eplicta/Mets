using System.Collections.Generic;
using System.Xml.Serialization;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// The control section of an ERMS export: what the document is, the classification schemas it uses, and who made it.
/// </summary>
public record ErmsControl
{
    public enum EMaintenanceStatus
    {
        [XmlEnum("cancelled")] Cancelled,
        [XmlEnum("created")] Created,
        [XmlEnum("deleted")] Deleted,
        [XmlEnum("derived")] Derived,
        [XmlEnum("new")] New,
        [XmlEnum("revised")] Revised,
        [XmlEnum("unknown")] Unknown,
        [XmlEnum("updated")] Updated
    }

    public IReadOnlyList<ErmsIdentification> Identifications { get; init; } = [];
    public IReadOnlyList<ErmsClassificationSchema> ClassificationSchemas { get; init; } = [];
    public EMaintenanceStatus MaintenanceStatus { get; init; } = EMaintenanceStatus.New;
    public IReadOnlyList<string> AgencyNames { get; init; } = [];
    public string AgencyCode { get; init; }
    public string AgencyCodeType { get; init; }
    public IReadOnlyList<ErmsMaintenanceEvent> MaintenanceEvents { get; init; } = [];
}
