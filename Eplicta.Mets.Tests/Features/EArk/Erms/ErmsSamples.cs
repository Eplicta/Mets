using System;
using Eplicta.Mets.Features.EArk.Erms;

namespace Eplicta.Mets.Tests.Features.EArk.Erms;

internal static class ErmsSamples
{
    public static readonly DateTime Created = new(2026, 3, 14, 9, 30, 0, DateTimeKind.Utc);

    public static ErmsControl Control()
    {
        return new ErmsControl
        {
            Identifications = [new ErmsIdentification { Type = "UUID", Value = "71a4e2a1-f2ad-417a-98b9-ae2b821288bf" }],
            ClassificationSchemas =
            [
                new ErmsClassificationSchema
                {
                    Id = "cs-skolan",
                    Status = ErmsClassificationSchema.EStatus.Current,
                    UsedFrom = new DateTime(2026, 1, 1),
                    Description = ["1 Styra verksamheten", "2 Utbilda elever"]
                }
            ],
            AgencyNames = ["Futuraskolan AB"],
            AgencyCode = "5560000000",
            AgencyCodeType = "organisationsnummer",
            MaintenanceEvents = [new ErmsMaintenanceEvent { DateTime = Created, Agent = new ErmsAgent { AgentType = ErmsAgent.EAgentType.Creator, Name = "FortDocs" } }]
        };
    }

    public static ErmsAggregation MinimalCaseFile()
    {
        return new ErmsAggregation { SystemIdentifier = "c-1", ObjectId = "2026-0042" };
    }

    public static ErmsDocument Minimal()
    {
        return new ErmsDocument { Control = Control(), Aggregations = [MinimalCaseFile()] };
    }

    public static ErmsAgent Person(ErmsAgent.EAgentType agentType, string name)
    {
        return new ErmsAgent { AgentType = agentType, Name = name };
    }

    public static ErmsRecord FullRecord()
    {
        return new ErmsRecord
        {
            SystemIdentifier = "r-1",
            RecordType = "Anmälan",
            ClassificationSchemaUsed = "cs-skolan",
            ObjectId = "2026-0042-1",
            ExtraIds = [new ErmsExtraId { Type = "avsändarens diarienummer", Value = "KS 2026/17" }],
            Classifications = [new ErmsClassification { Value = "Hantera kränkande behandling", ClassificationCode = "2.3.1" }],
            Keywords = ["kränkning"],
            Title = "Anmälan om kränkande behandling av elev",
            OtherTitles = [new ErmsOtherTitle { TitleType = "offentlig titel", Value = "Anmälan" }],
            Status = ErmsEntity.EStatus.Received,
            RunningNumber = 1,
            Relations = [new ErmsRelation { RelationType = ErmsRelation.ERelationType.HasRedactedVersion, Value = "r-9" }],
            Restrictions = [new ErmsRestriction { RestrictionType = ErmsRestriction.ERestrictionType.OtherType, OtherRestrictionType = "sekretess", Regulation = "23 kap. 2 § OSL" }],
            Disposal = new ErmsDisposal { Disposable = false, DisposalMandate = "Skollagen 29 kap. 16 f §", Date = new ErmsDisposalDate { Value = new DateTime(2033, 12, 31, 0, 0, 0, DateTimeKind.Utc) } },
            Direction = new ErmsDirection { Definition = ErmsDirection.EDefinition.Incoming },
            Agents =
            [
                new ErmsAgent
                {
                    AgentType = ErmsAgent.EAgentType.Sender,
                    Name = "Anna Andersson",
                    Organisation = "Vårdnadshavare",
                    IdNumber = "19800101-1234",
                    IdNumberType = "personnummer",
                    AddressLines = [new ErmsAddressLine { AddressType = ErmsAddressLine.EAddressType.PostalAddress, Value = "Skolvägen 1" }],
                    ContactLines = [new ErmsContactLine { ContactType = ErmsContactLine.EContactType.Email, Value = "anna@example.se" }],
                    ProtectedIdentity = false,
                    Notes = [new ErmsNote { NoteType = "roll", Value = "Vårdnadshavare" }]
                }
            ],
            Description = "Inkom via e-post.",
            Dates = [new ErmsDate { DateType = ErmsDate.EDateType.Received, Value = Created }, new ErmsDate { DateType = ErmsDate.EDateType.Created, Value = Created }],
            Actions = [new ErmsAction { ActionText = "Registrerad", ActionType = "registrering", Dates = [new ErmsDate { DateType = ErmsDate.EDateType.Created, Value = Created }], Agents = [Person(ErmsAgent.EAgentType.Creator, "Rita Registrator")] }],
            Notes = [new ErmsNote { Value = "Original i e-post", NoteDate = Created }],
            AdditionalInformation = new ErmsAdditionalInformation
            {
                Appendices = [new ErmsAppendix { Name = "Anmälan.pdf", Path = "2026-0042-1_1_Anmalan.pdf", FileFormat = "pdf", Description = "SHA-256 0011" }],
                OwnElements = [new ErmsOwnElementSet { Description = "Kortmall", Elements = [new ErmsOwnElement { Name = "Klass", DataType = "string", Value = "7B" }] }]
            }
        };
    }

    public static ErmsAggregation FullCaseFile()
    {
        return new ErmsAggregation
        {
            SystemIdentifier = "c-1",
            AggregationType = ErmsAggregation.EAggregationType.CaseFile,
            ClassificationSchemaUsed = "cs-skolan",
            ObjectId = "2026-0042",
            ExtraIds = [new ErmsExtraId { Type = "referens", Value = "3f2504e0-4f89-11d3-9a0c-0305e82c3301" }],
            Classifications = [new ErmsClassification { Value = "Hantera kränkande behandling", ClassificationCode = "2.3.1", ClassificationId = "p-17" }, new ErmsClassification { Value = "Kränkande behandling", ClassificationCode = "ärendetyp" }],
            Keywords = ["kränkning", "elevhälsa"],
            Title = "Kränkande behandling av elev i 7B",
            OtherTitles = [new ErmsOtherTitle { TitleType = "offentlig ärendemening", Value = "Kränkande behandling" }],
            Status = ErmsEntity.EStatus.Closed,
            Relations = [new ErmsRelation { RelationType = ErmsRelation.ERelationType.Reference, Value = "2026-0040" }],
            AdditionalInformation = new ErmsAdditionalInformation { OwnElements = [new ErmsOwnElementSet { Elements = [new ErmsOwnElement { Name = "JK-balans", DataType = "boolean", Value = "true" }] }] },
            Restrictions = [new ErmsRestriction { RestrictionType = ErmsRestriction.ERestrictionType.OtherType, OtherRestrictionType = "sekretess", ExplanatoryText = "Elevhälsa", Regulation = "25 kap. 1 § OSL", Dates = [new ErmsDate { DateType = ErmsDate.EDateType.Start, Value = Created }] }],
            Disposal = new ErmsDisposal { Disposable = true, DisposalScheduleId = "gr-1", DisposalAction = "Gallras", DisposalPeriod = "84 månader", DisposalMandate = "Dokumenthanteringsplan", DisposalDescription = "Gallras efter sju år", Date = new ErmsDisposalDate { DateType = ErmsDisposalDate.EDateType.DisposalDate, Value = new DateTime(2033, 3, 14, 0, 0, 0, DateTimeKind.Utc) } },
            Agents = [Person(ErmsAgent.EAgentType.ResponsiblePerson, "Henrik Handläggare"), Person(ErmsAgent.EAgentType.Editor, "Hanna Handläggare"), new ErmsAgent { AgentType = ErmsAgent.EAgentType.Counterpart, Name = "Elev", ProtectedIdentity = true }],
            Description = "Ärendet avser en anmälan.",
            Dates = [new ErmsDate { DateType = ErmsDate.EDateType.Created, Value = Created }, new ErmsDate { DateType = ErmsDate.EDateType.Closed, Value = Created.AddDays(30) }],
            Actions = [new ErmsAction { ActionText = "Ärendet avslutades", ActionMotivation = "Utredning klar", Dates = [new ErmsDate { DateType = ErmsDate.EDateType.Closed, Value = Created.AddDays(30) }], Agents = [Person(ErmsAgent.EAgentType.ResponsiblePerson, "Henrik Handläggare")] }],
            Notes = [new ErmsNote { NoteType = "anteckning", Value = "Se även tidigare ärende." }],
            Records = [FullRecord()]
        };
    }

    public static ErmsDocument Full()
    {
        return new ErmsDocument { Control = Control(), Aggregations = [FullCaseFile()] };
    }
}
