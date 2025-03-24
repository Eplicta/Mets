using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Eplicta.Mets.Entities;
using Tharga.Toolkit.Console.Commands.Base;

namespace Eplicta.Mets.Console.Commands.Mets;

public abstract class CreateConsoleCommand : AsyncActionCommandBase
{
    protected CreateConsoleCommand(string name) : base(name)
    {
    }

    public override async Task InvokeAsync(string[] param)
    {
        var metsDataBuilder = new Builder()
            .SetAgent(new MetsData.AgentData
            {
                Name = "Some Company",
                Note = "http://id.kb.se/organisations/SE0000000000",
                Type = MetsData.EType.Other,
                Role = MetsData.ERole.Archivist
            })
            .SetCompany(new MetsData.CompanyData
            {
                Name = "Eplicta AB",
                Note = "http://id.kb.se/organisations/SE0000000000",
                Role = MetsData.ERole.Editor,
                Type = MetsData.EType.Other
            })
            .SetModsSection(new MetsData.ModsSectionData
            {
                Xmlns = "http://www.w3.org/1999/xlink",
                Identifier = "C5385FBC5FC559E7C43AB6700DB28EF3",
                Url = new Uri("https://some.domain.com"),
                DateIssued = DateTime.Parse("2022-02-03T15:48:04.000+01:00"),
                AccessCondition = "gratis",
                ModsTitle = "Moln- och virtualiseringspecialist",
                Uri = new Uri("https://some.domain.com/"),
                ModsTitleInfo = "https://some.domain.com/",
                Notes =
                [
                    new MetsData.ModsNote
                    {
                        InnerText = "lorem ipsum",
                        Type = MetsData.ENoteType.PostMessage,
                        Href = "file:///text.txt"
                    }
                ],
                Place = new MetsData.PlaceInfo
                {
                    PlaceTerm = "Stockholm"
                },
                Publisher = "James Frih"
            })
            .SetSoftware(new MetsData.SoftwareData
            {
                Name = "Eplicta Aggregator",
                Note = "http://id.kb.se/organisations/SE0000000000",
                Role = MetsData.ERole.Editor,
                Type = MetsData.EType.Other,
                OtherType = MetsData.EOtherType.Software
            })
            .SetMetsHdr(new MetsData.MetsHdrData
            {
                Attributes =
                [
                    new MetsData.MetsHdrAttribute
                    {
                        Name = MetsData.EMetsHdrAttributeName.RecordStatus,
                        Value = "VERSION"
                    }
                ]
            })
            .SetMetsAttributes([
                new MetsData.MetsAttribute
                {
                    Name = MetsData.EMetsAttributeName.Label,
                    Value = "Moln- och virtualiseringspecialist"
                },
                new MetsData.MetsAttribute
                {
                    Name = MetsData.EMetsAttributeName.ObjId,
                    Value = "UUID:test ID"
                }
            ]);

        await AddResourceAsync(metsDataBuilder);

        var metsData = metsDataBuilder.Build();

        var renderer = new Renderer(metsData);

        using var archive = renderer.GetArchiveStream(ArchiveFormat.Zip, null, true, MetsSchema.KB);
        await File.WriteAllBytesAsync("C:\\temp\\mods-archive.zip", archive.ToArray());

        var xmlDocument = renderer.Render();
        if (!Validate(xmlDocument)) return;

        OutputInformation("Done");
    }

    protected abstract Task AddResourceAsync(Builder metsDataBuilder);

    private bool Validate(XmlDocument xmlDocument)
    {
        var sut = new MetsValidator();
        var result = sut.Validate(xmlDocument, ModsVersion.Mods_3_7, MetsSchema.Default)?.ToArray() ?? [];
        if (result.Any())
        {
            foreach (var item in result)
            {
                OutputError(item.Message);
            }

            return false;
        }

        return true;
    }
}