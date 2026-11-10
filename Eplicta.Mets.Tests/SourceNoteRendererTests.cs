using System;
using System.Linq;
using System.Xml;
using Eplicta.Mets.Entities;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests;

public class SourceNoteRendererTests
{
    private const string ModsNs = "http://www.loc.gov/mods/v3";

    [Fact]
    public void Render_WhenNoteIsSourcePlatform_ShouldEmitNoteWithTypeSourcePlatform()
    {
        var notes = RenderNotes(Note(MetsData.ENoteType.SourcePlatform, "Facebook"));

        var note = notes.Single();
        note.GetAttribute("type").Should().Be("source platform");
        note.InnerText.Should().Be("Facebook");
    }

    [Fact]
    public void Render_WhenNoteIsSourceName_ShouldEmitNoteWithTypeSourceName()
    {
        var notes = RenderNotes(Note(MetsData.ENoteType.SourceName, "UH-Uppsalahem-Facebook"));

        var note = notes.Single();
        note.GetAttribute("type").Should().Be("source name");
        note.InnerText.Should().Be("UH-Uppsalahem-Facebook");
    }

    [Fact]
    public void Render_WhenSeveralNotes_ShouldKeepGivenOrder()
    {
        var notes = RenderNotes(
            Note(MetsData.ENoteType.SourcePlatform, "Facebook"),
            Note(MetsData.ENoteType.SourceChannel, "Uppsalahem"),
            Note(MetsData.ENoteType.SourceName, "UH-Uppsalahem-Facebook"),
            Note(MetsData.ENoteType.CollectDate, "2026-01-01"));

        notes.Select(x => x.GetAttribute("type")).Should().Equal("source platform", "source channel", "source name", "collect date");
        notes.Select(x => x.InnerText).Should().Equal("Facebook", "Uppsalahem", "UH-Uppsalahem-Facebook", "2026-01-01");
    }

    [Fact]
    public void ENoteType_WhenExistingMembers_ShouldKeepNumericValues()
    {
        ((int)MetsData.ENoteType.None).Should().Be(0);
        ((int)MetsData.ENoteType.SourceChannel).Should().Be(11);
        ((int)MetsData.ENoteType.CollectDate).Should().Be(14);
    }

    [Fact]
    public void ENoteType_WhenSourceNotesAdded_ShouldComeAfterCollectDate()
    {
        var collectDate = (int)MetsData.ENoteType.CollectDate;

        ((int)MetsData.ENoteType.SourcePlatform).Should().BeGreaterThan(collectDate);
        ((int)MetsData.ENoteType.SourceName).Should().BeGreaterThan(collectDate);
        ((int)MetsData.ENoteType.SourcePlatform).Should().NotBe((int)MetsData.ENoteType.SourceName);
    }

    private static MetsData.ModsNote Note(MetsData.ENoteType type, string innerText)
    {
        return new MetsData.ModsNote { Type = type, InnerText = innerText };
    }

    private static XmlElement[] RenderNotes(params MetsData.ModsNote[] notes)
    {
        var builder = new Builder()
            .SetModsSection(new MetsData.ModsSectionData { Url = new Uri("http://ccc.ddd") })
            .AddMetsAttributes([new MetsData.MetsAttribute { Name = MetsData.EMetsAttributeName.ObjId, Value = string.Empty }]);
        foreach (var note in notes)
        {
            builder.AddModsNote(note);
        }

        var document = new Renderer(builder.Build()).Render(DateTime.MinValue);

        var nsmgr = new XmlNamespaceManager(document.NameTable);
        nsmgr.AddNamespace("mods", ModsNs);
        return document.SelectNodes("//mods:note", nsmgr)!.Cast<XmlElement>().ToArray();
    }
}
