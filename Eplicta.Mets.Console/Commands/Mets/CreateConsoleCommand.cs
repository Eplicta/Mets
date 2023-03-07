using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Eplicta.Mets.Entities;
using Tharga.Toolkit.Console.Commands.Base;
using Version = Eplicta.Mets.Entities.Version;

namespace Eplicta.Mets.Console.Commands.Mets;

public class CreateConsoleCommand : AsyncActionCommandBase
{
    public CreateConsoleCommand() : base("Create")
    {
    }

    public override async Task InvokeAsync(string[] param)
    {
        var metsData = new Builder()
            .SetAgent(new ModsData.AgentData
            {
                Name = "Some Company",
                Note = "http://id.kb.se/organisations/SE0000000000",
                Type = ModsData.EType.Other,
                Role = ModsData.ERole.Archivist
            })
            .SetCompany(new ModsData.CompanyData
            {
                Name = "Eplicta AB",
                Note = "http://id.kb.se/organisations/SE0000000000",
                Role = ModsData.ERole.Editor,
                Type = ModsData.EType.Other
            })
            .AddAltRecord(new ModsData.AltRecord
            {
                Type = ModsData.EAltRecordType.DeliveryType,
                InnerText = "DEPOSIT"
            })
            .AddAltRecord(new ModsData.AltRecord
            {
                Type = ModsData.EAltRecordType.DeliverySpecification,
                InnerText = "http://www.kb.se/namespace/digark/deliveryspecification/deposit/fgs-publ/v1/"
            })
            .AddAltRecord(new ModsData.AltRecord
            {
                Type = ModsData.EAltRecordType.SubmissionAgreement,
                InnerText = "http://www.kb.se/namespace/digark/submissionagreement/31-KB999-2013"
            })
            .SetModsSection(new ModsData.ModsSectionData
            {
                Xmlns = "http://www.w3.org/1999/xlink",
                Identifier = "C5385FBC5FC559E7C43AB6700DB28EF3",
                Url = new Uri("https://some.domain.com"),
                DateIssued = DateTime.Parse("2022-02-03T15:48:04.000+01:00"),
                AccessCondition = "gratis",
                ModsTitle = "Moln- och virtualiseringspecialist",
                Uri = new Uri("https://some.domain.com/"),
                ModsTitleInfo = "https://some.domain.com/",
                Notes = new[]
                {
                    new ModsData.ModsNote
                    {
                        InnerText = "lorem ipsum",
                        Type = ModsData.ENoteType.PostMessage
                    }
                },
                Place = new ModsData.PlaceInfo
                {
                    PlaceTerm = "Stockholm"
                }
            })
            .SetSoftware(new ModsData.SoftwareData
            {
                Name = "Eplicta Aggregator",
                Note = "http://id.kb.se/organisations/SE0000000000",
                Role = ModsData.ERole.Editor,
                Type = ModsData.EType.Other,
                OtherType = ModsData.EOtherType.Software
            })
            .SetMetsHdr(new ModsData.MetsHdrData
            {
                Attributes = new[]
                {
                    new ModsData.MetsHdrAttribute
                    {
                        Name = ModsData.EMetsHdrAttributeName.RecordStatus,
                        Value = "VERSION"
                    }
                }
            })
            .SetMetsAttributes(new []
            {
                new ModsData.MetsAttribute
                {
                    Name = ModsData.EMetsAttributeName.Label,
                    Value = "Moln- och virtualiseringspecialist"
                },
                new ModsData.MetsAttribute
                {
                    Name = ModsData.EMetsAttributeName.ObjId,
                    Value = "UUID:test ID"
                }
            })
            .AddFile(System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName)
            .Build();

        var renderer = new Renderer(metsData);

        var xmlDocument = renderer.Render();

        if (!Validate(xmlDocument)) return;

        //NOTE: This code saves the metadata to the temp-folder.
        //var xmlData = xmlDocument.OuterXml;
        //await File.WriteAllBytesAsync("C:\\temp\\metadata.xml", Encoding.UTF8.GetBytes(xmlData));

        //NOTE: This code craetes a zip-archive with metadata and resource-files amd saves to the temp-folder.
        await using var archive = renderer.GetArchiveStream(ArchiveFormat.Zip);
        await File.WriteAllBytesAsync("C:\\temp\\mods-archive.zip", archive.ToArray());

        OutputInformation("Done");
    }

    private bool Validate(XmlDocument xmlDocument)
    {
        var sut = new MetsValidator();
        //var schema = Helpers.Resource.GetXml("MODS_enligt_FGS-PUBL_xml1_0.xsd");
        var result = sut.Validate(xmlDocument, Version.ModsFgsPubl_1_0)?.ToArray() ?? Array.Empty<XmlValidatorResult>();
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