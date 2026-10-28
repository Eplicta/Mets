namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// A class the entity is classified under, by its name and, where it has them, its id and code.
/// </summary>
public record ErmsClassification
{
    public string Value { get; init; }
    public string ClassificationId { get; init; }
    public string ClassificationCode { get; init; }
    public string FullyQualifiedClassificationCode { get; init; }
}
