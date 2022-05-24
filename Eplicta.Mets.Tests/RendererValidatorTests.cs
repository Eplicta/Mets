using System.Collections;
using System.Collections.Generic;
using AutoFixture;
using Eplicta.Mets.Entities;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests;

public class RendererValidatorTests
{
    [Theory]
    [ClassData(typeof(SchemaGenerator))]
    public void Basic(string format)
    {
        //Arrange
        var modsData = new Fixture().Build<ModsData>().Create();
        var document = new Renderer(modsData).Render();
        var schema = Mets.Helpers.Resource.GetXml(format);
        var sut = new XmlValidator();

        //Act
        var result = sut.Validate(document, schema);

        //Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(SchemaGenerator))]
    public void Minimal(string format)
    {
        //Arrange
        var modsData = new Builder()
            .AddAltRecord(new ModsData.AltRecord())
            .AddAltRecord(new ModsData.AltRecord())
            .AddAltRecord(new ModsData.AltRecord())
            .AddFile(System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName)
            .Build();
        var document = new Renderer(modsData).Render();
        var schema = Mets.Helpers.Resource.GetXml(format);
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
            yield return new object[] { "MODS_enligt_FGS-PUBL_xml1_0.xsd" };
            //yield return new object[] { "MODS_enligt_FGS-PUBL_xml1_1.xsd" };
            yield return new object[] { "mods-3-0.xsd" };
            yield return new object[] { "mods-3-1.xsd" };
            yield return new object[] { "mods-3-2.xsd" };
            yield return new object[] { "mods-3-3.xsd" };
            yield return new object[] { "mods-3-4.xsd" };
            yield return new object[] { "mods-3-5.xsd" };
            yield return new object[] { "mods-3-6.xsd" };
            yield return new object[] { "mods-3-7.xsd" };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}