﻿using AutoFixture;
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
        var modsData = new Fixture().Build<MetsData>().Without(x => x.Sources).Create();
        var sut = new Renderer(modsData);

        //Act
        using var result = sut.GetArchiveStream(ArchiveFormat.Zip);

        //Assert
        result.ToArray().Should().NotBeNull();
    }
}