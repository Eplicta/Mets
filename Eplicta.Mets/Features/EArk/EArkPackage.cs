using System;
using System.Collections.Generic;

namespace Eplicta.Mets.Features.EArk;

/// <summary>
/// An E-ARK information package as described by its METS document. Build one with <see cref="EArkPackageBuilder"/>.
/// </summary>
public record EArkPackage
{
    public enum EOaisPackageType
    {
        SIP,
        AIP,
        DIP,
        AIU,
        AIC
    }

    public enum ERecordStatus
    {
        New,
        Supplement,
        Replacement,
        Test,
        Version,
        Delete,
        Other
    }

	/// <summary>
	/// TODO: Never rendered..?
	/// Usages
	/// * Automated Validation: When an archive ingests a package, the CONTENTINFORMATIONTYPE value triggers the appropriate validation rules (such as checking ERMS.xsd for records or Schematron rules for geospatial layers).
	/// * Rendering and Reuse: It informs dissemination tools how to unpack and display the content later—such as loading a Geodata package into GIS software or querying a SIARD package as a database
	/// </summary>
	public enum EContentInformationType
    {
		/// <summary>
		/// Preserving records, case files, folder hierarchies, and administrative metadata exported from Electronic Records Management Systems.
		/// </summary>
		ERMS,
		/// <summary>
		/// <see cref="SIARD"/>
		/// </summary>
		[Obsolete]
		SIARD1,
		/// <summary>
		/// <see cref="SIARD"/>
		/// </summary>
		[Obsolete]
		SIARD2,
		/// <summary>
		/// <see cref="SIARD"/>
		/// </summary>
		[Obsolete]
		SIARDDK,
		/// <summary>
		/// Archiving GIS layers, vector/raster maps, coordinate reference systems, and spatial metadata aligned with the EU INSPIRE directive.
		/// </summary>
		GeoData,
        MIXED,
		/// <summary>
		/// Used when a package contains data governed by a local, national, or newly proposed specification not yet in the core DILCIS vocabulary.
		/// </summary>
		OTHER,
		/// <summary>
		/// Preserving 3D CAD engineering models, manufacturing geometry, and product lifecycle metadata.
		/// </summary>
		_3D,
		/// <summary>
		/// Preserving relational databases in a vendor-neutral XML and folder format (SIARD 1, 2, or SIARDDK) so tables and schemas survive software obsolescence.
		/// </summary>
		SIARD,
	}

    public string ObjId { get; init; }
    public string Label { get; init; }
    public string ContentCategory { get; init; } = EArkConstants.ContentCategory.WebArchives;
    public string OtherContentCategory { get; init; }
    public EContentInformationType ContentInformationType { get; init; } = EContentInformationType.OTHER;
    public string OtherContentInformationType { get; init; }
    public EOaisPackageType OaisPackageType { get; init; } = EOaisPackageType.SIP;
    public ERecordStatus RecordStatus { get; init; } = ERecordStatus.New;
    public DateTime CreateDate { get; init; }
    public DateTime? LastModDate { get; init; }
    public string RepresentationName { get; init; } = EArkConstants.DefaultRepresentationName;
    public IReadOnlyList<EArkAgent> Agents { get; init; } = [];
    public IReadOnlyList<EArkAltRecordId> AltRecordIds { get; init; } = [];
    public IReadOnlyList<EArkMetadataReference> DescriptiveMetadata { get; init; } = [];
    public IReadOnlyList<EArkMetadataReference> PreservationMetadata { get; init; } = [];
    public IReadOnlyList<EArkFile> DocumentationFiles { get; init; } = [];
    public IReadOnlyList<EArkFile> SchemaFiles { get; init; } = [];
    public IReadOnlyList<EArkFile> RepresentationFiles { get; init; } = [];
    public AdditionalPackageInfo AdditionalPackageInfo { get; init; }
}
