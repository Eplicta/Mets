using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// The change history of exported cases in Riksarkivet's ermsLogs format, rendered by <see cref="ErmsLogRenderer"/>.
/// </summary>
public record ErmsLogDocument
{
    public enum ECreatorCodeType
    {
        [XmlEnum("VAT")] Vat,
        [XmlEnum("DUNS")] Duns,
        [XmlEnum("ORG")] Org,
        [XmlEnum("HSA")] Hsa,
        [XmlEnum("Local")] Local,
        [XmlEnum("URI")] Uri,
        [XmlEnum("OTHER")] Other
    }

    public string IdentificationType { get; init; } = "UUID";
    public string Identification { get; init; }
    public string Status { get; init; } = "NEW";
    public DateTime CreationDate { get; init; }
    public string CreatorName { get; init; }
    public string CreatorCode { get; init; }
    public ECreatorCodeType CreatorCodeType { get; init; } = ECreatorCodeType.Org;
    public string CreatorDescription { get; init; }
    public IReadOnlyList<ErmsCaseLog> CaseLogs { get; init; } = [];
}
