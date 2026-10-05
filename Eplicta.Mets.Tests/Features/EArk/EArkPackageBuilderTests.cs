using System;
using System.Linq;
using Eplicta.Mets.Features.EArk;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests.Features.EArk;

public class EArkPackageBuilderTests
{
    private static EArkPackageBuilder Minimal()
    {
        return new EArkPackageBuilder()
            .SetObjId("uuid-11111111-1111-1111-1111-111111111111")
            .SetSoftware("Eplicta WebConserver", "1.0.0")
            .AddAgent(EArkAgent.Submitter("Sjofartsverket"));
    }

    [Fact]
    public void Building_without_an_objid_fails()
    {
        var act = () => new EArkPackageBuilder().SetSoftware("tool", "1.0").AddAgent(EArkAgent.Submitter("org")).Build();

        act.Should().Throw<InvalidOperationException>().WithMessage("*ObjId*");
    }

    [Fact]
    public void Building_without_the_software_agent_fails()
    {
        var act = () => new EArkPackageBuilder().SetObjId("uuid-1").AddAgent(EArkAgent.Submitter("org")).Build();

        act.Should().Throw<InvalidOperationException>().WithMessage("*software*");
    }

    [Fact]
    public void Building_without_a_submitting_agent_fails()
    {
        var act = () => new EArkPackageBuilder().SetObjId("uuid-1").SetSoftware("tool", "1.0").Build();

        act.Should().Throw<InvalidOperationException>().WithMessage("*submitting*");
    }

    [Fact]
    public void The_software_agent_carries_the_fixed_csip_attribute_values()
    {
        var agent = Minimal().Build().Agents.Single(x => x.OtherType == EArkConstants.SoftwareAgentOtherType);

        agent.Role.Should().Be(EArkAgent.ERole.Creator);
        agent.Type.Should().Be(EArkAgent.EType.Other);
        agent.OtherType.Should().Be("SOFTWARE");
        agent.Name.Should().Be("Eplicta WebConserver");
        agent.Note.Should().Be("1.0.0");
        agent.NoteType.Should().Be(EArkAgent.ENoteType.SoftwareVersion);
    }

    [Fact]
    public void A_package_defaults_to_a_new_sip_of_web_archives()
    {
        var package = Minimal().Build();

        package.OaisPackageType.Should().Be(EArkPackage.EOaisPackageType.SIP);
        package.RecordStatus.Should().Be(EArkPackage.ERecordStatus.New);
        package.ContentCategory.Should().Be("Web Archives");
        package.RepresentationName.Should().Be("rep_1");
    }

    [Fact]
    public void A_create_date_is_supplied_when_the_caller_does_not_set_one()
    {
        var package = Minimal().Build();

        package.CreateDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void An_explicit_create_date_is_kept()
    {
        var created = new DateTime(2026, 3, 14, 9, 30, 0, DateTimeKind.Utc);

        Minimal().SetCreateDate(created).Build().CreateDate.Should().Be(created);
    }

    [Fact]
    public void Files_are_collected_into_their_own_groups()
    {
        var package = Minimal()
            .AddDocumentationFile(new EArkFile { Href = "documentation/report.xlsx", MimeType = "application/vnd.ms-excel" })
            .AddSchemaFile(new EArkFile { Href = "schemas/mets.xsd", MimeType = "text/xml" })
            .AddRepresentationFile(new EArkFile { Href = "representations/rep_1/data/webarchive.zip", MimeType = "application/zip" })
            .Build();

        package.DocumentationFiles.Should().ContainSingle();
        package.SchemaFiles.Should().ContainSingle();
        package.RepresentationFiles.Should().ContainSingle();
    }

    [Fact]
    public void Descriptive_and_preservation_metadata_are_kept_apart()
    {
        var package = Minimal()
            .AddDescriptiveMetadata(new EArkMetadataReference { Href = "metadata/descriptive/mods.xml", MdType = EArkMetadataReference.EMdType.MODS })
            .AddPreservationMetadata(new EArkMetadataReference { Href = "metadata/preservation/premis.xml", MdType = EArkMetadataReference.EMdType.PREMIS })
            .Build();

        package.DescriptiveMetadata.Should().ContainSingle(x => x.MdType == EArkMetadataReference.EMdType.MODS);
        package.PreservationMetadata.Should().ContainSingle(x => x.MdType == EArkMetadataReference.EMdType.PREMIS);
    }

    [Fact]
    public void Identifiers_are_generated_for_entries_that_do_not_carry_one()
    {
        var package = Minimal()
            .AddDocumentationFile(new EArkFile { Href = "documentation/report.xlsx" })
            .AddDescriptiveMetadata(new EArkMetadataReference { Href = "metadata/descriptive/mods.xml" })
            .Build();

        package.DocumentationFiles.Single().Id.Should().StartWith(EArkConstants.IdPrefix);
        package.DescriptiveMetadata.Single().Id.Should().StartWith(EArkConstants.IdPrefix);
    }

    [Fact]
    public void A_supplied_identifier_is_not_replaced()
    {
        var package = Minimal().AddDocumentationFile(new EArkFile { Id = "uuid-fixed", Href = "documentation/report.xlsx" }).Build();

        package.DocumentationFiles.Single().Id.Should().Be("uuid-fixed");
    }

    [Fact]
    public void The_riksarkivet_adaptation_is_absent_unless_it_is_asked_for()
    {
        Minimal().Build().AdditionalPackageInfo.Should().BeNull();
    }

    [Fact]
    public void The_riksarkivet_adaptation_is_carried_when_set()
    {
        var package = Minimal().SetAdditionalPackageInfo(new AdditionalPackageInfo { ArchiveName = "Sjofartsverket 2026" }).Build();

        package.AdditionalPackageInfo.ArchiveName.Should().Be("Sjofartsverket 2026");
    }
}
