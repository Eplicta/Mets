using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Eplicta.Mets.Features.EArk.Erms;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests.Features.EArk.Erms;

public class ErmsValidatorTests
{
    private static string[] Errors(ErmsDocument document)
    {
        return Errors(new ErmsRenderer(document).Render());
    }

    private static string[] Errors(XmlDocument document)
    {
        return new ErmsValidator().Validate(document).Select(x => x.Message).ToArray();
    }

    private static ErmsDocument WithCaseFile(Func<ErmsAggregation, ErmsAggregation> change)
    {
        return ErmsSamples.Minimal() with { Aggregations = [change(ErmsSamples.MinimalCaseFile())] };
    }

    public static IEnumerable<object[]> AnOtherWithoutSayingWhat()
    {
        yield return ["otherAggregationType", WithCaseFile(x => x with { AggregationType = ErmsAggregation.EAggregationType.OwnAggregationDefinition })];
        yield return ["otherDateType", WithCaseFile(x => x with { Dates = [new ErmsDate { DateType = ErmsDate.EDateType.Other, Value = ErmsSamples.Created }] })];
        yield return ["otherDirectionDefinition", WithCaseFile(x => x with { Records = [new ErmsRecord { SystemIdentifier = "r", ObjectId = "o", Direction = new ErmsDirection { Definition = ErmsDirection.EDefinition.Other } }] })];
        yield return ["otherAddressLineType", WithCaseFile(x => x with { Agents = [new ErmsAgent { AgentType = ErmsAgent.EAgentType.Sender, Name = "N", AddressLines = [new ErmsAddressLine { AddressType = ErmsAddressLine.EAddressType.Other, Value = "v" }] }] })];
        yield return ["otherContactLineType", WithCaseFile(x => x with { Agents = [new ErmsAgent { AgentType = ErmsAgent.EAgentType.Sender, Name = "N", ContactLines = [new ErmsContactLine { ContactType = ErmsContactLine.EContactType.Other, Value = "v" }] }] })];
        yield return ["otherAgentType", WithCaseFile(x => x with { Agents = [new ErmsAgent { AgentType = ErmsAgent.EAgentType.Other, Name = "N" }] })];
        yield return ["otherDisposalDateType", WithCaseFile(x => x with { Disposal = new ErmsDisposal { Disposable = true, Date = new ErmsDisposalDate { DateType = ErmsDisposalDate.EDateType.OtherDate, Value = ErmsSamples.Created } } })];
        yield return ["otherRestrictionType", WithCaseFile(x => x with { Restrictions = [new ErmsRestriction { RestrictionType = ErmsRestriction.ERestrictionType.OtherType, Regulation = "OSL" }] })];
        yield return ["otherRelationType", WithCaseFile(x => x with { Relations = [new ErmsRelation { RelationType = ErmsRelation.ERelationType.OwnRelationDefinition, Value = "x" }] })];
        yield return ["otherClassificationSchemaStatus", ErmsSamples.Minimal() with { Control = ErmsSamples.Control() with { ClassificationSchemas = [new ErmsClassificationSchema { Id = "cs", Status = ErmsClassificationSchema.EStatus.Other }] } }];
    }

    [Theory]
    [MemberData(nameof(AnOtherWithoutSayingWhat))]
    public void An_other_term_without_saying_what_it_is_is_reported_as_the_schematron_requires(string attribute, ErmsDocument document)
    {
        Errors(document).Should().ContainSingle(x => x.Contains(attribute, StringComparison.Ordinal));
    }

    [Fact]
    public void An_address_and_contact_element_with_nothing_in_it_is_reported()
    {
        var document = new ErmsRenderer(WithCaseFile(x => x with { Agents = [new ErmsAgent { AgentType = ErmsAgent.EAgentType.Sender, Name = "N", ContactLines = [new ErmsContactLine { ContactType = ErmsContactLine.EContactType.Email, Value = "a@b.se" }] }] })).Render();
        var contact = document.GetElementsByTagName("contactLine", ErmsConstants.Namespace)[0]!;
        contact.ParentNode!.RemoveChild(contact);

        Errors(document).Should().ContainSingle(x => x.Contains("addressContactInformation", StringComparison.Ordinal));
    }

    [Fact]
    public void A_missing_required_element_is_reported_by_the_schema()
    {
        Errors(WithCaseFile(x => x with { ObjectId = null })).Should().NotBeEmpty();
    }

    [Fact]
    public void A_classification_schema_id_with_spaces_is_reported_because_it_is_an_xs_id()
    {
        var document = ErmsSamples.Minimal() with { Control = ErmsSamples.Control() with { ClassificationSchemas = [new ErmsClassificationSchema { Id = "Klassificeringsstruktur 2026", Status = ErmsClassificationSchema.EStatus.Current }] } };

        Errors(document).Should().NotBeEmpty();
    }

    [Fact]
    public void A_well_formed_document_that_is_not_erms_is_reported_rather_than_thrown()
    {
        var document = new XmlDocument();
        document.LoadXml("<other/>");

        Errors(document).Should().NotBeEmpty();
    }
}
