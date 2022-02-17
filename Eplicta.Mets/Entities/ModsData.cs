using Eplicta.Mets.Helpers;

namespace Eplicta.Mets.Entities
{
    public record ModsData //: MetsData
    {



        public TitleInfoData TitleInfo { get; set; }
        public NameData Name { get; set; }
        public Resource[] Resources { get; set; }


        public Agentdata agent { get; set; } //should collect d
        public company eplicta { get; set; }
        public AltRecordID records { get; set; }

        public string Creator { get; set; }


        public string CreateDate { get; set; }


        //Will be used to collect all nessesery data from the website one by one
        public record Agentdata{
            public string name { get; set; }
            public string note { get; set; }
            public string Role { get; set; }
            public string Type { get; set; }
            public string OtherType { get; set; }
        }

        public record company
        {
            public string name { get; set; } = "Eplicta";
            public string note { get; set; }
            public string Role { get; set; } = "Editor";
            public string Type { get; set; } = "Organisation";

        }

        public record AltRecordID
        {
            public string name2 { get; set; }
            public string type1 { get; set; } = "DELIVERYTYPE";
            public string type2 { get; set; } = "DELIVERYSPECIFICATION";
            public string type3 { get; set; } = "SUBMISSIONAGREEMENT";
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