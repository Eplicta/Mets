using System.Xml.Serialization;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// One line of an agent's address, such as the postal code.
/// </summary>
public record ErmsAddressLine
{
    public enum EAddressType
    {
        [XmlEnum("postal_address")] PostalAddress,
        [XmlEnum("postal_code")] PostalCode,
        [XmlEnum("postal_city")] PostalCity,
        [XmlEnum("post_box")] PostBox,
        [XmlEnum("municipality_code")] MunicipalityCode,
        [XmlEnum("municipality")] Municipality,
        [XmlEnum("parish")] Parish,
        [XmlEnum("parish_code")] ParishCode,
        [XmlEnum("province")] Province,
        [XmlEnum("county")] County,
        [XmlEnum("country")] Country,
        [XmlEnum("other")] Other
    }

    public EAddressType AddressType { get; init; }
    public string OtherAddressType { get; init; }
    public string Value { get; init; }
}
