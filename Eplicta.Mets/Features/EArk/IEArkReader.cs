using Eplicta.Mets.Entities;

namespace Eplicta.Mets.Features.EArk;

/// <summary>
/// Reads the METS document of an E-ARK information package, and the MODS documents it references, back into objects.
/// </summary>
public interface IEArkReader
{
    /// <summary>
    /// Reads an E-ARK METS document into the <see cref="EArkPackage"/> that <see cref="EArkRenderer"/> would render it from.
    /// </summary>
    EArkPackage Read(string metsXml);

    /// <summary>
    /// Reads a standalone MODS document, such as metadata/descriptive/mods.xml, into its MODS element.
    /// </summary>
    DeserializedMets.DmdSecElement.MdWrapElement.XmlDataElement.ModsBaseElement ReadMods(string modsXml);
}
