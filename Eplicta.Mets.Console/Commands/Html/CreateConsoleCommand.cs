using System.Collections.Generic;
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
                            Value = "aaa_{pathX}_bbb",
                            Attributes = new Dictionary<string, string>
                            {
                                {
                                    "anka", "{pathX}"
                                },
                                {
                                    "korv", "jag"
                                },
                                {
                                    "lamm", "get"
                                }
                            }
                        },
                        new HtmlTemplate.Element
                        {
                           Name = "metadata",
                           Children = new HtmlTemplate.Element[]
                           {
                               new HtmlTemplate.Element
                               {
                                   Name = "epafDocumentId",
                                   Value = "{epafDocumentId}"
                               },
                               new HtmlTemplate.Element
                               {
                                   Name = "externalId",
                                   Value = "113824134542904_117380220853962"
                               },
                               new HtmlTemplate.Element
                               {
                                   Name = "uri",
                                   Value = "https://www.facebook.com/113824134542904/posts/117380220853962/"
                               },
                               new HtmlTemplate.Element
                               {
                                   Name = "publishDate",
                                   Value = "2022-02-07T13:14:22.0000000Z"

                               },
                               new HtmlTemplate.Element
                               {
                                   Name = "title",
                                   Value = "Detta är ett inlägg med bild och location"
                               },
                               new HtmlTemplate.Element
                               {
                                   Name = "postMessage",
                                   Children = new HtmlTemplate.Element[]
                                   {
                                       new HtmlTemplate.Element
                                       {
                                           Name = "![CDATA[ Detta är ett inlägg med bild och location. ]]",
                                       }
                                   }
                               },
                               new HtmlTemplate.Element
                               {
                                   Name = "likeCount",
                                   Value = "1"
                               },
                               new HtmlTemplate.Element
                               {
                                   Name = "shareCount",
                                   Value = "0"
                               },
                               new HtmlTemplate.Element
                               {
                                   Name = "location",
                                   Children = new HtmlTemplate.Element[]
                                   {
                                       new HtmlTemplate.Element
                                       {
                                           Name = "name",
                                           Value = "Stockholm"
                                       },
                                       new HtmlTemplate.Element
                                       {
                                           Name = "link",
                                           Value = "https://www.facebook.com/106505586052951"
                                       }
                                   }
                               }
                            }
                        },
                        new HtmlTemplate.Element
                        {
                            Name = "channelInfo",
                            Children = new HtmlTemplate.Element[]
                            {
                                new HtmlTemplate.Element
                                {
                                    Name = "epafChannelId",
                                    Value = "{epafChannelId}"
                                },
                                new HtmlTemplate.Element
                                {
                                    Name = "ChannelKey",
                                    Value = "Harvester testflöde-113824134542904",
                                    Attributes = new Dictionary<string, string>
                                    {
                                        {
                                            "Ky", "asdsad"
                                        },
                                        {
                                            "Häst", "Korv"
                                        }
                                    }
                                },
                                new HtmlTemplate.Element
                                {
                                    Name = "channelType",
                                    Value = "Facebook"
                                }
                            }
                        },
                        new HtmlTemplate.Element
                        {
                            Name = "resources",
                            Children = new HtmlTemplate.Element[]
                            {
                                new HtmlTemplate.Element
                                {
                                    Name = "file",
                                    Children = new HtmlTemplate.Element[]
                                    {
                                        new HtmlTemplate.Element
                                        {
                                            Name = "attachmentType",
                                            Value = "photo"
                                        }
                                    },
                                    Attributes = new Dictionary<string, string>
                                    {
                                        {
                                            "name","Content/273280339_117380057520645_1825831192817841875_n.jpg"
                                        },
                                        {
                                            "har","quack"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var htmlData = new HtmlData
            {
                //Title = "MyTitle"
                Data = new Dictionary<string, string>
                {
                    {"pathX", "Channel_C18D7E70CD734F8286E2CA6E6490D56E.serjutt" },
                    {"epafChannelId", "hästkorv" },
                    {"epafDocumentId", "EF7279696F91C83870A84C51E2EB48D1"},
                    
                }
            };

            var renderer = new Eplicta.Html.Renderer(template, htmlData);

            //NOTE: This code saves the metadata to the temp-folder.
            var xmlData = renderer.Render();
            await File.WriteAllBytesAsync("C:\\temp\\myPage.html", Encoding.UTF8.GetBytes(xmlData));

            OutputInformation("Done");
        }
    }
}