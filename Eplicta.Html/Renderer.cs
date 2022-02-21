using System.Linq;
using System.Text;
using Eplicta.Html.Entities;

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

        public string Render()
        {
            var sb = new StringBuilder();

            RenderChildren(new[] { _template.Root }, sb);

            var result = sb.ToString();
            return result;
        }

        private static void RenderChildren(HtmlTemplate.Element[] nodes, StringBuilder sb, int indent = 0)
        {
            var indentation = new string(' ', indent);

            foreach (var node in nodes)
            {
                if (!string.IsNullOrEmpty(node.Value))
                {
                    sb.AppendLine($"{indentation}<{node.Name}>{node.Value}</{node.Name}>");
                }
                else if (node.Children.Any())
                {
                    sb.AppendLine($"{indentation}<{node.Name}>");
                    indent += 4;
                    RenderChildren(node.Children, sb, indent);
                    indent -= 4;
                    sb.AppendLine($"{indentation}</{node.Name}/>");
                }
                else
                {
                    sb.AppendLine($"{indentation}<{node.Name}/>");
                }
            }
        }
    }
}