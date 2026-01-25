using System;
using AutoFixture;
using Eplicta.Mets.Entities;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests;

public class RendererTests
{
    private const string DefaultBuilderData = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><mets xmlns=\"http://www.loc.gov/METS/\" xmlns:mods=\"http://www.loc.gov/mods/v3\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" OBJID=\"\" TYPE=\"SIP\" PROFILE=\"http://www.kb.se/namespace/mets/fgs/eARD_Paket_FGS-PUBL.xml\"><metsHdr CREATEDATE=\"0001-01-01T00:00:00.0000000\"><agent ROLE=\"CREATOR\" TYPE=\"INDIVIDUAL\"><name></name></agent><agent ROLE=\"CREATOR\" TYPE=\"INDIVIDUAL\" OTHERTYPE=\"SOFTWARE\"><name></name></agent><altRecordID></altRecordID><altRecordID></altRecordID><altRecordID></altRecordID><metsDocumentID ID=\"sip.xml\" /></metsHdr><fileSec><fileGrp><file ID=\"IDD41D8CD98F00B204E9800998ECF8427E\" USE=\"\" MIMETYPE=\"\" SIZE=\"0\" CREATED=\"0001-01-01T00:00:00.0000000\" CHECKSUM=\"1B2M2Y8AsgTpgAmY7PhCfg==\" CHECKSUMTYPE=\"MD5\"><FLocat LOCTYPE=\"URL\" xlink:href=\"file:///\" xlink:type=\"simple\" /></file></fileGrp></fileSec><structMap TYPE=\"physical\"><div TYPE=\"files\"><div TYPE=\"publication\"><fptr FILEID=\"IDD41D8CD98F00B204E9800998ECF8427E\" /></div></div></structMap></mets>";
    private const string DefaultNewData = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><mets xmlns=\"http://www.loc.gov/METS/\" xmlns:mods=\"http://www.loc.gov/mods/v3\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" OBJID=\"\" TYPE=\"SIP\" PROFILE=\"http://www.kb.se/namespace/mets/fgs/eARD_Paket_FGS-PUBL.xml\"><metsHdr CREATEDATE=\"0001-01-01T00:00:00.0000000\" /><dmdSec ID=\"ID1\"><mdWrap MDTYPE=\"MODS\"><xmlData /></mdWrap></dmdSec><fileSec><fileGrp /></fileSec><structMap LABEL=\"No structmap defined in this information package\"><div /></structMap></mets>";

    [Fact]
    public void Basic()
    {
        //Arrange
        var metsData = new Fixture().Build<MetsData>().Without(x => x.Sources).Create();
        var sut = new Renderer(metsData);

        //Act
        var result = sut.Render();

        //Assert
        result.Should().NotBeNull();
        result.OuterXml.Should().NotBe(DefaultBuilderData);
    }

    [Fact(Skip = "Creating without builder is not supported")]
    public void Empty()
    {
        //Arrange
        var modsData = new MetsData();
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
            .AddAltRecord(new MetsData.AltRecord())
            .AddAltRecord(new MetsData.AltRecord())
            .AddAltRecord(new MetsData.AltRecord())
            .AddMetsAttributes([new MetsData.MetsAttribute { Name = MetsData.EMetsAttributeName.ObjId, Value = string.Empty }])
            .AddFile(new DataFileSource { Data = []})
            .Build();
        var sut = new Renderer(metsData);

        //Act
        var result = sut.Render(DateTime.MinValue);

        //Assert
        result.Should().NotBeNull();
        result.OuterXml.Should().Be(DefaultBuilderData);
    }

    [Fact]
    public void AgentData()
    {
        //Arrange
        var metsData = new Builder()
            .SetAgent(new MetsData.AgentData
            {
                Note = "a1",
                Type = MetsData.EType.Other,
                Name = "a3",
                Role = MetsData.ERole.Editor
            })
            .AddAltRecord(new MetsData.AltRecord())
            .AddAltRecord(new MetsData.AltRecord())
            .AddAltRecord(new MetsData.AltRecord())
            .AddMetsAttributes([new MetsData.MetsAttribute { Name = MetsData.EMetsAttributeName.ObjId, Value = string.Empty }])
            .AddFile(new DataFileSource { Data = [] })
            .Build();
        var sut = new Renderer(metsData);

        //Act
        var result = sut.Render(DateTime.MinValue);

        //Assert
        result.Should().NotBeNull();
        result.OuterXml.Should().NotBe(DefaultBuilderData);
        result.OuterXml.Should().Contain("a1");
        result.OuterXml.Should().Contain(MetsData.EType.Other.ToString().ToUpper());
        result.OuterXml.Should().Contain("a3");
        result.OuterXml.Should().Contain(MetsData.ERole.Editor.ToString().ToUpper());
    }

    //[Fact]
    //public void CompanyData()
    //{
    //    //Arrange
    //    var metsData = new Builder()
    //        //.SetCompany(new MetsData.CompanyData
    //        //{
    //        //    Note = "a1",
    //        //    Type = MetsData.EType.Other,
    //        //    Name = "a3",
    //        //    Role = MetsData.ERole.Editor
    //        //})
    //        .AddAltRecord(new MetsData.AltRecord())
    //        .AddAltRecord(new MetsData.AltRecord())
    //        .AddAltRecord(new MetsData.AltRecord())
    //        .AddMetsAttributes([new MetsData.MetsAttribute { Name = MetsData.EMetsAttributeName.ObjId, Value = string.Empty }])
    //        .AddFile(new DataFileSource { Data = [] })
    //        .Build();
    //    var sut = new Renderer(metsData);

    //    //Act
    //    var result = sut.Render(DateTime.MinValue);

    //    //Assert
    //    result.Should().NotBeNull();
    //    result.OuterXml.Should().NotBe(DefaultBuilderData);
    //    //result.OuterXml.Should().Contain("a1");
    //    result.OuterXml.Should().Contain(MetsData.EType.Other.ToString().ToUpper());
    //    result.OuterXml.Should().Contain("a3");
    //    result.OuterXml.Should().Contain(MetsData.ERole.Editor.ToString().ToUpper());
    //}

    [Fact]
    public void AltRecord()
    {
        //Arrange
        var metsData = new Builder()
            .AddAltRecord(new MetsData.AltRecord
            {
                Type = MetsData.EAltRecordType.DeliverySpecification,
                InnerText = "a1"
            })
            .AddAltRecord(new MetsData.AltRecord
            {
                Type = MetsData.EAltRecordType.SubmissionAgreement,
                InnerText = "a2"
            })
            .AddAltRecord(new MetsData.AltRecord
            {
                Type = MetsData.EAltRecordType.PreviousSubmissionAgreement,
                InnerText = "a3"
            })
            .AddFile(new DataFileSource { Data = [] })
            .AddMetsAttributes([new MetsData.MetsAttribute { Name = MetsData.EMetsAttributeName.ObjId, Value = string.Empty }])
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
        result.OuterXml.Should().Contain(MetsData.EAltRecordType.DeliverySpecification.ToString().ToUpper());
        result.OuterXml.Should().Contain(MetsData.EAltRecordType.SubmissionAgreement.ToString().ToUpper());
        result.OuterXml.Should().Contain(MetsData.EAltRecordType.PreviousSubmissionAgreement.ToString().ToUpper());
    }

    [Fact]
    public void ModsSection()
    {
        //Arrange
        var metsData = new Builder()
            .SetModsSection(new MetsData.ModsSectionData
            {
                Identifier = "a1",
                Uri = new Uri("http://aaa.bbb"),
                Url = new Uri("http://ccc.ddd"),
            })
            .AddAltRecord(new MetsData.AltRecord())
            .AddAltRecord(new MetsData.AltRecord())
            .AddAltRecord(new MetsData.AltRecord())
            .AddMetsAttributes([new MetsData.MetsAttribute { Name = MetsData.EMetsAttributeName.ObjId, Value = string.Empty }])
            .AddFile(new DataFileSource { Data = [] })
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