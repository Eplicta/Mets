using System;
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
                    Name = "head",
                    Children = new[]
                {
                    new HtmlTemplate.Element
                    {
                        Name = "meta",
                        Attributes = new Dictionary<string, string>
                        {
                            {
                                "charset","utf-8"
                            }
                        }
                    },
                    new HtmlTemplate.Element
                    {
                        Name = "title",
                        Value = "{Title} | Facebook" //TODO: Ersätt 'Facebook' med rätt typ.
                    },
                    new HtmlTemplate.Element
                    {
                        Name = "link",
                        Attributes = new Dictionary<string, string>
                        {
                            {
                                "rel", "stylesheet"
                            },
                            {
                                "href","/style.css"
                            },
                            {
                                "media","all"
                            }
                        }
                    },
                    new HtmlTemplate.Element
                    {
                        Name = "body",
                        Attributes = new Dictionary<string, string>
                        {
                            { "class", "facebook post post-image post-location" }
                        },
                        Children = new []
                        {
                        new HtmlTemplate.Element
                        {
                        Name = "div",
                        Attributes = new Dictionary<string, string>
                        {
                            {"class", "post" }
                        },
                        Children = new[]
                        {
                            new HtmlTemplate.Element
                            {
                                Name = "h1",
                                Value = "{h1}"
                            },
                            new HtmlTemplate.Element
                            {
                                 Name = "div",
                                 Attributes = new Dictionary<string, string>
                                 {
                                     {"class", "feed" }
                                 },
                                 Children = new []
                                 {
                                     new HtmlTemplate.Element
                                     {
                                         Name = "a",
                                         Attributes = new Dictionary<string, string>
                                         {
                                             { "href","{ahrefp}" }
                                         },
                                         Children = new []
                                         {
                                             new HtmlTemplate.Element
                                             {
                                                 Name = "p",
                                                 Value = "{creator}"
                                             }
                                         }
                                     }
                                 }

                            },
                            new HtmlTemplate.Element
                            {
                                Name = "div",
                                Attributes = new Dictionary<string, string>
                                {
                                    {
                                        "Class","location"
                                    }
                                },
                                Children = new []
                                {
                                    new HtmlTemplate.Element
                                    {
                                        Name = "a",
                                        Attributes = new Dictionary<string, string>
                                        {
                                            {
                                                "href","{href}"
                                            }
                                        },
                                        Children = new []
                                        {
                                            new HtmlTemplate.Element
                                            {
                                                Name = "p",
                                                Value = "{pcity}"
                                            }
                                        }
                                    }


                                }
                            },
                            new HtmlTemplate.Element
                            {
                                Name ="div",
                                Attributes= new Dictionary<string, string>
                                {
                                    {
                                        "class", "time"
                                    }
                                },
                                Children= new []
                                {
                                    new HtmlTemplate.Element
                                    {
                                        Name ="p",
                                        Value = "{p}"
                                    }
                                }
                            },
                            new HtmlTemplate.Element
                            {
                                Name = "div",
                                Attributes = new Dictionary<string, string>
                                {
                                    {
                                        "class", "content-text"
                                    }
                                },
                                Value = "{div}"

                            },
                            new HtmlTemplate.Element
                            {
                                Name="div",
                                Attributes = new Dictionary<string, string>
                                {
                                    {
                                        "class","content-image"
                                    }
                                },
                                Children = new []
                                {
                                    new HtmlTemplate.Element
                                    {
                                        Name = "img",
                                        Attributes = new Dictionary<string, string>
                                        {
                                            {
                                                "src", "{src}"
                                            }
                                        }

                                    }
                                }

                            },
                            new HtmlTemplate.Element
                            {
                                Name = "div",
                                Attributes = new Dictionary<string, string>
                                {
                                    {
                                        "class", "reactions"
                                    }
                                },
                                Children = new  []
                                {
                                    new HtmlTemplate.Element
                                    {
                                        Name = "div",
                                        Attributes = new Dictionary<string, string>
                                        {
                                            {
                                                "class", "likecount"
                                            }
                                        },
                                        Children = new[]
                                        {
                                            new HtmlTemplate.Element
                                            {
                                                Name = "p",
                                                Value = "{plikes}"
                                            }
                                        }
                                    },
                                    new HtmlTemplate.Element
                                    {
                                        Name = "div",
                                        Attributes = new Dictionary<string,string>
                                        {
                                            {
                                                "class", "sharecount"
                                            }
                                        },
                                        Children = new []
                                        {
                                            new HtmlTemplate.Element
                                            {
                                                Name = "p",
                                                Value = "{pshare}"
                                            }
                                        }
                                    }
                                }
                            },
                            new HtmlTemplate.Element
                            {
                               Name = "div",
                               Attributes = new Dictionary<string, string>
                               {
                                   {
                                       "class", "source"
                                   }
                               },
                               Children= new []
                               {
                                   new HtmlTemplate.Element
                                   {
                                       Name = "a",
                                       Attributes = new Dictionary<string,string>
                                       {
                                           {
                                               "href","{ahref}"
                                           }
                                       },
                                       Children = new []
                                       {
                                           new HtmlTemplate.Element
                                           {
                                               Name = "p",
                                               Value = "Source"
                                           }
                                       }
                                   }
                               }
                            }


                        }
                    },

                        }
                   } }}
            };

            var htmlData = new HtmlData 
            {
                
                //Title = "MyTitle"
                Data = new Dictionary<string, string>
                {
                    {"pathX", "Channel_C18D7E70CD734F8286E2CA6E6490D56E.serjutt dkfkdf kdf kdsf dsf dsf dsf " },
                    {"epafChannelId", "hästkorv" },
                    {"epafDocumentId", "EF7279696F91C83870A84C51E2EB48D1"},
                    {"anka", "duck" },
                    {"publishDate","HDSFNFDJK"}

                },
                Recourses = new List<Dictionary<string, string>>
                {
                     new Dictionary<string, string>
                {
                    {"src", "Content/e8da2bddd3bc4fe8baac3bddbc4978aa.mp4" },
                    {"content", "movie" }           
                },
                    new Dictionary<string, string>
                {
                    {"src", "ankmåös" },
                    {"content", "attachment content-attachment-docx" }
                },
                    new Dictionary<string, string>
                {
                    {"src", "Zlatmannn" },
                    {"content", "attachment content-attachment-txt" }
                },
                    new Dictionary<string, string>
                {
                    {"src", "hårbomb23" },
                    {"content", "attachment contetn-attachment-jpg" }
                },
                    new Dictionary<string, string>
                {
                    {"src", "Anks" },
                    {"content", "movie" }
                },                    
                    new Dictionary<string, string>
                {
                    {"src", "golazo"},
                    {"content", "attachment content-attachment-pdf"}
                }
                
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