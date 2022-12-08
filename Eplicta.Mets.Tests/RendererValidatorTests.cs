using System;
using System.Collections;
using System.Collections.Generic;
using AutoFixture;
using Eplicta.Mets.Entities;
using FluentAssertions;
using Xunit;
using Version = Eplicta.Mets.Entities.Version;

namespace Eplicta.Mets.Tests;

public class RendererValidatorTests
{
    [Theory]
    [ClassData(typeof(SchemaGenerator))]
    public void Basic(Version version)
    {
        //Arrange
        var modsData = new Fixture().Build<ModsData>().Create();
        var document = new Renderer(modsData, version).Render();
        var schema = Mets.Helpers.Resource.GetXml($"{version.Key}.xsd");
        var sut = new XmlValidator();

        //Act
        var result = sut.Validate(document, schema);

        //Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(SchemaGenerator))]
    public void Minimal(Version version)
    {
        //Arrange
        var modsData = new Builder()
            .AddAltRecord(new ModsData.AltRecord())
            .AddAltRecord(new ModsData.AltRecord())
            .AddAltRecord(new ModsData.AltRecord())
            .AddFile(new FileSource { Data = Array.Empty<byte>() })
            .Build();
        var document = new Renderer(modsData, version).Render();
        var schema = Mets.Helpers.Resource.GetXml($"{version.Key}.xsd");
        var sut = new XmlValidator();

        //Act
        var result = sut.Validate(document, schema);

        //Assert
        result.Should().BeEmpty();
    }

    class SchemaGenerator : IEnumerable<object[]>
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