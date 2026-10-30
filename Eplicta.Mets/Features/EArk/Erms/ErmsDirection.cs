using System.Xml.Serialization;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// Whether a record came in, went out, or was drawn up internally.
/// </summary>
public record ErmsDirection
{
    public enum EDefinition
    {
        [XmlEnum("incoming")] Incoming,
        [XmlEnum("outgoing")] Outgoing,
        [XmlEnum("internal_memo_for_follow-up")] InternalMemoForFollowUp,
        [XmlEnum("internal_memo_without_follow-up")] InternalMemoWithoutFollowUp,
        [XmlEnum("case_draft")] CaseDraft,
        [XmlEnum("other")] Other
    }

    public EDefinition Definition { get; init; }
    public string OtherDefinition { get; init; }
}
