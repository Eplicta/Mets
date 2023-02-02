using System.Collections;
using System.Collections.Generic;
using AutoFixture;
using Eplicta.Mets.Entities;
using FluentAssertions;
using Xunit;
using Version = Eplicta.Mets.Entities.Version;

namespace Eplicta.Mets.Tests;

public class MetsValidatorTests
{
    [Theory]
    [ClassData(typeof(MetsVersionGenerator))]
    public void Basic(Version version)
    {
        //Arrange
        var modsData = new Fixture().Build<ModsData>().Without(x => x.MetsHdr).Create();
        var document = new Renderer(modsData).Render();
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
        var document = new Renderer(modsData).Render();
        var sut = new MetsValidator();

        //Act
        var result = sut.Validate(document, version);

        //Assert
        result.Should().BeEmpty();
    }

    class MetsVersionGenerator : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { Version.ModsFgsPubl_1_0 };
            //yield return new object[] { Version.ModsFgsPubl_1_1 };
            yield return new object[] { Version.Mods_3_0 };
            yield return new object[] { Version.Mods_3_1 };
            yield return new object[] { Version.Mods_3_2 };
            yield return new object[] { Version.Mods_3_3 };
            yield return new object[] { Version.Mods_3_4 };
            yield return new object[] { Version.Mods_3_5 };
            yield return new object[] { Version.Mods_3_6 };
            yield return new object[] { Version.Mods_3_7 };

            //foreach (var version in Version.All())
            //{
            //    yield return new object[] { version };
            //}
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}