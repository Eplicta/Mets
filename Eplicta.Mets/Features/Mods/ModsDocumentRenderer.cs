using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Eplicta.Mets.Features.EArk;

namespace Eplicta.Mets.Features.Mods;

/// <summary>
/// Renders a <see cref="ModsDocument"/> as a standalone MODS document, the file an E-ARK package references from its
/// dmdSec rather than embedding. <see cref="EArkReader.ReadMods"/> reads it back.
/// </summary>
public static class ModsDocumentRenderer
{
    private const string ModsNamespace = "http://www.loc.gov/mods/v3";
    private const string ModsPrefix = "mods";
    private const string DateEncoding = "w3cdtf";

    public static XmlDocument Render(ModsDocument mods)
    {
        if (mods == null) throw new ArgumentNullException(nameof(mods));
        if (string.IsNullOrWhiteSpace(mods.Identifier)) throw new ArgumentException("A MODS document needs its identifier.", nameof(mods));

        var document = new XmlDocument();
        document.AppendChild(document.CreateXmlDeclaration("1.0", "UTF-8", null));

        var root = document.CreateElement(ModsPrefix, "mods", ModsNamespace);
        document.AppendChild(root);

        var identifier = Append(root, "identifier", mods.Identifier);
        if (!string.IsNullOrEmpty(mods.IdentifierType)) identifier.SetAttribute("type", mods.IdentifierType);

        if (!string.IsNullOrEmpty(mods.Title)) Append(Append(root, "titleInfo"), "title", mods.Title);
        if (mods.Url != null) Append(Append(root, "location"), "url", mods.Url.AbsoluteUri);
        if (mods.DateIssued != null) Append(Append(root, "originInfo"), "dateIssued", EArkRenderer.ToUtc(mods.DateIssued.Value)).SetAttribute("encoding", DateEncoding);

        foreach (var genre in (mods.Genres ?? []).Where(x => !string.IsNullOrEmpty(x)))
        {
            Append(root, "genre", genre);
        }

        foreach (var note in (mods.Notes ?? []).Where(x => !string.IsNullOrEmpty(x?.Value)))
        {
            var element = Append(root, "note", note.Value);
            if (!string.IsNullOrEmpty(note.Type)) element.SetAttribute("type", note.Type);
        }

        return XmlCharacters.Clean(document);
    }

    /// <summary>The rendered document as indented UTF-8 without a byte order mark, ready to be the content of an <see cref="EArkMetadataReference"/>.</summary>
    public static byte[] RenderBytes(ModsDocument mods)
    {
        var document = Render(mods);

        using var buffer = new MemoryStream();
        using (var writer = new XmlTextWriter(buffer, new UTF8Encoding(false)) { Formatting = Formatting.Indented })
        {
            document.WriteTo(writer);
        }

        return buffer.ToArray();
    }

    private static XmlElement Append(XmlElement parent, string name, string value = null)
    {
        var element = parent.OwnerDocument!.CreateElement(ModsPrefix, name, ModsNamespace);
        if (value != null) element.InnerText = value;
        parent.AppendChild(element);
        return element;
    }
}
