using System.Xml.Serialization;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// One way to reach an agent, such as an e-mail address or a phone number.
/// </summary>
public record ErmsContactLine
{
    public enum EContactType
    {
        [XmlEnum("phonenumber")] PhoneNumber,
        [XmlEnum("mobilenumber")] MobileNumber,
        [XmlEnum("fax")] Fax,
        [XmlEnum("email")] Email,
        [XmlEnum("homepage")] Homepage,
        [XmlEnum("other")] Other
    }

    public EContactType ContactType { get; init; }
    public string OtherContactType { get; init; }
    public string Value { get; init; }
}
