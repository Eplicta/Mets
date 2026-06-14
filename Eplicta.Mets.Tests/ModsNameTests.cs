using System;
using System.Xml;
using Eplicta.Mets.Entities;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests;

public class ModsNameTests
{
    private const string ModsNs = "http://www.loc.gov/mods/v3";
    private const string DisplayName = "Uppsala Stadsarkiv";

    private static MetsData BuildMetsDataWithCreator()
    {
        return new Builder()
            .SetModsSection(new MetsData.ModsSectionData
            {
                Identifier = "test-identifier",
                Creator = new MetsData.ModsName
                {
                    DisplayName = DisplayName,
                    Type = MetsData.ENameType.Corporate
                }
            })
            .AddAltRecord(new MetsData.AltRecord())
            .AddAltRecord(new MetsData.AltRecord())
            .AddAltRecord(new MetsData.AltRecord())
            .AddMetsAttributes([new MetsData.MetsAttribute { Name = MetsData.EMetsAttributeName.ObjId, Value = string.Empty }])
            .AddFile(new DataFileSource { Data = [] })
            .Build();
    }

    private static XmlDocument RenderWithCreator()
    {
        return new Renderer(BuildMetsDataWithCreator()).Render(DateTime.MinValue);
    }

    [Fact]
    public void Render_WhenCreatorIsNull_ShouldNotEmitModsNameElement()
    {
        //Arrange
        var metsData = new Builder()
            .SetModsSection(new MetsData.ModsSectionData
            {
                Identifier = "test-identifier",
                Creator = null
            })
            .AddAltRecord(new MetsData.AltRecord())
            .AddAltRecord(new MetsData.AltRecord())
            .AddAltRecord(new MetsData.AltRecord())
            .AddMetsAttributes([new MetsData.MetsAttribute { Name = MetsData.EMetsAttributeName.ObjId, Value = string.Empty }])
            .AddFile(new DataFileSource { Data = [] })
            .Build();
        var document = new Renderer(metsData).Render(DateTime.MinValue);

        //Act
        var nsmgr = new XmlNamespaceManager(document.NameTable);
        nsmgr.AddNamespace("mods", ModsNs);
        var nameNode = document.SelectSingleNode("//mods:mods/mods:name", nsmgr);

        //Assert
        nameNode.Should().BeNull("mods:name must not be emitted when Creator is null");
    }

    [Fact]
    public void Render_WhenCreatorIsSet_ShouldEmitModsDisplayFormWithCreatorDisplayName()
    {
        //Arrange
        var document = RenderWithCreator();

        //Act
        var nsmgr = new XmlNamespaceManager(document.NameTable);
        nsmgr.AddNamespace("mods", ModsNs);
        var displayFormNode = document.SelectSingleNode("//mods:mods/mods:name/mods:displayForm", nsmgr);

        //Assert
        displayFormNode.Should().NotBeNull("mods:displayForm must be emitted inside mods:name");
        displayFormNode.InnerText.Should().Be(DisplayName);
    }

    [Fact]
    public void Render_WhenCreatorIsSet_ShouldEmitModsNamePartWithCreatorDisplayName()
    {
        //Arrange
        var document = RenderWithCreator();

        //Act
        var nsmgr = new XmlNamespaceManager(document.NameTable);
        nsmgr.AddNamespace("mods", ModsNs);
        var namePartNode = document.SelectSingleNode("//mods:mods/mods:name/mods:namePart", nsmgr);

        //Assert
        namePartNode.Should().NotBeNull("mods:namePart must be emitted inside mods:name");
        namePartNode.InnerText.Should().Be(DisplayName);
    }

    [Fact]
    public void Render_WhenCreatorIsSet_ShouldEmitModsNameWithCorrectTypeAttribute()
    {
        //Arrange
        var document = RenderWithCreator();

        //Act
        var nsmgr = new XmlNamespaceManager(document.NameTable);
        nsmgr.AddNamespace("mods", ModsNs);
        var nameNode = document.SelectSingleNode("//mods:mods/mods:name", nsmgr);

        //Assert
        nameNode.Should().NotBeNull("mods:name must be emitted when Creator is set");
        var typeAttr = ((XmlElement)nameNode).GetAttribute("type");
        typeAttr.Should().Be("corporate", "type attribute must be the lowercased ENameType value");
    }

    [Fact]
    public void Render_WhenCreatorIsSet_ShouldEmitModsRoleTermWithCreatorText()
    {
        //Arrange
        var document = RenderWithCreator();

        //Act
        var nsmgr = new XmlNamespaceManager(document.NameTable);
        nsmgr.AddNamespace("mods", ModsNs);
        var roleTermNode = document.SelectSingleNode("//mods:mods/mods:name/mods:role/mods:roleTerm", nsmgr);

        //Assert
        roleTermNode.Should().NotBeNull("mods:roleTerm must be emitted inside mods:role");
        roleTermNode.InnerText.Should().Be("creator");
        var el = (XmlElement)roleTermNode;
        el.GetAttribute("type").Should().Be("text");
        el.GetAttribute("authority").Should().Be("marcrelator");
    }

    [Fact]
    public void Render_WhenCreatorIsSet_ShouldStillValidateAgainstFgsPublSchema()
    {
        //Arrange
        var metsData = new Builder()
            .SetModsSection(new MetsData.ModsSectionData
            {
                Identifier = "test-identifier",
                Url = new Uri("http://example.com/doc"),
                ModsTitle = "Test document",
                Creator = new MetsData.ModsName
                {
                    DisplayName = DisplayName,
                    Type = MetsData.ENameType.Corporate
                }
            })
            .AddAltRecord(new MetsData.AltRecord())
            .AddAltRecord(new MetsData.AltRecord())
            .AddAltRecord(new MetsData.AltRecord())
            .AddMetsAttributes([new MetsData.MetsAttribute { Name = MetsData.EMetsAttributeName.ObjId, Value = string.Empty }])
            .AddFile(new DataFileSource { Data = [] })
            .Build();
        var document = new Renderer(metsData).Render(DateTime.MinValue);
        var sut = new MetsValidator();

        //Act
        var result = sut.Validate(document, ModsVersion.ModsFgsPubl_1_0, MetsSchema.Default);

        //Assert
        result.Should().BeEmpty("a mods:name element must not break FGS-PUBL schema validation");
    }

    [Fact]
    public void Deserialize_WhenDocumentContainsModsName_ShouldPopulateNamesArray()
    {
        //Arrange
        var document = RenderWithCreator();
        var sut = new Serializer();

        //Act
        var result = sut.Deserialize(document);

        //Assert
        var names = result.DmdSec.MdWrap.XmlData.Mods.Names;
        names.Should().NotBeNullOrEmpty("Names must be populated when mods:name is present in the document");
        names.Should().HaveCount(1);
    }

    [Fact]
    public void Deserialize_WhenDocumentContainsModsName_ShouldParseDisplayForm()
    {
        //Arrange
        var document = RenderWithCreator();
        var sut = new Serializer();

        //Act
        var result = sut.Deserialize(document);

        //Assert
        var name = result.DmdSec.MdWrap.XmlData.Mods.Names[0];
        name.DisplayForm.Should().Be(DisplayName);
    }

    [Fact]
    public void Deserialize_WhenDocumentContainsModsName_ShouldParseNamePart()
    {
        //Arrange
        var document = RenderWithCreator();
        var sut = new Serializer();

        //Act
        var result = sut.Deserialize(document);

        //Assert
        var name = result.DmdSec.MdWrap.XmlData.Mods.Names[0];
        name.NamePart.Should().Be(DisplayName);
    }

    [Fact]
    public void Deserialize_WhenDocumentContainsModsName_ShouldParseTypeAttribute()
    {
        //Arrange
        var document = RenderWithCreator();
        var sut = new Serializer();

        //Act
        var result = sut.Deserialize(document);

        //Assert
        var name = result.DmdSec.MdWrap.XmlData.Mods.Names[0];
        name.Type.Should().Be("corporate");
    }

    [Fact]
    public void Deserialize_WhenDocumentContainsModsName_ShouldParseRoleTerm()
    {
        //Arrange
        var document = RenderWithCreator();
        var sut = new Serializer();

        //Act
        var result = sut.Deserialize(document);

        //Assert
        var name = result.DmdSec.MdWrap.XmlData.Mods.Names[0];
        // Assumption: ModsNameElement exposes the roleTerm inner text via a RoleTerm property
        // (or a nested Role/RoleTerm element structure). The implementation must expose
        // the "creator" string so that this assertion passes.
        name.RoleTerm.Should().Be("creator");
    }
}
