using System.Collections.Generic;

namespace Eplicta.Html.Entities
{
    public record HtmlData
    {
        public Dictionary<string, string> Data { get; set; }

        public Dictionary<string, string> Anka { get; set; }

        public Dictionary<string, string> Fisk { get; set; } 

        public List<Dictionary<string, string>> Recourses { get; set; }
    }
}