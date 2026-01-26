using System;
using Eplicta.Mets.Entities;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests;

public class ParseTests
{
    [Fact]
    public void ChangeAgentOnArchive()
    {
        //Arrange
        var originalAgentData = new MetsData.AgentData
        {
            Note = "a1",
            Type = MetsData.EType.Other,
            Name = "a3",
            Role = MetsData.ERole.Editor
        };
        var metsData = new Builder()
            .AddAgent(originalAgentData)
            .AddMetsAttributes([new MetsData.MetsAttribute { Name = MetsData.EMetsAttributeName.ObjId, Value = string.Empty }])
            .Build();
        var renderer = new Renderer(metsData);
        var createTime = DateTime.UtcNow;
        var initialPackage = renderer.Render(createTime);
        var sut = new Parser();

        //Act
        var unpackedMetsData = sut.GetMetsData(initialPackage.OuterXml);

        //Assert
        unpackedMetsData.CreateTime.Should().Be(createTime.ToLocalTime());
    }

    [Fact]
    public void RenderParseRender()
    {
        //Arrange
        var metsData1 = new MetsData();
        var renderer1 = new Renderer(metsData1);
        var sut = new Parser();
        var result = renderer1.Render(DateTime.UtcNow.ToLocalTime());

        //Act
        var metaData2 = sut.GetMetsData(result.OuterXml);

        //Assert
        var renderer2 = new Renderer(metaData2.MetsData);
        var result2 = renderer2.Render(metaData2.CreateTime);
        result.Should().BeEquivalentTo(result2);
    }
}