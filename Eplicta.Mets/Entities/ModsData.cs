namespace Eplicta.Mets.Entities
{
    public class ModsData //: MetsData
    {
        public TitleInfoData TitleInfo { get; set; }
        public NameData Name { get; set; }

        public class TitleInfoData
        {
            public string Title { get; set; }
            public string SubTitle { get; set; }
        }

        public class NameData
        {
            public string NamePart { get; set; }
        }
    }
}