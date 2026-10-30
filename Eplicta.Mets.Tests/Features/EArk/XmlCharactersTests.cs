using System.Xml;
using Eplicta.Mets.Features.EArk;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests.Features.EArk;

public class XmlCharactersTests
{
    [Theory]
    [InlineData("Kränkning\u000Bi 7B", "Kränkning i 7B")]
    [InlineData("sida 1\u000Csida 2", "sida 1 sida 2")]
    [InlineData("a\u0000b\u001Fc", "a b c")]
    public void A_control_character_xml_cannot_hold_becomes_a_space(string value, string expected)
    {
        XmlCharacters.Clean(value).Should().Be(expected);
    }

    [Fact]
    public void An_unpaired_surrogate_or_a_non_character_is_removed()
    {
        XmlCharacters.Clean($"a{(char)0xD800}b").Should().Be("ab");
        XmlCharacters.Clean($"a{(char)0xDC00}b").Should().Be("ab");
        XmlCharacters.Clean($"a{(char)0xFFFE}b{(char)0xFFFF}").Should().Be("ab");
    }

    [Theory]
    [InlineData("Rad ett\nrad två\r\n\tindrag")]
    [InlineData("Tumme upp 👍")]
    [InlineData("Åsa Öberg")]
    public void Text_xml_can_hold_is_returned_as_the_same_instance(string value)
    {
        XmlCharacters.Clean(value).Should().BeSameAs(value);
    }

    [Fact]
    public void Every_attribute_and_text_of_a_document_is_cleaned_so_it_serialises_as_well_formed_xml()
    {
        var document = new XmlDocument();
        var root = document.CreateElement("case");
        root.SetAttribute("label", "2026-0042\u000BKränkning");
        root.AppendChild(document.CreateElement("title")).InnerText = "Kränkning\u0001i 7B";
        document.AppendChild(root);

        XmlCharacters.Clean(document);

        var reloaded = new XmlDocument();
        reloaded.LoadXml(document.OuterXml);
        reloaded.DocumentElement!.GetAttribute("label").Should().Be("2026-0042 Kränkning");
        reloaded.DocumentElement["title"]!.InnerText.Should().Be("Kränkning i 7B");
    }
}
