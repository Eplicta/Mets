using System;
using System.Text;
using System.Xml;
using Eplicta.Mets.Features.EArk;
using Eplicta.Mets.Features.Mods;
using Xunit;

namespace Eplicta.Mets.Tests.Features.Mods;

public class ModsDocumentRendererTests
{
    private const string ModsNamespace = "http://www.loc.gov/mods/v3";
    private static readonly DateTime Issued = new(2026, 3, 14, 9, 30, 0, DateTimeKind.Utc);

    private static ModsDocument Full()
    {
        return new ModsDocument
        {
            Identifier = "case-2026-0042",
            IdentifierType = "uuid",
            Title = "2026-0042 Kränkande behandling",
            Url = new Uri("https://fortdocs.example/cases/2026-0042"),
            DateIssued = Issued,
            Genres = [EplictaPackageTypes.CaseArchive, "Second genre"],
            Notes = [new ModsDocumentNote { Type = "agency", Value = "Futuraskolan AB" }, new ModsDocumentNote { Type = "case", Value = "Ärende & <info>" }]
        };
    }

    private static ModsDocument Minimal()
    {
        return new ModsDocument { Identifier = "id-1" };
    }

    private static XmlNamespaceManager Namespaces(XmlDocument document)
    {
        var manager = new XmlNamespaceManager(document.NameTable);
        manager.AddNamespace("m", ModsNamespace);
        return manager;
    }

    private static string Text(XmlDocument document, string xpath)
    {
        return document.SelectSingleNode(xpath, Namespaces(document))?.InnerText;
    }

    private static int Count(XmlDocument document, string xpath)
    {
        return document.SelectNodes(xpath, Namespaces(document))!.Count;
    }

    [Fact]
    public void Render_ShouldHaveModsRootInTheModsNamespace()
    {
        var actual = ModsDocumentRenderer.Render(Minimal());

        Assert.Equal("mods", actual.DocumentElement!.LocalName);
        Assert.Equal("mods", actual.DocumentElement.Prefix);
        Assert.Equal(ModsNamespace, actual.DocumentElement.NamespaceURI);
    }

    [Fact]
    public void Render_WhenDefaults_ShouldUseLocalIdentifierType()
    {
        var actual = ModsDocumentRenderer.Render(Minimal());

        Assert.Equal("id-1", Text(actual, "/m:mods/m:identifier"));
        Assert.Equal("local", actual.SelectSingleNode("/m:mods/m:identifier/@type", Namespaces(actual))!.Value);
    }

    [Fact]
    public void Render_WhenOnlyIdentifier_ShouldOmitAllOptionalElements()
    {
        var actual = ModsDocumentRenderer.Render(Minimal());

        Assert.Equal(0, Count(actual, "/m:mods/m:titleInfo"));
        Assert.Equal(0, Count(actual, "/m:mods/m:location"));
        Assert.Equal(0, Count(actual, "/m:mods/m:originInfo"));
        Assert.Equal(0, Count(actual, "/m:mods/m:genre"));
        Assert.Equal(0, Count(actual, "/m:mods/m:note"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Render_WhenTitleIsNullOrEmpty_ShouldOmitTitleInfo(string title)
    {
        var actual = ModsDocumentRenderer.Render(Minimal() with { Title = title });

        Assert.Equal(0, Count(actual, "/m:mods/m:titleInfo"));
    }

    [Fact]
    public void Render_WhenGenreIsEmpty_ShouldSkipThatGenre()
    {
        var actual = ModsDocumentRenderer.Render(Minimal() with { Genres = ["A", "", null] });

        Assert.Equal(1, Count(actual, "/m:mods/m:genre"));
    }

    [Fact]
    public void Render_WhenNoteValueIsEmpty_ShouldSkipThatNote()
    {
        var actual = ModsDocumentRenderer.Render(Minimal() with { Notes = [new ModsDocumentNote { Type = "t", Value = "" }, new ModsDocumentNote { Type = "t2", Value = "v" }] });

        Assert.Equal(1, Count(actual, "/m:mods/m:note"));
    }

    [Fact]
    public void Render_WhenFull_ShouldRenderAllElements()
    {
        var actual = ModsDocumentRenderer.Render(Full());

        Assert.Equal("case-2026-0042", Text(actual, "/m:mods/m:identifier"));
        Assert.Equal("2026-0042 Kränkande behandling", Text(actual, "/m:mods/m:titleInfo/m:title"));
        Assert.Equal("https://fortdocs.example/cases/2026-0042", Text(actual, "/m:mods/m:location/m:url"));
        Assert.Equal("2026-03-14T09:30:00Z", Text(actual, "/m:mods/m:originInfo/m:dateIssued"));
        Assert.Equal("w3cdtf", actual.SelectSingleNode("/m:mods/m:originInfo/m:dateIssued/@encoding", Namespaces(actual))!.Value);
        Assert.Equal(2, Count(actual, "/m:mods/m:genre"));
        Assert.Equal(2, Count(actual, "/m:mods/m:note"));
    }

    [Fact]
    public void Render_WhenUnspecifiedKind_ShouldTreatDateAsUtc()
    {
        var actual = ModsDocumentRenderer.Render(Minimal() with { DateIssued = new DateTime(2026, 3, 14, 9, 30, 0, DateTimeKind.Unspecified) });

        Assert.Equal("2026-03-14T09:30:00Z", Text(actual, "/m:mods/m:originInfo/m:dateIssued"));
    }

    [Fact]
    public void Render_WhenLocalKind_ShouldConvertToUtc()
    {
        var local = new DateTime(2026, 3, 14, 9, 30, 0, DateTimeKind.Local);

        var actual = ModsDocumentRenderer.Render(Minimal() with { DateIssued = local });

        Assert.Equal(local.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"), Text(actual, "/m:mods/m:originInfo/m:dateIssued"));
    }

    [Fact]
    public void Render_WhenNullDocument_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ModsDocumentRenderer.Render(null));
        Assert.Throws<ArgumentNullException>(() => ModsDocumentRenderer.RenderBytes(null));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Render_WhenIdentifierIsEmpty_ShouldThrowArgumentException(string identifier)
    {
        Assert.ThrowsAny<ArgumentException>(() => ModsDocumentRenderer.Render(new ModsDocument { Identifier = identifier }));
        Assert.ThrowsAny<ArgumentException>(() => ModsDocumentRenderer.RenderBytes(new ModsDocument { Identifier = identifier }));
    }

    [Fact]
    public void RenderBytes_ShouldBeUtf8WithoutBomAndHaveXmlDeclaration()
    {
        var bytes = ModsDocumentRenderer.RenderBytes(Full());

        Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);
        var text = Encoding.UTF8.GetString(bytes);
        Assert.StartsWith("<?xml", text);
        Assert.Contains("encoding=\"utf-8\"", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RenderBytes_ShouldBeIndented()
    {
        var text = Encoding.UTF8.GetString(ModsDocumentRenderer.RenderBytes(Full()));

        Assert.Contains("\n", text);
        Assert.Matches(@"\n\s+<mods:", text);
    }

    [Fact]
    public void RenderBytes_ShouldKeepNonAsciiCharacters()
    {
        var text = Encoding.UTF8.GetString(ModsDocumentRenderer.RenderBytes(Full()));

        Assert.Contains("Kränkande", text);
    }

    [Fact]
    public void RenderBytes_ShouldRoundTripThroughTheReader()
    {
        var bytes = ModsDocumentRenderer.RenderBytes(Full());

        var actual = new EArkReader().ReadMods(Encoding.UTF8.GetString(bytes));

        Assert.Equal("case-2026-0042", actual.Identifier.Value);
        Assert.Equal("uuid", actual.Identifier.Type);
        Assert.Equal("2026-0042 Kränkande behandling", actual.TitleInfo.Title.Value);
        Assert.Equal("https://fortdocs.example/cases/2026-0042", actual.Location.Url.Value);
        Assert.Equal("w3cdtf", actual.OriginInfo.DateIssued.Encoding);
        Assert.Equal(Issued, actual.OriginInfo.DateIssued.Value.ToUniversalTime());
        Assert.Equal([EplictaPackageTypes.CaseArchive, "Second genre"], Array.ConvertAll(actual.Genres, x => x.Value));
        Assert.Equal(2, actual.Notes.Length);
        Assert.Equal("agency", actual.Notes[0].Type);
        Assert.Equal("Futuraskolan AB", actual.Notes[0].Value);
        Assert.Equal("case", actual.Notes[1].Type);
        Assert.Equal("Ärende & <info>", actual.Notes[1].Value);
    }

    [Fact]
    public void RenderBytes_WhenMinimal_ShouldRoundTripWithoutOptionalValues()
    {
        var actual = new EArkReader().ReadMods(Encoding.UTF8.GetString(ModsDocumentRenderer.RenderBytes(Minimal())));

        Assert.Equal("id-1", actual.Identifier.Value);
        Assert.Equal("local", actual.Identifier.Type);
        Assert.Null(actual.TitleInfo);
        Assert.Null(actual.Location);
        Assert.Null(actual.OriginInfo);
        Assert.True(actual.Genres == null || actual.Genres.Length == 0);
        Assert.True(actual.Notes == null || actual.Notes.Length == 0);
    }

    [Fact]
    public void ModsDocument_Defaults_ShouldBeLocalAndEmptyLists()
    {
        var document = new ModsDocument { Identifier = "x" };

        Assert.Equal("local", document.IdentifierType);
        Assert.Empty(document.Genres);
        Assert.Empty(document.Notes);
        Assert.Null(document.DateIssued);
    }
}
