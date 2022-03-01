using System.Linq;
using System.Text;
using Eplicta.Html.Entities;
using System.Collections.Generic;
 


namespace Eplicta.Html
{
    public class Renderer
    {
        private readonly HtmlTemplate _template;
        private readonly HtmlData _htmlData;

        public Renderer(HtmlTemplate template, HtmlData htmlData)
        {
            _template = template;
            _htmlData = htmlData;
        }

        public object DateTime { get; private set; }

        public string Render()
        {
            var sb = new StringBuilder();

            RenderChildren(new[] { _template.Root }, sb);

            var result = sb.ToString();
            return result;
        }
        private string GetValue(string value)
        {

            if (value != null && value.Contains("{"))
            {
                var k = value;
                var iStart = k.IndexOf("{");
                var iEnd = k.IndexOf("}");
                var key1 = k.Substring(iStart + 1, iEnd - iStart - 1);

                if (_htmlData.Data.TryGetValue(key1, out var val))
                {
                    var pre = value.Substring(0, iStart);
                    var suff = value.Substring(iEnd + 1);

                    value = pre + val + suff;
                }
            }

            return value;
        }

        private void RenderChildren(HtmlTemplate.Element[] nodes, StringBuilder sb, int indent = 0)
        {
            var indentation = new string(' ', indent);

            foreach (var node in nodes)
            {
                var attr = "";// $" name=\"anka\"";
                if (node.Attributes.Any())
                {
                    foreach (var attribute in node.Attributes)
                    {
                        //sb.AppendLine($"{attribute.Key}=\"{attribute.Value}\"");
                        var attributeValue = GetValue(attribute.Value);
                        attr += $" {attribute.Key}=\"{attributeValue}\"";
         
                    }                    
                }
                var Hår = node.Value;
                if (Hår != null && Hår.Contains("{publishDate}"))
                {  
                    System.DateTime hd = System.DateTime.Now;
                    node.Value = hd.ToString("O");
                }

                var value = GetValue(node.Value);
           
                if (!string.IsNullOrEmpty(value))
                {
                    sb.AppendLine($"{indentation}<{node.Name}{attr}>{value}</{node.Name}>");
                }              
                else if (node.Children.Any())
                {
                    sb.AppendLine($"{indentation}<{node.Name}{attr}>");
                    indent += 4;
                    RenderChildren(node.Children, sb, indent);
                    indent -= 4;
                    sb.AppendLine($"{indentation}</{node.Name}/>");
                }
                else
                {
                    sb.AppendLine($"{indentation}<{node.Name}{attr}/>");
                }
                
 
            }
        }
    }
}