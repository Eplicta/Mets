using System;
using System.Xml;
using AutoFixture;
using Eplicta.Mets.Entities;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests;

public class RendererTests
{
    private const string MetsNs = "http://www.loc.gov/METS/";
    private const string ModsNs = "http://www.loc.gov/mods/v3";

    [Fact]
    public void Empty_package_emits_div_in_mets_namespace()
    {
        // EP-4404: a package with no files and no sources takes the "empty package" branch, which created
        // the structMap's child <div> in the empty namespace instead of the METS namespace — breaking XSD
        // validation. The div must live in http://www.loc.gov/METS/ like every other METS element.
        var metsData = new Builder()
            .AddMetsAttributes([new MetsData.MetsAttribute { Name = MetsData.EMetsAttributeName.ObjId, Value = string.Empty }])
            .Build();
        var document = new Renderer(metsData).Render(DateTime.MinValue);

        var nsmgr = new XmlNamespaceManager(document.NameTable);
        nsmgr.AddNamespace("mets", MetsNs);

        // The empty-package structMap is the one carrying LABEL (the populated one uses TYPE="Package").
        var div = document.SelectSingleNode("//mets:structMap[@LABEL]/mets:div", nsmgr);
        div.Should().NotBeNull("the empty-package structMap <div> must be in the METS namespace");
        div.NamespaceURI.Should().Be(MetsNs);
    }

    private const string DefaultBuilderData = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><mets:mets xmlns=\"http://www.loc.gov/METS/\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" OBJID=\"\" TYPE=\"SIP\" xmlns:mets=\"http://www.loc.gov/METS/\"><mets:metsHdr CREATEDATE=\"0001-01-01T00:00:00Z\"><mets:agent ROLE=\"CREATOR\" TYPE=\"INDIVIDUAL\" OTHERTYPE=\"SOFTWARE\"><mets:name></mets:name></mets:agent><mets:altRecordID></mets:altRecordID><mets:altRecordID></mets:altRecordID><mets:altRecordID></mets:altRecordID><mets:altRecordID TYPE=\"SUBMISSIONAGREEMENT\">OK</mets:altRecordID><mets:altRecordID>SIP</mets:altRecordID><mets:altRecordID>sip.xml</mets:altRecordID></mets:metsHdr><mets:fileSec><mets:fileGrp USE=\"FILES\"><mets:file ID=\"IDD41D8CD98F00B204E9800998ECF8427E\" USE=\"\" MIMETYPE=\"\" SIZE=\"0\" CREATED=\"0001-01-01T00:00:00Z\" CHECKSUM=\"1B2M2Y8AsgTpgAmY7PhCfg==\" CHECKSUMTYPE=\"MD5\"><mets:FLocat LOCTYPE=\"URL\" xlink:href=\"file:///\" xlink:type=\"simple\" /></mets:file></mets:fileGrp></mets:fileSec><mets:structMap TYPE=\"Package\"><mets:div TYPE=\"DataFiles\"><mets:fptr FILEID=\"IDD41D8CD98F00B204E9800998ECF8427E\" /></mets:div></mets:structMap></mets:mets>";
    private const string DefaultNewData = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><mets xmlns=\"http://www.loc.gov/METS/\" xmlns:mods=\"http://www.loc.gov/mods/v3\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" OBJID=\"\" TYPE=\"SIP\" ><metsHdr CREATEDATE=\"0001-01-01T00:00:00.0000000\" /><dmdSec ID=\"ID1\"><mdWrap MDTYPE=\"MODS\"><xmlData /></mdWrap></dmdSec><fileSec><fileGrp /></fileSec><structMap LABEL=\"No structmap defined in this information package\"><div /></structMap></mets>";

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
            .AddAgent(new MetsData.AgentData
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
        result.OuterXml.Should().Contain(nameof(MetsData.EType.Other).ToUpper());
        result.OuterXml.Should().Contain("a3");
        result.OuterXml.Should().Contain(nameof(MetsData.ERole.Editor).ToUpper());
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
        result.OuterXml.Should().Contain(nameof(MetsData.EAltRecordType.DeliverySpecification).ToUpper());
        result.OuterXml.Should().Contain(nameof(MetsData.EAltRecordType.SubmissionAgreement).ToUpper());
        result.OuterXml.Should().Contain(nameof(MetsData.EAltRecordType.PreviousSubmissionAgreement).ToUpper());
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

    [Fact]
    public void CreateDate_in_local_time_renders_the_utc_instant()
    {
        //Arrange
        var localNow = new DateTime(2026, 8, 28, 13, 21, 5, DateTimeKind.Utc).ToLocalTime();
        var metsData = new Builder()
            .AddMetsAttributes([new MetsData.MetsAttribute { Name = MetsData.EMetsAttributeName.ObjId, Value = string.Empty }])
            .Build();
        var sut = new Renderer(metsData);

        //Act
        var result = sut.Render(localNow);

        //Assert
        var nsmgr = new XmlNamespaceManager(result.NameTable);
        nsmgr.AddNamespace("mets", MetsNs);
        result.SelectSingleNode("//mets:metsHdr", nsmgr).Attributes["CREATEDATE"].Value.Should().Be("2026-08-28T13:21:05Z");
    }

    [Fact]
    public void CreateDate_with_unspecified_kind_is_not_shifted_by_the_host_time_zone()
    {
        //Arrange
        var unspecified = new DateTime(2026, 8, 28, 13, 21, 5, DateTimeKind.Unspecified);
        var metsData = new Builder()
            .AddMetsAttributes([new MetsData.MetsAttribute { Name = MetsData.EMetsAttributeName.ObjId, Value = string.Empty }])
            .Build();
        var sut = new Renderer(metsData);

        //Act
        var result = sut.Render(unspecified);

        //Assert
        var nsmgr = new XmlNamespaceManager(result.NameTable);
        nsmgr.AddNamespace("mets", MetsNs);
        result.SelectSingleNode("//mets:metsHdr", nsmgr).Attributes["CREATEDATE"].Value.Should().Be("2026-08-28T13:21:05Z");
    }

    [Fact]
    public void DateIssued_in_local_time_renders_the_utc_instant()
    {
        //Arrange
        var localDateIssued = new DateTime(2026, 8, 28, 13, 21, 5, DateTimeKind.Utc).ToLocalTime();
        var metsData = new Builder()
            .SetModsSection(new MetsData.ModsSectionData { DateIssued = localDateIssued, Url = new Uri("http://ccc.ddd") })
            .AddMetsAttributes([new MetsData.MetsAttribute { Name = MetsData.EMetsAttributeName.ObjId, Value = string.Empty }])
            .Build();
        var sut = new Renderer(metsData);

        //Act
        var result = sut.Render(DateTime.MinValue);

        //Assert
        var nsmgr = new XmlNamespaceManager(result.NameTable);
        nsmgr.AddNamespace("mods", ModsNs);
        result.SelectSingleNode("//mods:dateIssued", nsmgr).InnerText.Should().Be("2026-08-28T13:21:05Z");
    }

    [Fact]
    public void DateIssued_in_utc_keeps_its_instant()
    {
        //Arrange
        var utcDateIssued = new DateTime(2026, 8, 28, 13, 21, 5, DateTimeKind.Utc);
        var metsData = new Builder()
            .SetModsSection(new MetsData.ModsSectionData { DateIssued = utcDateIssued, Url = new Uri("http://ccc.ddd") })
            .AddMetsAttributes([new MetsData.MetsAttribute { Name = MetsData.EMetsAttributeName.ObjId, Value = string.Empty }])
            .Build();
        var sut = new Renderer(metsData);

        //Act
        var result = sut.Render(DateTime.MinValue);

        //Assert
        var nsmgr = new XmlNamespaceManager(result.NameTable);
        nsmgr.AddNamespace("mods", ModsNs);
        result.SelectSingleNode("//mods:dateIssued", nsmgr).InnerText.Should().Be("2026-08-28T13:21:05Z");
    }
}
