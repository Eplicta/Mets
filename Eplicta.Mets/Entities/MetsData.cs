using System;

namespace Eplicta.Mets.Entities;

public record MetsData
{
    public enum EType
    {
        Individual,
        Organization,
        Other
    }

    public enum EOtherType
    {
        Software
    }

    public enum ERole
    {
        Creator,
        Editor,
        Archivist,
        Preservation,
        Disseminator,
        Custodian,
        IpOwner,
        Other
    }

    public enum EAltRecordType
    {
        DeliveryType,
        DeliverySpecification,
        SubmissionAgreement,
        PreviousSubmissionAgreement,
        DataSubmissionSession,
        PackageNumber,
        ReferenceCode,
        PreviousReferenceCode,
        Appraisal,
        AccessRestrict
    }

    public enum EChecksumType
    {
        MD5,
        SHA_1,
        SHA_256,
        SHA_384,
        SHA_512
    }

    public enum ELocType
    {
        Urn,
        Url,
        Handle,
        Other
    }

    public enum ENoteType
    {
        None,
        LikeCount,
        FollowerCount,
        PostId,
        PostMessage,
        ShareCount,
        Description,
        ReplyCount,
        RetweetCount,
        AuthorDisplayName,
        ChannelInformationPackageId,
        SourceChannel,
        Keywords,
        ViewCount
    }

    public enum EMetsHdrAttributeName
    {
        RecordStatus,
        CreateDate
    }

    public enum EMetsAttributeName
    {
        Label,
        ObjId
    }

    public MetsHdrData MetsHdr { get; set; }
    public AgentData Agent { get; set; }
    public CompanyData Company { get; set; }
    public SoftwareData Software { get; set; }
    public AltRecord[] AltRecords { get; set; }
    public ModsSectionData Mods { get; set; }
    public FileData[] Files { get; set; }
    public MetsAttribute[] Attributes { get; set; }
    public string MetsProfile {get; set;}

    public record MetsHdrData
    {
        public MetsHdrAttribute[] Attributes { get; set; }
    }

    public record AgentData
    {
        public string Name { get; set; }
        public string Note { get; set; }
        public EType Type { get; set; }
        public ERole Role { get; set; }
    }

    public record CompanyData
    {
        public string Name { get; set; }
        public string Note { get; set; }
        public ERole Role { get; set; }
        public EType Type { get; set; }
    }

    public record SoftwareData
    {
        public string Name { get; set; }
        public string Note { get; set; }
        public ERole Role { get; set; }
        public EType Type { get; set; }
        public EOtherType OtherType { get; set; }
    }

    public record AltRecord
    {
        public EAltRecordType Type { get; set; }
        public string InnerText { get; set; }
    }

    public record ModsNote
    {
        public ENoteType Type { get; set; }
        public string InnerText { get; set; }
        public string Href { get; set; }
    }

    public record ModsSectionData
    {
        public string Xmlns { get; set; }
        public string Identifier { get; set; }
        public Uri Url { get; set; }
        public DateTime DateIssued { get; set; }
        public string AccessCondition { get; set; }
        public string ModsTitle { get; set; }
        public Uri Uri { get; set; }
        public string ModsTitleInfo { get; set; }
        public PlaceInfo Place { get; set; }
        public ModsNote[] Notes { get; set; }
        public string Publisher { get; set; }
    }

    public record FileData
    {
        public string Id { get; set; }
        public string Use { get; set; }
        public string MimeType { get; set; }
        public long Size { get; set; }
        public DateTime Created { get; set; }
        public string Checksum { get; set; }
        public EChecksumType ChecksumType { get; set; }
        public ELocType LocType { get; set; }
        public string FileName { get; set; }
        public byte[] Data { get; set; }
        //public string Ns2Href { get; set; }
    }
    public record Attribute
    {
        public string Value { get; set; }
    }

    public record MetsHdrAttribute : Attribute
    {
        public EMetsHdrAttributeName Name { get; set; }
    }

    public record MetsAttribute : Attribute
    {
        public EMetsAttributeName Name { get; set; }
    }

    public record PlaceInfo
    {
        public string PlaceTerm { get; set; }
    }
}