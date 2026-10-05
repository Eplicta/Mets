namespace Eplicta.Mets.Features.EArk;

/// <summary>
/// A mets/metsHdr/altRecordID entry. SIP5-8 define the submission agreement and reference code types.
/// </summary>
public record EArkAltRecordId
{
    public enum EType
    {
        SubmissionAgreement,
        PreviousSubmissionAgreement,
        ReferenceCode,
        PreviousReferenceCode
    }

    public EType Type { get; init; }
    public string Value { get; init; }
}
