using System.Text;
using System.Xml;

namespace Eplicta.Mets.Features.EArk;

/// <summary>
/// Removes what XML 1.0 cannot carry from a rendered document, so a control character pasted into a title never
/// makes a package that no archive can read.
/// </summary>
public static class XmlCharacters
{
    /// <summary>Cleans every attribute and text in the document in place, and returns it.</summary>
    public static XmlDocument Clean(XmlDocument document)
    {
        if (document?.DocumentElement != null) Clean(document.DocumentElement);

        return document;
    }

    /// <summary>
    /// The text with a space for each control character XML cannot hold, and without unpaired surrogates and
    /// non-characters; the same instance when there is nothing to remove.
    /// </summary>
    public static string Clean(string value)
    {
        if (string.IsNullOrEmpty(value) || IsClean(value)) return value;

        var builder = new StringBuilder(value.Length);
        for (var i = 0; i < value.Length; i++)
        {
            var character = value[i];
            if (XmlConvert.IsXmlChar(character))
            {
                builder.Append(character);
            }
            else if (IsPair(value, i))
            {
                builder.Append(character).Append(value[++i]);
            }
            else if (character < ' ')
            {
                builder.Append(' ');
            }
        }

        return builder.ToString();
    }

    private static void Clean(XmlNode node)
    {
        if (node.Attributes != null)
        {
            foreach (XmlAttribute attribute in node.Attributes)
            {
                var cleaned = Clean(attribute.Value);
                if (!ReferenceEquals(cleaned, attribute.Value)) attribute.Value = cleaned;
            }
        }

        foreach (XmlNode child in node.ChildNodes)
        {
            if (child is XmlCharacterData data)
            {
                var cleaned = Clean(data.Data);
                if (!ReferenceEquals(cleaned, data.Data)) data.Data = cleaned;
            }
            else
            {
                Clean(child);
            }
        }
    }

    private static bool IsClean(string value)
    {
        for (var i = 0; i < value.Length; i++)
        {
            if (XmlConvert.IsXmlChar(value[i])) continue;
            if (!IsPair(value, i)) return false;
            i++;
        }

        return true;
    }

    private static bool IsPair(string value, int index) =>
        char.IsHighSurrogate(value[index]) && index + 1 < value.Length && XmlConvert.IsXmlSurrogatePair(value[index + 1], value[index]);
}
