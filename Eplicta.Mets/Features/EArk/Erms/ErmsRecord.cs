namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// A record in an ERMS export, such as a registered document of a case, with its files listed as appendices.
/// </summary>
public record ErmsRecord : ErmsEntity
{
    public string RecordType { get; init; }
    public int? RunningNumber { get; init; }
    public ErmsDirection Direction { get; init; }
}
