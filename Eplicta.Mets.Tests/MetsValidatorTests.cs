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
    public void Basic(ModsVersion version)
    {
        //Arrange
        var modsData = new Fixture().Build<MetsData>()
            .Without(x => x.MetsHdr)
            .Without(x => x.Files)
            .Without(x => x.Sources)
            .Without(x => x.AltRecords)
            .Create();
        var document = new Renderer(modsData).Render(null, MetsSchema.Riksarkivet);
        var sut = new MetsValidator();

        //Act
        var result = sut.Validate(document, version, MetsSchema.Riksarkivet);

        //Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(MetsSchemaGenerator))]
    public void Validate_Schemas(MetsSchema schema)
    {
        //Arrange
        var modsData = new Fixture().Build<MetsData>()
            .Without(x => x.MetsHdr)
            .Without(x => x.Files)
            .Without(x => x.Sources)
            .Without(x => x.AltRecords)
            .Create();
        var document = new Renderer(modsData).Render(null, schema);
        var sut = new MetsValidator();

        //Act
        var result = sut.Validate(document, ModsVersion.ModsFgsPubl_1_0, schema);

        //Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(MetsVersionGenerator))]
    public void Minimal(ModsVersion version)
    {
        //Arrange
        var modsData = new Builder()
            .AddAltRecord(new MetsData.AltRecord())
            .AddAltRecord(new MetsData.AltRecord())
            .AddAltRecord(new MetsData.AltRecord())
            .AddMetsAttributes([new MetsData.MetsAttribute { Name = MetsData.EMetsAttributeName.ObjId, Value = string.Empty }])
            .AddFile(new DataFileSource { Data = [] })
            .Build();
        var document = new Renderer(modsData).Render();
        var sut = new MetsValidator();

        //Act
        var result = sut.Validate(document, version, MetsSchema.Default);

        //Assert
        result.Should().BeEmpty();
    }

    class MetsVersionGenerator : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return [ModsVersion.ModsFgsPubl_1_0];
            yield return [ModsVersion.Mods_3_0];
            yield return [ModsVersion.Mods_3_1];
            yield return [ModsVersion.Mods_3_2];
            yield return [ModsVersion.Mods_3_3];
            yield return [ModsVersion.Mods_3_4];
            yield return [ModsVersion.Mods_3_5];
            yield return [ModsVersion.Mods_3_6];
            yield return [ModsVersion.Mods_3_7];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    class MetsSchemaGenerator : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return [MetsSchema.Default];
            yield return [MetsSchema.Riksarkivet];
            yield return [MetsSchema.KB];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}