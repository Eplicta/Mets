namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// A title other than the main one, such as the public title of a case whose real title is secret.
/// </summary>
public record ErmsOtherTitle
{
    public string TitleType { get; init; }
    public string Value { get; init; }
}
