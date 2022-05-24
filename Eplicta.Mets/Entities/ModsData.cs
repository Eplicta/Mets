using System;

namespace Eplicta.Mets.Entities;

public record ModsData
{
    public TitleInfoData TitleInfo { get; set; }
    public NameData Name { get; set; }
    public ResourceData[] Resources { get; set; }
    public AgentData Agent { get; set; }
    public CompanyData Company { get; set; }
    public SoftwareData Software { get; set; }
    public AltRecordId Records { get; set; }
    public ModsSectionData Mods { get; set; }
    public FileData[] Files { get; set; }
    public string Creator { get; set; }

    public record AgentData
    {
        public string Name { get; set; }
        public string Note { get; set; }
        public string Type { get; set; }
        public string Role { get; set; }
    }

    public record CompanyData
    {
        public string Name { get; set; }
        public string Note { get; set; }
        public string Role { get; set; }
        public string Type { get; set; }
    }

    public record SoftwareData
    {
        public string Name { get; set; }
        public string Note { get; set; }
        public string Role { get; set; }
        public string Type { get; set; }
        public string OtherType { get; set; }
    }

    public record AltRecordId
    {
        public string Type1 { get; set; }
        public string InnerText1 { get; set; }
        public string Type2 { get; set; }
        public string InnerText2 { get; set; }
        public string Type3 { get; set; }
        public string InnerText3 { get; set; }
    }

    public record ModsSectionData
    {
        public string ObjId { get; set; }
        public string Xmlns { get; set; }
        public string Identifier { get; set; }
        public string Url { get; set; }
        public DateTime DateIssued { get; set; }
        public string AccessCondition { get; set; }
        public string ModsTitle { get; set; }
        public string Uri { get; set; }
        public string ModeTitle2 { get; set; }
    }

    public record FileData
    {
        public string Id { get; set; }
        public string Use { get; set; }
        public string MimeType { get; set; }
        public long Size { get; set; }
        public string Created { get; set; }
        public string CheckSum { get; set; }
        public string ChecksumType { get; set; }
        public string Ns2Type { get; set; }
        public string Ns2Href { get; set; }
        public string LocType { get; set; }
    }

    public record TitleInfoData
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
    }

    public record NameData
    {
        public string NamePart { get; set; }
    }

    public record ResourceData
    {
        public string Name { get; set; }
        public byte[] Data { get; set; }
    }
}