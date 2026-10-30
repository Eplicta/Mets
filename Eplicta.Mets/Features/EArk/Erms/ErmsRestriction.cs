using System.Collections.Generic;
using System.Xml.Serialization;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// A restriction on access, such as secrecy, with the regulation it rests on.
/// </summary>
public record ErmsRestriction
{
    public enum ERestrictionType
    {
        [XmlEnum("confidential")] Confidential,
        [XmlEnum("gdpr")] Gdpr,
        [XmlEnum("integrity")] Integrity,
        [XmlEnum("other_type")] OtherType
    }

    public ERestrictionType RestrictionType { get; init; }
    public string OtherRestrictionType { get; init; }
    public string ExplanatoryText { get; init; }
    public string Regulation { get; init; }
    public IReadOnlyList<ErmsAgent> Agents { get; init; } = [];
    public IReadOnlyList<ErmsDate> Dates { get; init; } = [];
}
