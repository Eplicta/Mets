using System.Linq;
using Eplicta.Mets.Features.EArk;
using Xunit;

namespace Eplicta.Mets.Tests.Features.EArk;

public class EArkReaderModsGenreTests
{
    private static string Mods(string genres)
    {
        return $"""
                <?xml version="1.0" encoding="UTF-8"?>
                <mods:mods xmlns:mods="http://www.loc.gov/mods/v3">
                  <mods:identifier type="local">id-1</mods:identifier>
                  {genres}
                </mods:mods>
                """;
    }

    [Fact]
    public void ReadMods_WhenOneGenre_ShouldReadValueAndAuthority()
    {
        var actual = new EArkReader().ReadMods(Mods("<mods:genre authority=\"eplicta\">FortDocs Case Archive</mods:genre>"));

        var genre = Assert.Single(actual.Genres);
        Assert.Equal("FortDocs Case Archive", genre.Value);
        Assert.Equal("eplicta", genre.Authority);
    }

    [Fact]
    public void ReadMods_WhenTwoGenres_ShouldReadBothInOrder()
    {
        var actual = new EArkReader().ReadMods(Mods("<mods:genre authority=\"a\">First</mods:genre><mods:genre>Second</mods:genre>"));

        Assert.Equal(2, actual.Genres.Length);
        Assert.Equal("First", actual.Genres[0].Value);
        Assert.Equal("a", actual.Genres[0].Authority);
        Assert.Equal("Second", actual.Genres[1].Value);
        Assert.Null(actual.Genres[1].Authority);
    }

    [Fact]
    public void ReadMods_WhenNoGenre_ShouldReadGenresAsNullOrEmpty()
    {
        var actual = new EArkReader().ReadMods(Mods(string.Empty));

        Assert.True(actual.Genres == null || actual.Genres.Length == 0);
    }

    [Fact]
    public void ReadMods_WhenDemoModsHasNoGenre_ShouldStillReadAndHaveNoGenres()
    {
        var actual = new EArkReader().ReadMods(TestResource.EplictaDemoMods());

        Assert.Equal("local", actual.Identifier.Type);
        Assert.True(actual.Genres == null || !actual.Genres.Any());
    }
}
