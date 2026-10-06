namespace Eplicta.Mets.Features.EArk;

/// <summary>
/// Fixed strings defined by E-ARK CSIP and E-ARK SIP version 2.1.0, and by Riksarkivet's application of them.
/// </summary>
public static class EArkConstants
{
    public const string MetsNamespace = "http://www.loc.gov/METS/";
    public const string CsipNamespace = "https://DILCIS.eu/XML/METS/CSIPExtensionMETS";
    public const string SipNamespace = "https://DILCIS.eu/XML/METS/SIPExtensionMETS";
    public const string XlinkNamespace = "http://www.w3.org/1999/xlink";
    public const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";
    public const string AdditionalPackageInfoNamespace = "http://xml.ra.se/e-arkiv/implementation-IP/version10/SNAadditionalPackageInfo";

    public const string CsipPrefix = "csip";
    public const string SipPrefix = "sip";
    public const string XlinkPrefix = "xlink";
    public const string XsiPrefix = "xsi";

    public const string SipProfile = "https://earksip.dilcis.eu/profile/E-ARK-SIP.xml";
    public const string CsipProfile = "https://earkcsip.dilcis.eu/profile/E-ARK-CSIP.xml";

    public const string MetsFileName = "METS.xml";
    public const string AdditionalPackageInfoFileName = "additionalPackageInfo.xml";

    public const string MetadataFolderName = "metadata";
    public const string DescriptiveFolderName = "descriptive";
    public const string PreservationFolderName = "preservation";
    public const string RepresentationsFolderName = "representations";
    public const string DataFolderName = "data";
    public const string SchemasFolderName = "schemas";
    public const string DocumentationFolderName = "documentation";
    public const string DefaultRepresentationName = "rep_1";

    public const string DocumentationUse = "Documentation";
    public const string SchemasUse = "Schemas";
    public const string RepresentationsUse = "Representations";
    public const string MetadataLabel = "Metadata";

    public const string StructMapType = "PHYSICAL";
    public const string StructMapLabel = "CSIP";
    public const string LocType = "URL";
    public const string XlinkSimple = "simple";

    public const string SoftwareAgentOtherType = "SOFTWARE";
    public const string SoftwareVersionNoteType = "SOFTWARE VERSION";
    public const string IdentificationCodeNoteType = "IDENTIFICATIONCODE";

    /// <summary>
    /// Dates are written as a true UTC instant. Riksarkivet's example omits the zone designator, which is
    /// also valid xsd:dateTime, but an explicit instant cannot be misread in another time zone.
    /// </summary>
    public const string Iso8601UtcFormat = "yyyy-MM-ddTHH:mm:ss'Z'";
    public const string IdPrefix = "uuid-";

    /// <summary>
    /// Values for the mets/@TYPE Content Category vocabulary that this library emits. The vocabulary is far
    /// larger; METS itself does not constrain the attribute, so callers may supply any term verbatim.
    /// </summary>
    public static class ContentCategory
    {
        public const string WebArchives = "Web Archives";
        public const string Websites = "Websites";
        public const string Datasets = "Datasets";
        public const string Other = "OTHER";
    }
}
