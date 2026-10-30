namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// An identifier other than the object id, with the kind of identifier it is.
/// </summary>
public record ErmsExtraId
{
    public string Type { get; init; }
    public string Value { get; init; }
}
