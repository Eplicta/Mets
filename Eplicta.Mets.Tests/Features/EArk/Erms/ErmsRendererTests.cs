using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Eplicta.Mets.Features.EArk.Erms;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests.Features.EArk.Erms;

public class ErmsRendererTests
{
    private static string Errors(XmlDocument document)
    {
        return string.Join(Environment.NewLine, new ErmsValidator().Validate(document).Select(x => $"{x.XmlSeverityType}: {x.Message}"));
    }

    private static XmlNamespaceManager Namespaces(XmlDocument document)
    {
        var manager = new XmlNamespaceManager(document.NameTable);
        manager.AddNamespace("e", ErmsConstants.Namespace);
        return manager;
    }

    private static XmlElement Single(XmlDocument document, string xpath)
    {
        return (XmlElement)document.SelectSingleNode(xpath, Namespaces(document));
    }

    public static IEnumerable<object[]> EveryVocabularyTerm()
    {
        foreach (var value in Enum.GetValues<ErmsDate.EDateType>()) yield return [ErmsSamples.Minimal() with { Aggregations = [ErmsSamples.MinimalCaseFile() with { Dates = [new ErmsDate { DateType = value, OtherDateType = "annat", Value = ErmsSamples.Created }] }] }];
        foreach (var value in Enum.GetValues<ErmsAgent.EAgentType>()) yield return [ErmsSamples.Minimal() with { Aggregations = [ErmsSamples.MinimalCaseFile() with { Agents = [new ErmsAgent { AgentType = value, OtherAgentType = "annan", Name = "N" }] }] }];
        foreach (var value in Enum.GetValues<ErmsRelation.ERelationType>()) yield return [ErmsSamples.Minimal() with { Aggregations = [ErmsSamples.MinimalCaseFile() with { Relations = [new ErmsRelation { RelationType = value, OtherRelationType = "annan", Value = "x" }] }] }];
        foreach (var value in Enum.GetValues<ErmsRestriction.ERestrictionType>()) yield return [ErmsSamples.Minimal() with { Aggregations = [ErmsSamples.MinimalCaseFile() with { Restrictions = [new ErmsRestriction { RestrictionType = value, OtherRestrictionType = "annan", Regulation = "OSL" }] }] }];
        foreach (var value in Enum.GetValues<ErmsEntity.EStatus>()) yield return [ErmsSamples.Minimal() with { Aggregations = [ErmsSamples.MinimalCaseFile() with { Status = value }] }];
        foreach (var value in Enum.GetValues<ErmsAggregation.EAggregationType>()) yield return [ErmsSamples.Minimal() with { Aggregations = [ErmsSamples.MinimalCaseFile() with { AggregationType = value, OtherAggregationType = "serie" }] }];
        foreach (var value in Enum.GetValues<ErmsDirection.EDefinition>()) yield return [ErmsSamples.Minimal() with { Aggregations = [ErmsSamples.MinimalCaseFile() with { Records = [new ErmsRecord { SystemIdentifier = "r", ObjectId = "o", Direction = new ErmsDirection { Definition = value, OtherDefinition = "annan" } }] }] }];
        foreach (var value in Enum.GetValues<ErmsAddressLine.EAddressType>()) yield return [ErmsSamples.Minimal() with { Aggregations = [ErmsSamples.MinimalCaseFile() with { Agents = [new ErmsAgent { AgentType = ErmsAgent.EAgentType.Sender, Name = "N", AddressLines = [new ErmsAddressLine { AddressType = value, OtherAddressType = "annan", Value = "v" }] }] }] }];
        foreach (var value in Enum.GetValues<ErmsContactLine.EContactType>()) yield return [ErmsSamples.Minimal() with { Aggregations = [ErmsSamples.MinimalCaseFile() with { Agents = [new ErmsAgent { AgentType = ErmsAgent.EAgentType.Sender, Name = "N", ContactLines = [new ErmsContactLine { ContactType = value, OtherContactType = "annan", Value = "v" }] }] }] }];
        foreach (var value in Enum.GetValues<ErmsDisposalDate.EDateType>()) yield return [ErmsSamples.Minimal() with { Aggregations = [ErmsSamples.MinimalCaseFile() with { Disposal = new ErmsDisposal { Disposable = true, Date = new ErmsDisposalDate { DateType = value, OtherDateType = "annan", Value = ErmsSamples.Created } } }] }];
        foreach (var value in Enum.GetValues<ErmsControl.EMaintenanceStatus>()) yield return [ErmsSamples.Minimal() with { Control = ErmsSamples.Control() with { MaintenanceStatus = value } }];
        foreach (var value in Enum.GetValues<ErmsMaintenanceEvent.EEventType>()) yield return [ErmsSamples.Minimal() with { Control = ErmsSamples.Control() with { MaintenanceEvents = [new ErmsMaintenanceEvent { EventType = value, DateTime = ErmsSamples.Created, Agent = new ErmsAgent { AgentType = ErmsAgent.EAgentType.Creator, Name = "FortDocs" } }] } }];
        foreach (var value in Enum.GetValues<ErmsClassificationSchema.EStatus>().Where(x => x != ErmsClassificationSchema.EStatus.Other)) yield return [ErmsSamples.Minimal() with { Control = ErmsSamples.Control() with { ClassificationSchemas = [new ErmsClassificationSchema { Id = "cs", Status = value }] } }];
    }

    [Fact]
    public void A_minimal_case_file_validates_against_the_erms_schema()
    {
        Errors(new ErmsRenderer(ErmsSamples.Minimal()).Render()).Should().BeEmpty();
    }

    [Fact]
    public void A_case_file_with_every_element_we_model_validates_against_the_erms_schema()
    {
        Errors(new ErmsRenderer(ErmsSamples.Full()).Render()).Should().BeEmpty();
    }

    [Fact]
    public void A_document_of_records_without_aggregations_validates()
    {
        var document = new ErmsDocument { Control = ErmsSamples.Control(), Records = [ErmsSamples.FullRecord()] };

        Errors(new ErmsRenderer(document).Render()).Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(EveryVocabularyTerm))]
    public void Every_term_in_every_vocabulary_is_one_the_schema_accepts(ErmsDocument document)
    {
        Errors(new ErmsRenderer(document).Render()).Should().BeEmpty();
    }

    [Fact]
    public void The_document_is_in_the_erms_namespace_and_points_at_the_schemas_the_package_carries()
    {
        var document = new ErmsRenderer(ErmsSamples.Minimal()).Render();

        document.DocumentElement!.LocalName.Should().Be("erms");
        document.DocumentElement.NamespaceURI.Should().Be(ErmsConstants.Namespace);
        document.DocumentElement.GetAttribute("schemaLocation", "http://www.w3.org/2001/XMLSchema-instance").Should().Be("https://DILCIS.eu/XML/ERMS ../../../schemas/ERMS_v3.xsd");
        var model = document.ChildNodes.OfType<XmlProcessingInstruction>().Single(x => x.Name == "xml-model");
        model.Data.Should().Be("href=\"../../../schemas/erms_v3.sch\" type=\"application/xml\" schematypens=\"http://purl.oclc.org/dsdl/schematron\"");
    }

    [Fact]
    public void Vocabulary_terms_are_written_as_the_schema_spells_them()
    {
        var document = new ErmsRenderer(ErmsSamples.Full()).Render();

        Single(document, "//e:aggregation").GetAttribute("aggregationType").Should().Be("caseFile");
        Single(document, "//e:record/e:direction").GetAttribute("directionDefinition").Should().Be("incoming");
        Single(document, "//e:aggregation/e:restriction").GetAttribute("restrictionType").Should().Be("other_type");
        Single(document, "//e:aggregation/e:agents/e:agent").GetAttribute("agentType").Should().Be("responsible_person");
        Single(document, "//e:aggregation/e:status").GetAttribute("value").Should().Be("closed");
        new ErmsRenderer(ErmsSamples.Minimal() with { Aggregations = [ErmsSamples.MinimalCaseFile() with { Records = [new ErmsRecord { SystemIdentifier = "r", ObjectId = "o", Direction = new ErmsDirection { Definition = ErmsDirection.EDefinition.InternalMemoForFollowUp } }] }] })
            .Render().SelectSingleNode("//e:direction", Namespaces(document))!.Attributes!["directionDefinition"]!.Value.Should().Be("internal_memo_for_follow-up");
    }

    [Fact]
    public void Moments_are_written_as_utc_instants_and_days_as_days()
    {
        var local = new DateTime(2026, 3, 14, 10, 30, 0, DateTimeKind.Local);
        var document = new ErmsRenderer(ErmsSamples.Minimal() with { Aggregations = [ErmsSamples.MinimalCaseFile() with { Dates = [new ErmsDate { DateType = ErmsDate.EDateType.Created, Value = local }] }] }).Render();

        Single(document, "//e:aggregation/e:dates/e:date").InnerText.Should().Be(local.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss'Z'"));
        Single(document, "//e:classificationSchema/e:useDatePeriod/e:fromDate").InnerText.Should().Be("2026-01-01");
    }

    [Fact]
    public void A_record_lists_its_files_last_as_the_schema_orders_it()
    {
        var record = Single(new ErmsRenderer(ErmsSamples.Full()).Render(), "//e:record");

        record.ChildNodes.OfType<XmlElement>().Last().LocalName.Should().Be("additionalInformation");
        record.SelectSingleNode("e:additionalInformation/e:appendix", Namespaces(record.OwnerDocument))!.Attributes!["path"]!.Value.Should().Be("2026-0042-1_1_Anmalan.pdf");
    }

    [Fact]
    public void Nothing_empty_is_written_for_what_was_not_given()
    {
        var aggregation = Single(new ErmsRenderer(ErmsSamples.Minimal()).Render(), "//e:aggregation");

        aggregation.ChildNodes.OfType<XmlElement>().Select(x => x.LocalName).Should().Equal("objectId");
    }

    [Fact]
    public void A_document_with_both_aggregations_and_records_is_refused()
    {
        var document = ErmsSamples.Minimal() with { Records = [ErmsSamples.FullRecord()] };

        var act = () => new ErmsRenderer(document).Render();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void A_document_with_neither_aggregations_nor_records_is_refused()
    {
        var act = () => new ErmsRenderer(new ErmsDocument { Control = ErmsSamples.Control() }).Render();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void The_schema_and_schematron_ship_inside_the_library()
    {
        ErmsRenderer.Schema().Should().Contain("targetNamespace=\"https://DILCIS.eu/XML/ERMS\"");
        ErmsRenderer.Schematron().Should().Contain("http://purl.oclc.org/dsdl/schematron");
    }
}
