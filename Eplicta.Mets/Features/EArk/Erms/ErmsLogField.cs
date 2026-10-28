namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// A field's value before and after a change.
/// </summary>
public record ErmsLogField
{
    public string Name { get; init; }
    public string OriginalValue { get; init; }
    public string NewValue { get; init; }
}
