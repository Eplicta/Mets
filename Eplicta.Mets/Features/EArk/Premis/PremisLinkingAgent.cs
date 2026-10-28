namespace Eplicta.Mets.Features.EArk.Premis;

/// <summary>
/// An agent taking part in an event, and the part it played, such as "executing program".
/// </summary>
public record PremisLinkingAgent
{
    public PremisIdentifier Identifier { get; init; }
    public string Role { get; init; }
}
