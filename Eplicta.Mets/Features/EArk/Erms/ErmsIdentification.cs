namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// An identifier of the ERMS document itself, with the kind of identifier it is, such as "UUID".
/// </summary>
public record ErmsIdentification
{
    public string Type { get; init; }
    public string Value { get; init; }
}
