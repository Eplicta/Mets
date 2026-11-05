using System;

namespace Eplicta.Mets.Features.EArk;

/// <summary>
/// Markers for the packages Eplicta's own systems produce for FortDocs to ingest. A web archive carries its marker
/// as the OTHERCONTENTINFORMATIONTYPE; a case archive carries its marker as a MODS genre, because an ERMS package
/// must keep <see cref="Erms.ErmsConstants.ContentInformationType"/> as its content information type.
/// </summary>
public static class EplictaPackageTypes
{
    /// <summary>The OTHERCONTENTINFORMATIONTYPE of a web archive made by Conserver.</summary>
    public const string WebArchive = "Eplicta Web Archive";

    /// <summary>The MODS genre of a case archive exported from FortDocs as an ERMS package.</summary>
    public const string CaseArchive = "FortDocs Case Archive";

    /// <summary>Whether a value read from a package names the given package type, ignoring case and surrounding whitespace.</summary>
    public static bool Matches(string value, string packageType)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        return string.Equals(value.Trim(), packageType, StringComparison.OrdinalIgnoreCase);
    }
}
