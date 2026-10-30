using System;
using System.Xml.Serialization;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// A date in the disposal of an entity, such as the day it may be disposed of.
/// </summary>
public record ErmsDisposalDate
{
    public enum EDateType
    {
        [XmlEnum("action_due")] ActionDue,
        [XmlEnum("applied")] Applied,
        [XmlEnum("confirmation_due")] ConfirmationDue,
        [XmlEnum("disposal_date")] DisposalDate,
        [XmlEnum("lifted")] Lifted,
        [XmlEnum("overdue_alert")] OverdueAlert,
        [XmlEnum("retention_period_start")] RetentionPeriodStart,
        [XmlEnum("retention_period_end")] RetentionPeriodEnd,
        [XmlEnum("other_date")] OtherDate
    }

    public EDateType DateType { get; init; } = EDateType.DisposalDate;
    public string OtherDateType { get; init; }
    public DateTime Value { get; init; }
}
