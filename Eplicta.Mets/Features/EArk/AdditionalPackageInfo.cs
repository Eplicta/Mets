using System;

namespace Eplicta.Mets.Features.EArk;

/// <summary>
/// The content of Riksarkivet's additionalPackageInfo.xml, which their application of E-ARK CSIP and SIP adds
/// to the documentation folder. It carries Swedish archival governance metadata that E-ARK itself has nowhere
/// to put. Required only for deliveries to Riksarkivet.
/// </summary>
public record AdditionalPackageInfo
{
    public enum ECreatorCodeType
    {
        VAT,
        DUNS,
        ORG,
        HSA,
        Local,
        URI,
        OTHER
    }

    public Guid Identification { get; init; } = Guid.NewGuid();
    public EArkPackage.ERecordStatus Status { get; init; } = EArkPackage.ERecordStatus.New;
    public DateTime CreationDate { get; init; }
    public string CreatorName { get; init; }
    public string CreatorCode { get; init; }
    public ECreatorCodeType CreatorCodeType { get; init; } = ECreatorCodeType.ORG;
    public string CreatorDescription { get; init; }
    public string ArchiveName { get; init; }
    public bool Disposable { get; init; }
    public string DisposalDescription { get; init; }
    public bool AccessRestricted { get; init; }
    public string AccessRestrictDescription { get; init; }
    public bool UseRestricted { get; init; }
    public string UseRestrictDescription { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string InformationClass { get; init; }
    public string SecurityClassification { get; init; }
    public bool Publishable { get; init; }
    public string ArchivalReferenceCode { get; init; }
}
