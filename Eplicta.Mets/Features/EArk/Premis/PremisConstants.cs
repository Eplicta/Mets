namespace Eplicta.Mets.Features.EArk.Premis;

/// <summary>
/// Fixed strings of PREMIS 3.0, the preservation metadata an E-ARK package carries under metadata/preservation.
/// </summary>
public static class PremisConstants
{
    public const string Namespace = "http://www.loc.gov/premis/v3";
    public const string Prefix = "premis";
    public const string Version = "3.0";

    public const string FileName = "premis.xml";
    public const string SchemaFileName = "premis-v3-0.xsd";

    /// <summary>How a file in metadata/preservation reaches the package's schemas folder.</summary>
    public const string SchemasPathFromPreservationMetadata = "../../schemas";

    public const string LocalIdentifierType = "local";
    public const string Sha256 = "SHA-256";

    internal const string SchemaResource = "EArk.premis-v3-0.xsd";
}
