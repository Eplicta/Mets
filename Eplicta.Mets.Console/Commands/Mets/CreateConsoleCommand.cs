using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Eplicta.Mets.Entities;
using Tharga.Toolkit.Console.Commands.Base;

namespace Eplicta.Mets.Console.Commands.Mets;

public class CreateConsoleCommand : AsyncActionCommandBase
{
    public CreateConsoleCommand() : base("Create")
    {
    }

    public override async Task InvokeAsync(string[] param)
    {
        var metsData = new ModsData
        {
            Name = new ModsData.NameData
            {
                NamePart = "kommun"
            },
            TitleInfo = new ModsData.TitleInfoData
            {
                Title = "Moln- och virtualiseringspecialist",
                SubTitle = null
            },
            Creator = "Test 2",
            //CreateDate = "2022-02-14",
            Agent = new ModsData.AgentData
            {
                Name = "Some Company",
                Note = "http://id.kb.se/organisations/SE0000000000",
                Type = "ORGANIZATION",
                Role = "ARCHIVIST"
            },
            Company = new ModsData.CompanyData
            {
                Name = "Eplicta AB",
                Note = "http://id.kb.se/organisations/SE0000000000",
                Role = "EDITOR",
                Type = "ORGANISATION"
            },
            Records = new ModsData.AltRecordId
            {
                Type1 = "DELIVERYTYPE",
                InnerText1 = "DEPOSIT",
                Type2 = "DELIVERYSPECIFICATION",
                InnerText2 = "http://www.kb.se/namespace/digark/deliveryspecification/deposit/fgs-publ/v1/",
                Type3 = "SUBMISSIONAGREEMENT",
                InnerText3 = "http://www.kb.se/namespace/digark/submissionagreement/31-KB999-2013"
            },
            Mods = new ModsData.ModsSectionData
            {
                ObjId = "UUID:test ID",
                Xmlns = "http://www.w3.org/1999/xlink",
                Identifier = "C5385FBC5FC559E7C43AB6700DB28EF3",
                Url = "https://www.alingsas.se/utbildning-och-barnomsorg/vuxenutbildning/jag-vill-studera/program-i-alingsas/moln-och-virtualiseringspecialist/",
                DateIssued = DateTime.Parse("2022-02-03T15:48:04.000+01:00"), //TODO: Add as date and output with the correct format.
                AccessCondition = "gratis",
                ModsTitle = "Moln- och virtualiseringspecialist",
                Uri = "https://www.alingsas.se/",
                ModeTitle2 = "https://www.alingsas.se/"
            },
            Software = new ModsData.SoftwareData
            {
                Name = "Eplicta Aggregator",
                Note = "http://id.kb.se/organisations/SE0000000000",
                Role = "EDITOR",
                Type = "OTHER",
                OtherType = "SOFTWARE"
            },
            Files = new ModsData.FileData[]
            {
                new()
                {
                    Id = "ID4d6bdd9068214aa5a57d53bdbe4a9cf3",
                    Use = "Acrobat PDF/X - Portable Document Format - Exchange 1:1999;PRONOM:fmt/144",
                    MimeType = "application/pdf",
                    Size = 1145856,
                    Created = "2022-02-19T16:44:44.000+01:00",
                    CheckSum = "801520fe16da09d1365596dfabb2846b",
                    ChecksumType = "MD5",
                    Ns2Type = "simple",
                    Ns2Href = "file:Content/Moln-och-virtualiseringsspecialist.pdf",
                    LocType = "URL"
                }
            },
            Resources = new ModsData.ResourceData[]
            {
                new()
            }
        };

        var renderer = new Renderer(metsData);

        //NOTE: This code saves the metadata to the temp-folder.
        var xmlData = renderer.Render().OuterXml;
        await File.WriteAllBytesAsync("C:\\temp\\metadata.xml", Encoding.UTF8.GetBytes(xmlData));

        //NOTE: This code craetes a zip-archive with metadata and resource-files amd saves to the temp-folder.
        await using var archive = renderer.GetArchiveStream();
        await File.WriteAllBytesAsync("C:\\temp\\mods-archive.zip", archive.ToArray());

        OutputInformation("Done");
    }
}