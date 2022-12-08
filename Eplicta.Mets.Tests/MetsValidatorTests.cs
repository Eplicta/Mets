using System.Collections;
using System.Collections.Generic;
using AutoFixture;
using Eplicta.Mets.Entities;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests;

public class MetsValidatorTests
{
    [Theory]
    [ClassData(typeof(MetsVersionGenerator))]
    public void Basic(Version version)
    {
        //Arrange
        var modsData = new Fixture().Build<ModsData>().Create();
        var document = new Renderer(modsData, version).Render();
        var sut = new MetsValidator();

        //Act
        var result = sut.Validate(document, version);

        //Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(MetsVersionGenerator))]
    public void Minimal(Version version)
    {
        //Arrange
        var modsData = new Builder()
            .AddAltRecord(new ModsData.AltRecord())
            .AddAltRecord(new ModsData.AltRecord())
            .AddAltRecord(new ModsData.AltRecord())
            .AddFile(new FileSource { Data = new byte[] { } })
            .Build();
        var document = new Renderer(modsData, version).Render();
        var sut = new MetsValidator();

        //Act
        var result = sut.Validate(document, version);

        //Assert
        result.Should().BeEmpty();
    }

    private class MetsVersionGenerator : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            foreach (var version in Version.All())
            {
                yield return new object[] { version };
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}