using Eplicta.Mets.Helpers;

namespace Eplicta.Mets.Entities
{
    public record ModsData //: MetsData
    {



        public TitleInfoData TitleInfo { get; set; }
        public NameData Name { get; set; }
        public Resource[] Resources { get; set; }


        public Agent[] agents { get; set; } //should collect d
        public string Creator { get; set; }


        public string CreateDate { get; set; }


        //Will be used to collect all nessesery data from the website one by one
        public record Agent{
            public string[] name { get; set; }
            public string[] note { get; set; }
            public string[] Role { get; set; }
            public string[] Type { get; set; }
            public string[] OtherType { get; set; }
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

        public record Resource
        {
            public string Name { get; set; }
            public byte[] Data { get; set; }
        }
    }
}