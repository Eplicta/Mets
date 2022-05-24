using System;
using System.Collections.Generic;

namespace Eplicta.Html.Entities;

public record HtmlTemplate
{
    public Element Root { get; set; }

    public record Element
    {
        public Dictionary<string, string> Attributes = new();
        public string Name { get; set; }
        public string Value { get; set; }
        public Element[] Children { get; set; } = Array.Empty<Element>();
    }
}