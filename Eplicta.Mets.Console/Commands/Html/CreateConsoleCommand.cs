using System.IO;
using System.Text;
using System.Threading.Tasks;
using Eplicta.Html;
using Eplicta.Html.Entities;
using Tharga.Toolkit.Console.Commands.Base;

namespace Eplicta.Mets.Console.Commands.Html
{
    public class CreateConsoleCommand : AsyncActionCommandBase
    {
        public CreateConsoleCommand() : base("create")
        {
        }

        public override async Task InvokeAsync(string[] param)
        {
            var template = new HtmlTemplate
            {
                Root = new HtmlTemplate.Element
                {
                    Name = "document",
                    Children = new[]
                    {
                        new HtmlTemplate.Element
                        {
                            Name = "sourceReference",
                            Value = "Channel_C18D7E70CD734F8286E2CA6E6490D56E.zip"
                        },
                        new HtmlTemplate.Element
                        {
                           Name = "metadata",
                           Children = new HtmlTemplate.Element[]
                           {
                               new HtmlTemplate.Element
                               {
                                   Name = "epafDocumentId",
                                   Value = "EF7279696F91C83870A84C51E2EB48D1"
                               },
                               new HtmlTemplate.Element
                               {
                                   Name = "uri",
                                   Value = "https://www.facebook.com/113824134542904/posts/117380220853962/"
                               },
                           }
                        }
                    }
                }
            };

            var htmlData = new HtmlData
            {
                Title = "MyTitle"
            };

            var renderer = new Eplicta.Html.Renderer(template, htmlData);

            //NOTE: This code saves the metadata to the temp-folder.
            var xmlData = renderer.Render();
            await File.WriteAllBytesAsync("C:\\temp\\myPage.html", Encoding.UTF8.GetBytes(xmlData));

            OutputInformation("Done");
        }
    }
}