using System.Collections.Generic;
using System.Xml.Serialization;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// A person or organisation and the part they play, such as the responsible handler or the sender of a record.
/// </summary>
public record ErmsAgent
{
    public enum EAgentType
    {
        [XmlEnum("administrator")] Administrator,
        [XmlEnum("agent")] Agent,
        [XmlEnum("archiver")] Archiver,
        [XmlEnum("authorising_person")] AuthorisingPerson,
        [XmlEnum("borrower")] Borrower,
        [XmlEnum("contact_person")] ContactPerson,
        [XmlEnum("counterpart")] Counterpart,
        [XmlEnum("creator")] Creator,
        [XmlEnum("custodian")] Custodian,
        [XmlEnum("deliverer")] Deliverer,
        [XmlEnum("dispatcher")] Dispatcher,
        [XmlEnum("editor")] Editor,
        [XmlEnum("ipp_owner")] IppOwner,
        [XmlEnum("main_signatory")] MainSignatory,
        [XmlEnum("mover")] Mover,
        [XmlEnum("opening_person")] OpeningPerson,
        [XmlEnum("other_signatory")] OtherSignatory,
        [XmlEnum("owner")] Owner,
        [XmlEnum("reader")] Reader,
        [XmlEnum("recipient")] Recipient,
        [XmlEnum("receiver")] Receiver,
        [XmlEnum("relocator")] Relocator,
        [XmlEnum("responsible_person")] ResponsiblePerson,
        [XmlEnum("sender")] Sender,
        [XmlEnum("user")] User,
        [XmlEnum("other")] Other
    }

    public EAgentType AgentType { get; init; }
    public string OtherAgentType { get; init; }
    public string Name { get; init; }
    public string Organisation { get; init; }
    public string UnitName { get; init; }
    public string IdNumber { get; init; }
    public string IdNumberType { get; init; }
    public string Role { get; init; }
    public IReadOnlyList<ErmsAddressLine> AddressLines { get; init; } = [];
    public IReadOnlyList<ErmsContactLine> ContactLines { get; init; } = [];
    public bool? ProtectedIdentity { get; init; }
    public IReadOnlyList<ErmsNote> Notes { get; init; } = [];
}
