namespace Eplicta.Mets.Features.EArk.Premis;

/// <summary>
/// An identifier of a PREMIS object, event or agent, with the kind of identifier it is.
/// </summary>
public record PremisIdentifier
{
    public string Type { get; init; } = PremisConstants.LocalIdentifierType;
    public string Value { get; init; }

    public static PremisIdentifier Local(string value)
    {
        return new PremisIdentifier { Value = value };
    }
}
