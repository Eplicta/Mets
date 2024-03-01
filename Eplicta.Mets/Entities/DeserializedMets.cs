using System;
using System.Linq;
using System.Xml.Serialization;

namespace Eplicta.Mets.Entities;

[XmlRoot(ElementName = "mets", Namespace = "http://www.loc.gov/METS/")]
public record DeserializedMets
{
    [XmlElement("metsHdr")]
    public MetsHdrElement MetsHdr { get; set; }

    [XmlElement("dmdSec")]
    public DmdSecElement DmdSec { get; set; }

    [XmlElement("fileSec")]
    public FileSecElement FileSec { get; set; }

    [XmlElement("structMap")]
    public StructMapElement StructMap { get; set; }

    public record MetsHdrElement
    {
        [XmlElement("agent")]
        public AgentElement[] Agents { get; set; } = Array.Empty<AgentElement>();

        [XmlAttribute("CREATEDATE")]
        public DateTime CreateDate { get; set; }

        [XmlAttribute("RECORDSTATUS")]
        public string RecordStatus { get; set; }

        public AgentElement Creator => Agents.FirstOrDefault(x => x.Role == "CREATOR");
        public AgentElement Editor => Agents.FirstOrDefault(x => x.Role == "EDITOR");
        public AgentElement Archivist => Agents.FirstOrDefault(x => x.Role == "ARCHIVIST");

        public record AgentElement
        {
            [XmlAttribute("ROLE")]
            public string Role { get; set; }

            [XmlAttribute("TYPE")]
            public string Type { get; set; }

            [XmlAttribute("OTHERTYPE")]
            public string OtherType { get; set; }

            [XmlElement("name")]
            public string Name { get; set; }

            [XmlElement("note")]
            public string Note { get; set; }
        }
    }

    public record DmdSecElement
    {
        [XmlAttribute("ID")]
        public string Id { get; set; }

        [XmlElement("mdWrap")]
        public MdWrapElement MdWrap { get; set; }

        public record MdWrapElement
        {
            [XmlAttribute("MDTYPE")]
            public string MdType { get; set; }

            [XmlElement("xmlData")]
            public XmlDataElement XmlData { get; set; }

            public record XmlDataElement
            {
                [XmlElement(ElementName = "mods", Namespace = "http://www.loc.gov/mods/v3")]
                public ModsBaseElement Mods { get; set; }

                public record ModsBaseElement
                {
                    [XmlElement(ElementName = "identifier", Namespace = "http://www.loc.gov/mods/v3")]
                    public ModsIdentifierElement Identifier { get; set; }

                    [XmlElement(ElementName = "location", Namespace = "http://www.loc.gov/mods/v3")]
                    public ModsLocationElement Location { get; set; }

                    [XmlElement(ElementName = "originInfo", Namespace = "http://www.loc.gov/mods/v3")]
                    public OriginInfoElement OriginInfo { get; set; }

                    [XmlElement(ElementName = "titleInfo", Namespace = "http://www.loc.gov/mods/v3")]
                    public TitleInfoElement TitleInfo { get; set; }

                    [XmlElement(ElementName = "relatedItem", Namespace = "http://www.loc.gov/mods/v3")]
                    public RelatedItemElement RelatedItem { get; set; }

                    [XmlElement(ElementName = "note", Namespace = "http://www.loc.gov/mods/v3")]
                    public ModsNoteElement[] Notes { get; set; }

                    public record ModsIdentifierElement : XmlValueElement<string>
                    {
                        [XmlAttribute("type")]
                        public string Type { get; set; }
                    }

                    public record ModsLocationElement
                    {
                        [XmlElement(ElementName = "url", Namespace = "http://www.loc.gov/mods/v3")]
                        public XmlValueElement<string> Url { get; set; }
                    }

                    public record OriginInfoElement
                    {
                        [XmlElement(ElementName = "dateIssued", Namespace = "http://www.loc.gov/mods/v3")]
                        public DateIssuedElement DateIssued { get; set; }
                    }

                    public record TitleInfoElement
                    {
                        [XmlElement(ElementName = "title", Namespace = "http://www.loc.gov/mods/v3")]
                        public XmlValueElement<string> Title { get; set; }
                    }

                    public record RelatedItemElement
                    {
                        [XmlElement(ElementName = "identifier", Namespace = "http://www.loc.gov/mods/v3")]
                        public ModsIdentifierElement Identifier { get; set; }

                        [XmlElement(ElementName = "titleInfo", Namespace = "http://www.loc.gov/mods/v3")]
                        public TitleInfoElement TitleInfo { get; set; }
                    }

                    public record DateIssuedElement : XmlValueElement<DateTime>
                    {
                        [XmlAttribute("encoding")]
                        public string Encoding { get; set; }
                    }
                }
            }
        }
    }

    public record FileSecElement
    {
        [XmlElement("fileGrp")]
        public FileGrpElement FileGrp { get; set; }

        public record FileGrpElement
        {
            [XmlElement("file")]
            public FileElement[] Files { get; set; }

            public record FileElement
            {
                [XmlAttribute("ID")]
                public string Id { get; set; }

                [XmlAttribute("USE")]
                public string Use { get; set; }

                [XmlAttribute("MIMETYPE")]
                public string MimeType { get; set; }

                [XmlAttribute("SIZE")]
                public string Size { get; set; }

                [XmlAttribute("CREATED")]
                public DateTime Created { get; set; }

                [XmlAttribute("CHECKSUM")]
                public string CheckSum { get; set; }

                [XmlAttribute("CHECKSUMTYPE")]
                public MetsData.EChecksumType CheckSumType { get; set; }

                [XmlElement("FLocat")]
                public FLocatElement FLocat { get; set; }

                public record FLocatElement
                {
                    [XmlAttribute("LOCTYPE")]
                    public string LocType { get; set; }

                    [XmlAttribute(AttributeName = "href", Namespace = "http://www.w3.org/1999/xlink")]
                    public string Href { get; set; }

                    [XmlAttribute(AttributeName = "type", Namespace = "http://www.w3.org/1999/xlink")]
                    public string Type { get; set; }
                }
            }
        }
    }

    public record StructMapElement
    {
        [XmlElement("div")]
        public PublicationDivContainerElement Div { get; set; }

        public record DivElement
        {
            [XmlAttribute("TYPE")]
            public string Type { get; set; }
        }

        public record PublicationDivContainerElement : DivElement
        {
            [XmlElement("div")]
            public FptrContainerElement Container { get; set; }
        }

        public record FptrContainerElement : DivElement
        {
            [XmlElement("fptr")]
            public FtprElement[] Ftpr { get; set; }

            public record FtprElement
            {
                [XmlAttribute("FILEID")]
                public string FileId { get; set; }
            }
        }
    }

    public record ModsNoteElement : XmlValueElement<string>
    {
        [XmlAttribute("type")]
        public string Type { get; set; }
    }
}

public record XmlValueElement<T>
{
    [XmlText]
    public T Value { get; set; }

    public static implicit operator T(XmlValueElement<T> e)
    {
        return e.Value;
    }
}