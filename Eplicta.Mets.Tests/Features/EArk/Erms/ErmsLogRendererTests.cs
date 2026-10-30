using System;
using System.Linq;
using System.Xml;
using Eplicta.Mets.Features.EArk.Erms;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests.Features.EArk.Erms;

public class ErmsLogRendererTests
{
    private static ErmsLogDocument Minimal()
    {
        return new ErmsLogDocument
        {
            Identification = "71a4e2a1-f2ad-417a-98b9-ae2b821288bf",
            CreationDate = ErmsSamples.Created,
            CreatorName = "Futuraskolan AB",
            CaseLogs =
            [
                new ErmsCaseLog
                {
                    Id = "2026-0042",
                    Modifications = [new ErmsLogModification { Number = "1", Date = ErmsSamples.Created, Agent = "Henrik Handläggare", CaseFields = [new ErmsLogField { Name = "Ärendemening", OriginalValue = "A", NewValue = "B" }] }]
                }
            ]
        };
    }

    private static ErmsLogDocument Full()
    {
        return Minimal() with
        {
            CreatorCode = "5560000000",
            CreatorCodeType = ErmsLogDocument.ECreatorCodeType.Org,
            CreatorDescription = "Skolhuvudman",
            CaseLogs =
            [
                new ErmsCaseLog
                {
                    Id = "2026-0042",
                    Modifications =
                    [
                        new ErmsLogModification
                        {
                            Number = "1",
                            Date = ErmsSamples.Created,
                            Agent = "Henrik Handläggare",
                            CaseFields = [new ErmsLogField { Name = "Ärendemening", OriginalValue = "N/A", NewValue = "Kränkande behandling" }],
                            RecordChanges = [new ErmsLogRecordChange { Id = "2026-0042-3", Name = "Yttrande", Summary = "Riktning", Fields = [new ErmsLogField { Name = "Riktning", OriginalValue = "N/A", NewValue = "Upprättad" }] }]
                        },
                        new ErmsLogModification { Number = "2", Date = ErmsSamples.Created.AddHours(1), Agent = "Rita Registrator", RecordChanges = [new ErmsLogRecordChange { Id = "2026-0042-1", Name = "Anmälan", Fields = [new ErmsLogField { Name = "Sekretess", OriginalValue = "", NewValue = "23 kap. 2 § OSL" }] }] }
                    ]
                }
            ]
        };
    }

    private static string Errors(XmlDocument document)
    {
        return string.Join(Environment.NewLine, new ErmsLogValidator().Validate(document).Select(x => $"{x.XmlSeverityType}: {x.Message}"));
    }

    [Fact]
    public void A_single_change_validates_against_riksarkivets_log_schema()
    {
        Errors(new ErmsLogRenderer(Minimal()).Render()).Should().BeEmpty();
    }

    [Fact]
    public void Case_and_record_changes_with_everything_we_model_validate()
    {
        Errors(new ErmsLogRenderer(Full()).Render()).Should().BeEmpty();
    }

    [Theory]
    [InlineData(ErmsLogDocument.ECreatorCodeType.Vat)]
    [InlineData(ErmsLogDocument.ECreatorCodeType.Duns)]
    [InlineData(ErmsLogDocument.ECreatorCodeType.Org)]
    [InlineData(ErmsLogDocument.ECreatorCodeType.Hsa)]
    [InlineData(ErmsLogDocument.ECreatorCodeType.Local)]
    [InlineData(ErmsLogDocument.ECreatorCodeType.Uri)]
    [InlineData(ErmsLogDocument.ECreatorCodeType.Other)]
    public void Every_creator_code_type_is_one_the_schema_accepts(ErmsLogDocument.ECreatorCodeType type)
    {
        Errors(new ErmsLogRenderer(Minimal() with { CreatorCode = "1", CreatorCodeType = type }).Render()).Should().BeEmpty();
    }

    [Fact]
    public void The_document_is_in_riksarkivets_namespace_and_points_at_the_schema_the_package_carries()
    {
        var root = new ErmsLogRenderer(Minimal()).Render().DocumentElement!;

        root.LocalName.Should().Be("ermsLogs");
        root.NamespaceURI.Should().Be("http://xml.ra.se/e-arkiv/implementation-ERMS/version10/SNAermsLogs");
        root.GetAttribute("schemaLocation", "http://www.w3.org/2001/XMLSchema-instance").Should().Be("http://xml.ra.se/e-arkiv/implementation-ERMS/version10/SNAermsLogs ../../../schemas/SNAermsLogs.xsd");
    }

    [Fact]
    public void An_empty_previous_value_is_written_as_an_empty_field_rather_than_left_out()
    {
        var document = new ErmsLogRenderer(Full()).Render();
        var manager = new XmlNamespaceManager(document.NameTable);
        manager.AddNamespace("l", ErmsConstants.LogNamespace);

        document.SelectSingleNode("//l:recordMod[@id='2026-0042-1']/l:modField/l:originalValueField", manager)!.InnerText.Should().BeEmpty();
    }

    [Fact]
    public void A_case_without_changes_is_reported_since_the_schema_requires_at_least_one()
    {
        Errors(new ErmsLogRenderer(Minimal() with { CaseLogs = [new ErmsCaseLog { Id = "2026-0042" }] }).Render()).Should().NotBeEmpty();
    }
}
