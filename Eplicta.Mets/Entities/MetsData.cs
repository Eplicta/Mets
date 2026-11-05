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
        AccessRestrict,
        METS
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

    public static class ENoteType
	{
		// https://www.loc.gov/standards/mods/mods-notes.html
		// tmp.split('\n').map(o => o.trim()).filter(o => o.length).map(o => `public const string ${o.split(/[ /]/).map(p => p[0].toUpperCase() + p.slice(1)).join("")} = "${o}";`).join("\n")

		public const string AccrualMethod = "accrual method";
        public const string AccrualPolicy = "accrual policy";
		public const string Acquisition = "acquisition";
		public const string Action = "action";
		public const string AdditionalPhysicalForm = "additional physical form";
		public const string Admin = "admin";
		public const string BibliographicHistory = "bibliographic history";
		public const string Bibliography = "bibliography";
		public const string BiographicalHistorical = "biographical/historical";
		public const string CitationReference = "citation/reference";
		public const string ConservationHistory = "conservation history";
		public const string Content = "content";
		public const string CreationProductionCredits = "creation/production credits";
		public const string Date = "date";
		public const string Exhibitions = "exhibitions";
		public const string Funding = "funding";
		public const string Handwritten = "handwritten";
		public const string Language = "language";
		public const string Numbering = "numbering";
		public const string DateSequentialDesignation = "date/sequential designation";
		public const string OriginalLocation = "original location";
		public const string OriginalVersion = "original version";
		public const string Orthography = "orthography";
		public const string Ownership = "ownership";
		public const string Performers = "performers";
		public const string PreferredCitation = "preferred citation";
		public const string Publications = "publications";
		public const string Reproduction = "reproduction";
		public const string Restriction = "restriction";
		public const string SourceCharacteristics = "source characteristics";
		public const string SourceDimensions = "source dimensions";
		public const string SourceIdentifier = "source identifier";
		public const string SourceNote = "source note";
		public const string SourceType = "source type";
		public const string StatementOfResponsibility = "statement of responsibility";
		public const string SubjectCompleteness = "subject completeness";
		public const string SystemDetails = "system details";
		public const string Thesis = "thesis";
		public const string Venue = "venue";
		public const string VersionIdentification = "version identification";

		public static class SocialMedia
        {
			public const string LikeCount = "likeCount";
			public const string FollowerCount = "followerCount";
			public const string PostId = "postId";
			public const string ReplyCount = "replyCount";
			public const string RetweetCount = "retweetCount";
			public const string ViewCount = "viewCount";
			public const string ShareCount = "shareCount";
			public const string None = "none";
			public const string PostMessage = "postMessage";
			public const string Description = "description";
			public const string AuthorDisplayName = "authorDisplayName";
			public const string ChannelInformationPackageId = "channelInformationPackageId";
			public const string SourceChannel = "sourceChannel";
			public const string Keywords = "keywords";
			public const string CollectDate = "collectDate";
		}
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

    public enum ENameType
    {
        Personal,
        Corporate
    }

    public MetsHdrData MetsHdr { get; set; }
    public AgentData[] Agents { get; set; }
    //public CompanyData Company { get; set; }
    public SoftwareData Software { get; set; }
    public AltRecord[] AltRecords { get; set; }
    public ModsSectionData Mods { get; set; }
    public FileData[] Files { get; set; }

    public StreamFileSource[] Sources { get; set; }

    public MetsAttribute[] Attributes { get; set; }
    //public string MetsProfile {get; set;}

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

    //public record CompanyData
    //{
    //    public string Name { get; set; }
    //    public string Note { get; set; }
    //    public ERole Role { get; set; }
    //    public EType Type { get; set; }
    //}

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
        public EAltRecordType? Type { get; set; }
        public string InnerText { get; set; }
    }

    public record ModsNote
    {
		//public ENoteType Type { get; set; }
		public required string Type { get; set; }
		public string InnerText { get; set; }
        public string Href { get; set; }
    }

    public record ModsName
    {
        public string DisplayName { get; set; }
        public ENameType Type { get; set; }
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
        public ModsName Creator { get; set; }
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