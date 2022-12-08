using AutoFixture;
using Eplicta.Mets.Entities;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests;

public class ArchiveTests
{
    [Fact]
    public void Basic()
    {
        //Arrange
        var modsData = new Fixture().Build<ModsData>().Create();
        var sut = new Renderer(modsData, Version.ModsFgsPubl_1_0);

        //Act
        var result = sut.GetArchiveStream();

        //Assert
        result.ToArray().Should().NotBeNull();
    }
}