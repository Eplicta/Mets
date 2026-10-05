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

    public enum EContentInformationType
    {
        ERMS,
        SIARD1,
        SIARD2,
        SIARDDK,
        GeoData,
        MIXED,
        OTHER
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
