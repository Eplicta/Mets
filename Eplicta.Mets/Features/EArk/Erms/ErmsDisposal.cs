namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// Whether, when and on what authority the entity may be disposed of.
/// </summary>
public record ErmsDisposal
{
    public bool Disposable { get; init; }
    public string DisposalScheduleId { get; init; }
    public string DisposalAction { get; init; }
    public string DisposalPeriod { get; init; }
    public string DisposalMandate { get; init; }
    public string DisposalDescription { get; init; }

    /// <summary>The schema requires exactly one disposal date whenever disposal is given.</summary>
    public ErmsDisposalDate Date { get; init; }
}
