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
        public CompanySoftware software { get; set; } 
        public AltRecordID records { get; set; }
        public ModsSectionInfo mods { get; set; }
        public file[] files { get; set; }


        public string Creator { get; set; }


        public string CreateDate { get; set; }


        //Will be used to collect all nessesery data from the website
        //All necessery info about the websites will be located here
        //Dynamic data
        public record Agentdata{
            public string name { get; set; } = "Alingsås Kommun";
            public string note { get; set; } = "http://id.kb.se/organisations/SE2120001553";
            public string Type { get; set; } = "ORGANIZATION".ToUpper();
            public string Role { get; set; } = "ARCHIVIST".ToUpper();
        }

        //All info about the company doing the E-plikt
        //Static data
        public record company
        {
            public string name { get; set; } = "Eplicta AB";
            public string note { get; set; } = "http://id.kb.se/organisations/SE5590369160";
            public string Role { get; set; } = "Editor".ToUpper();
            public string Type { get; set; } = "Organisation".ToUpper();

        }

        public record CompanySoftware
        {
            public string name { get; set; } = "Eplicta Aggregator";
            public string note { get; set; } = "http://id.kb.se/organisations/SE5590369160";
            public string Role { get; set; } = "Editor";
            public string Type { get; set; } = "other";
            public string othertype { get; set; } = "software";

        }


        //record ID's 
        //Static data
        public record AltRecordID
        {
            public string type1 { get; set; } = "DELIVERYTYPE";
            public string innertext1 { get; set; } = "DEPOSIT".ToUpper();
            public string type2 { get; set; } = "DELIVERYSPECIFICATION".ToUpper();
            public string innertext2 { get; set; } = "http://www.kb.se/namespace/digark/deliveryspecification/deposit/fgs-publ/v1/";
            public string type3 { get; set; } = "SUBMISSIONAGREEMENT".ToUpper();
            /*public string code { get; set; } = "31-KB999-2013";*/ //calculated code for back of the innertext3 link
            public string innertext3 { get; set; } = $"http://www.kb.se/namespace/digark/submissionagreement/31-KB999-2013";
        }

        //Everything for Mods section
        //Mostly dynamic data
        public record ModsSectionInfo
        {
            public string ObjId { get; set; } = "UUID:test ID";
            public string xmlns { get; set; } = "http://www.w3.org/1999/xlink";
            public string identifier { get; set; } = "C5385FBC5FC559E7C43AB6700DB28EF3".ToUpper();
            public string URL { get; set; } = "https://www.alingsas.se/utbildning-och-barnomsorg/vuxenutbildning/jag-vill-studera/program-i-alingsas/moln-och-virtualiseringspecialist/";
            public string DateIssued { get; set; } = "2022-02-03T15:48:04.000+01:00";
            public string accesscondition { get; set; } = "gratis";
            public string modstitle { get; set; } = "Moln- och virtualiseringspecialist";
            public string uri { get; set; } = "https://www.alingsas.se/";
            public string modstitle2 { get; set; } = "https://www.alingsas.se/";
        }


        //files section for storing data about files
        //Dynamic and Calculated data, and pronom-code
        public record file
        {
            public string ID { get; set; } = "ID4d6bdd9068214aa5a57d53bdbe4a9cf3";  //Calculated data
            public string USE { get; set; } = "Acrobat PDF/X - Portable Document Format - Exchange 1:1999;PRONOM:fmt/144"; //Pronom-code
            public string MIMETYPE { get; set; } = "application/pdf";   //Dynamic data
            public string SIZE { get; set; } = "1145856"; //Calculated data
            public string CREATED { get; set; } = "2022-02-19T16:44:44.000+01:00";  //Calculated data
            public string CHECKSUM { get; set; } = "801520fe16da09d1365596dfabb2846b"; //Calculated data
            public string CHECKSUMTYPE { get; set; } = "MD5"; //static data
            public string ns2Type { get; set; } = "simple";
            public string ns2href { get; set; } = "file:Content/Moln-och-virtualiseringsspecialist.pdf";
            public string loctype { get; set; } = "URL";

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