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

            //sb.AppendLine("<metaData>");
            //sb.AppendLine("<epafDocumentId>");
            //sb.Append("EF7279696F91C83870A84C51E2EB48D1");
            //sb.AppendLine("</epafDocumentId>");
            //sb.AppendLine("<externalId>");
            //sb.Append("113824134542904_117380220853962");
            //sb.AppendLine("</externalId");
            //sb.AppendLine("<uri>");
            //sb.Append("https://www.facebook.com/113824134542904/posts/117380220853962/");
            //sb.AppendLine("</uri>");
            //sb.AppendLine("<publishDate>");
            //sb.Append("2022-02-07T13:14:22.0000000Z");
            //sb.AppendLine("</publishDate>");
            //sb.AppendLine("<title>");
            //sb.Append("Detta är ett inlägg med bild och location");
            //sb.AppendLine("</title>");
            //sb.AppendLine("<postMessage>");
            //sb.Append("<![CDATA[ Detta är ett inlägg med bild och location. ]]>");
            //sb.AppendLine("</postMessage>");
            //sb.AppendLine("<likeCount>");
            //sb.Append("1");
            //sb.AppendLine("</likeCount>");
            //sb.AppendLine("<shareCount>");
            //sb.Append("0");
            //sb.AppendLine("</shareCount>");
            //sb.AppendLine("<location>");
            //sb.AppendLine("<name>");
            //sb.Append("Stockholm");
            //sb.AppendLine("</name>");
            //sb.AppendLine("<link>");
            //sb.Append("https://www.facebook.com/106505586052951");
            //sb.AppendLine("</link>");
            //sb.AppendLine("</location>");
            //sb.AppendLine("</metaData>");
            //sb.AppendLine("<channelInfo>");
            //sb.AppendLine("<epafChannelId>");
            //sb.Append("C18D7E70CD734F8286E2CA6E6490D56E");
            //sb.AppendLine("</epafChannelId>");
            //sb.AppendLine("<channelKey>");
            //sb.Append("Harvester testflöde-113824134542904");
            //sb.AppendLine("</channelKey>");
            //sb.AppendLine("<channelType>");
            //sb.Append("Facebook");
            //sb.AppendLine("</channelType>");
            //sb.AppendLine("</channelInfo>");
            //sb.AppendLine("<resources>");
            //sb.AppendLine("<file name=Content / 273280339_117380057520645_1825831192817841875_n.jpg>");
            //sb.AppendLine("<attachmentType>");
            //sb.Append("photo");
            //sb.AppendLine("</attachmentType>"); 
            //sb.AppendLine("</file");
            //sb.AppendLine("</resources>");




            var result = sb.ToString();
            return result;
        }

        private static void RenderChildren(HtmlTemplate.Element[] nodes, StringBuilder sb, int indent = 0)
        {
            var indentation = new string(' ', indent);

            foreach (var node in nodes)
            {
                var attr = "";// $" name=\"anka\"";
                if (node.Attributes.Any())
                {
                    foreach (var attribute in node.Attributes)
                    {
                        //sb.AppendLine($"{attribute.Key}={attribute.Value}");
                        attr = $" {attribute.Key}=\"{attribute.Value}\"";
                    }

                }

                if (!string.IsNullOrEmpty(node.Value))
                {
                    sb.AppendLine($"{indentation}<{node.Name}>{node.Value}</{node.Name}>");
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
                    sb.AppendLine($"{indentation}<{node.Name}/>");
                }
                
                
            }
        }
    }
}