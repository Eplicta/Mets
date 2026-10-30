namespace Eplicta.Mets.Features.EArk.Premis;

/// <summary>
/// A message digest of a file, the algorithm that made it, and who calculated it.
/// </summary>
public record PremisFixity
{
    public string Algorithm { get; init; } = PremisConstants.Sha256;
    public string Digest { get; init; }
    public string Originator { get; init; }
}
