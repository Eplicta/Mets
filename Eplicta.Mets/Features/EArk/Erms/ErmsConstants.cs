namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// Fixed strings defined by E-ARK CITS ERMS 3.0, which Riksarkivet adopts unchanged as FGS Ärendehantering 3.0.
/// </summary>
public static class ErmsConstants
{
    public const string Namespace = "https://DILCIS.eu/XML/ERMS";
    public const string SchematronNamespace = "http://purl.oclc.org/dsdl/schematron";

    public const string FileName = "ERMS.xml";
    public const string SchemaFileName = "ERMS_v3.xsd";
    public const string SchematronFileName = "erms_v3.sch";

    /// <summary>The CONTENTINFORMATIONTYPE term for an ERMS package, given as OTHER under CSIP 2.1.0.</summary>
    public const string ContentInformationType = "citserms_v3_0";

    /// <summary>How a file in representations/&lt;rep&gt;/data reaches the package's schemas folder.</summary>
    public const string SchemasPathFromRepresentationData = "../../../schemas";

    internal const string SchemaResource = "EArk.ERMS_v3.xsd";
    internal const string SchematronResource = "EArk.erms_v3.sch";
}
