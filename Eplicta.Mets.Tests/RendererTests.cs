using System;
using AutoFixture;
using Eplicta.Mets.Entities;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests;

public class RendererTests
{
    private const string DefaultBuilderData = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><mets xmlns=\"http://www.loc.gov/METS/\" xmlns:mods=\"http://www.loc.gov/mods/v3\" xmlns:ns2=\"http://www.w3.org/1999/xlink\" OBJID=\"\" TYPE=\"SIP\" PROFILE=\"http://www.kb.se/namespace/mets/fgs/eARD_Paket_FGS-PUBL.xml\"><metsHdr CREATEDATE=\"0001-01-01T00:00:00.0000000\"><agent ROLE=\"CREATOR\" TYPE=\"INDIVIDUAL\"><name></name><note></note></agent><agent ROLE=\"CREATOR\" TYPE=\"INDIVIDUAL\"><name></name><note></note></agent><agent ROLE=\"CREATOR\" TYPE=\"INDIVIDUAL\" OTHERTYPE=\"SOFTWARE\"><name></name></agent><altRecordID TYPE=\"DELIVERYTYPE\"></altRecordID><altRecordID TYPE=\"DELIVERYTYPE\"></altRecordID><altRecordID TYPE=\"DELIVERYTYPE\"></altRecordID></metsHdr><dmdSec ID=\"ID1\"><mdWrap MDTYPE=\"MODS\"><xmlData><mods:mods xmlns=\"\"><mods:location><mods:url>https://some.url</mods:url></mods:location><mods:originInfo><mods:dateIssued encoding=\"w3cdtf\">0001-01-01T00:00:00.0000000</mods:dateIssued></mods:originInfo><mods:accessCondition></mods:accessCondition><mods:titleInfo><mods:title>Unknown</mods:title></mods:titleInfo><mods:relatedItem type=\"host\"><mods:titleInfo><mods:title>Unknown</mods:title></mods:titleInfo></mods:relatedItem></mods:mods></xmlData></mdWrap></dmdSec><fileSec><fileGrp><file ID=\"IDd41d8cd9-8f00-b204-e980-0998ecf8427e\" USE=\"\" MIMETYPE=\"\" SIZE=\"0\" CREATED=\"0001-01-01T00:00:00.0000000\" CHECKSUM=\"1B2M2Y8AsgTpgAmY7PhCfg==\" CHECKSUMTYPE=\"MD5\"><FLocat LOCTYPE=\"URL\" ns2:href=\"file:\" ns2:type=\"simple\" /></file></fileGrp></fileSec><structMap TYPE=\"physical\"><div TYPE=\"files\"><div TYPE=\"publication\"><fptr FILEID=\"IDd41d8cd9-8f00-b204-e980-0998ecf8427e\" /></div></div></structMap></mets>";
    private const string DefaultNewData = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><mets xmlns=\"http://www.loc.gov/METS/\" xmlns:mods=\"http://www.loc.gov/mods/v3\" xmlns:ns2=\"http://www.w3.org/1999/xlink\" TYPE=\"SIP\" PROFILE=\"http://www.kb.se/namespace/mets/fgs/eARD_Paket_FGS-PUBL.xml\"><metsHdr CREATEDATE=\"0001-01-01T00:00:00.0000000\" /><dmdSec ID=\"ID1\"><mdWrap MDTYPE=\"MODS\"><xmlData /></mdWrap></dmdSec><fileSec><fileGrp /></fileSec></mets>";

    [Fact]
    public void Basic()
    {
        //Arrange
        var metsData = new Fixture().Build<ModsData>().Create();
        var sut = new Renderer(metsData);

        //Act
        var result = sut.Render();

        //Assert
        result.Should().NotBeNull();
        result.OuterXml.Should().NotBe(DefaultBuilderData);
    }

    [Fact]
    public void Empty()
    {
        //Arrange
        var modsData = new ModsData();
        var sut = new Renderer(modsData);

        //Act
        var result = sut.Render(DateTime.MinValue);

        //Assert
        result.Should().NotBeNull();
        result.OuterXml.Should().Be(DefaultNewData);
    }

    [Fact]
    public void Empty_builder()
    {
        //Arrange
        var metsData = new Builder()
            .AddAltRecord(new ModsData.AltRecord())
            .AddAltRecord(new ModsData.AltRecord())
            .AddAltRecord(new ModsData.AltRecord())
            .AddFile(new FileSource { Data = Array.Empty<byte>()})
            .Build();
        var sut = new Renderer(metsData);

        //Act
        var result = sut.Render(DateTime.MinValue);

        //Assert
        result.Should().NotBeNull();
        var xxx = result.OuterXml;
        result.OuterXml.Should().Be(DefaultBuilderData);
    }

    [Fact]
    public void AgentData()
    {
        //Arrange
        var metsData = new Builder()
            .SetAgent(new ModsData.AgentData
            {
                Note = "a1",
                Type = ModsData.EType.Other,
                Name = "a3",
                Role = ModsData.ERole.Editor
            })
            .AddAltRecord(new ModsData.AltRecord())
            .AddAltRecord(new ModsData.AltRecord())
            .AddAltRecord(new ModsData.AltRecord())
            .AddFile(new FileSource { Data = Array.Empty<byte>() })
            .Build();
        var sut = new Renderer(metsData);

        //Act
        var result = sut.Render(DateTime.MinValue);

        //Assert
        result.Should().NotBeNull();
        result.OuterXml.Should().NotBe(DefaultBuilderData);
        result.OuterXml.Should().Contain("a1");
        result.OuterXml.Should().Contain(ModsData.EType.Other.ToString().ToUpper());
        result.OuterXml.Should().Contain("a3");
        result.OuterXml.Should().Contain(ModsData.ERole.Editor.ToString().ToUpper());
    }

    [Fact]
    public void CompanyData()
    {
        //Arrange
        var metsData = new Builder()
            .SetCompany(new ModsData.CompanyData
            {
                Note = "a1",
                Type = ModsData.EType.Other,
                Name = "a3",
                Role = ModsData.ERole.Editor
            })
            .AddAltRecord(new ModsData.AltRecord())
            .AddAltRecord(new ModsData.AltRecord())
            .AddAltRecord(new ModsData.AltRecord())
            .AddFile(new FileSource { Data = Array.Empty<byte>() })
            .Build();
        var sut = new Renderer(metsData);

        //Act
        var result = sut.Render(DateTime.MinValue);

        //Assert
        result.Should().NotBeNull();
        result.OuterXml.Should().NotBe(DefaultBuilderData);
        result.OuterXml.Should().Contain("a1");
        result.OuterXml.Should().Contain(ModsData.EType.Other.ToString().ToUpper());
        result.OuterXml.Should().Contain("a3");
        result.OuterXml.Should().Contain(ModsData.ERole.Editor.ToString().ToUpper());
    }

    [Fact]
    public void AltRecord()
    {
        //Arrange
        var metsData = new Builder()
            .AddAltRecord(new ModsData.AltRecord
            {
                Type = ModsData.EAltRecordType.DeliverySpecification,
                InnerText = "a1"
            })
            .AddAltRecord(new ModsData.AltRecord
            {
                Type = ModsData.EAltRecordType.SubmissionAgreement,
                InnerText = "a2"
            })
            .AddAltRecord(new ModsData.AltRecord
            {
                Type = ModsData.EAltRecordType.PreviousSubmissionAgreement,
                InnerText = "a3"
            })
            .AddFile(new FileSource { Data = Array.Empty<byte>() })
            .Build();
        var sut = new Renderer(metsData);

        //Act
        var result = sut.Render(DateTime.MinValue);

        //Assert
        result.Should().NotBeNull();
        result.OuterXml.Should().NotBe(DefaultBuilderData);
        result.OuterXml.Should().Contain("a1");
        result.OuterXml.Should().Contain("a2");
        result.OuterXml.Should().Contain("a3");
        result.OuterXml.Should().Contain(ModsData.EAltRecordType.DeliverySpecification.ToString().ToUpper());
        result.OuterXml.Should().Contain(ModsData.EAltRecordType.SubmissionAgreement.ToString().ToUpper());
        result.OuterXml.Should().Contain(ModsData.EAltRecordType.PreviousSubmissionAgreement.ToString().ToUpper());
    }

    [Fact]
    public void ModsSection()
    {
        //Arrange
        var metsData = new Builder()
            .SetModsSection(new ModsData.ModsSectionData
            {
                Identifier = "a1",
                Uri = new Uri("http://aaa.bbb"),
                Url = new Uri("http://ccc.ddd"),
            })
            .AddAltRecord(new ModsData.AltRecord())
            .AddAltRecord(new ModsData.AltRecord())
            .AddAltRecord(new ModsData.AltRecord())
            .AddFile(new FileSource { Data = Array.Empty<byte>() })
            .Build();
        var sut = new Renderer(metsData);

        //Act
        var result = sut.Render(DateTime.MinValue);

        //Assert
        result.Should().NotBeNull();
        result.OuterXml.Should().NotBe(DefaultBuilderData);
        result.OuterXml.Should().Contain("a1");
        result.OuterXml.Should().Contain("http://aaa.bbb");
        result.OuterXml.Should().Contain("http://ccc.ddd");
    }
}