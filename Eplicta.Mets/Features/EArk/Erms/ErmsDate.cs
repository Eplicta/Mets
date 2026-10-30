using System;
using System.Xml.Serialization;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// A dated moment in the life of an entity, such as when it was created, received or closed.
/// </summary>
public record ErmsDate
{
    public enum EDateType
    {
        [XmlEnum("aggregated")] Aggregated,
        [XmlEnum("appraisal")] Appraisal,
        [XmlEnum("archived")] Archived,
        [XmlEnum("archiving")] Archiving,
        [XmlEnum("captured")] Captured,
        [XmlEnum("checked_in")] CheckedIn,
        [XmlEnum("checked_out")] CheckedOut,
        [XmlEnum("classification")] Classification,
        [XmlEnum("closed")] Closed,
        [XmlEnum("confidentiality_assessed")] ConfidentialityAssessed,
        [XmlEnum("created")] Created,
        [XmlEnum("decision")] Decision,
        [XmlEnum("decision_date")] DecisionDate,
        [XmlEnum("decision_deadline")] DecisionDeadline,
        [XmlEnum("decrypted")] Decrypted,
        [XmlEnum("deleted")] Deleted,
        [XmlEnum("destroyed")] Destroyed,
        [XmlEnum("dispatch")] Dispatch,
        [XmlEnum("encrypted")] Encrypted,
        [XmlEnum("end")] End,
        [XmlEnum("expedited")] Expedited,
        [XmlEnum("expiration")] Expiration,
        [XmlEnum("finished")] Finished,
        [XmlEnum("first_used")] FirstUsed,
        [XmlEnum("last_addition")] LastAddition,
        [XmlEnum("last_addition_timestamp")] LastAdditionTimestamp,
        [XmlEnum("last_reviewed")] LastReviewed,
        [XmlEnum("loan")] Loan,
        [XmlEnum("main_signature")] MainSignature,
        [XmlEnum("modified")] Modified,
        [XmlEnum("moved")] Moved,
        [XmlEnum("opened")] Opened,
        [XmlEnum("opening_date")] OpeningDate,
        [XmlEnum("originated")] Originated,
        [XmlEnum("other_signature")] OtherSignature,
        [XmlEnum("ownership_start")] OwnershipStart,
        [XmlEnum("prepared")] Prepared,
        [XmlEnum("received")] Received,
        [XmlEnum("received_at_location")] ReceivedAtLocation,
        [XmlEnum("relocated")] Relocated,
        [XmlEnum("rendered")] Rendered,
        [XmlEnum("reviewed")] Reviewed,
        [XmlEnum("sent")] Sent,
        [XmlEnum("start")] Start,
        [XmlEnum("take_back")] TakeBack,
        [XmlEnum("transferred")] Transferred,
        [XmlEnum("other")] Other
    }

    public EDateType DateType { get; init; }
    public string OtherDateType { get; init; }
    public DateTime Value { get; init; }
}
