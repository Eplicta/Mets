namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// A typed property of an own element, such as the size or checksum Riksarkivet asks to see beside each appendix.
/// </summary>
public record ErmsOwnElementAttribute
{
    public string Name { get; init; }
    public string DataType { get; init; }
    public string Format { get; init; }
    public string Value { get; init; }
}
