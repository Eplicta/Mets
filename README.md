# Eplicta Mets
[![NuGet](https://img.shields.io/nuget/v/Eplicta.Mets)](https://www.nuget.org/packages/Eplicta.Mets)
![Nuget](https://img.shields.io/nuget/dt/Eplicta.Mets)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![GitHub repo Issues](https://img.shields.io/github/issues/Eplicta/Mets?style=flat&logo=github&logoColor=red&label=Issues)](https://github.com/Eplicta/Mets/issues?q=is%3Aopen)

This code helps build and verify packages used for electronic archives. 
The two standards **Mets** (Metadata Encoding and Transmission Standard) and **Mods** (Metadata Object Description Schema) are used for storing digital documents in electronic archives.

## How to get started
Register the services using *AddEplictaMets*.

```
var builder = WebApplication.CreateBuilder(args);

//...

builder.Services.AddEplictaMets();

var app = builder.Build();

//...

app.Run();

```

## Builder
```
var metsData = new Builder()
    .SetMetsAttributes(new []
    {
        new MetsData.MetsAttribute
        {
            Name = MetsData.EMetsAttributeName.ObjId,
            Value = "UUID:test ID"
        }
    })
    .Build();
var renderer = new Renderer(metsData);
var xmlDocument = renderer.Render();

await using var archive = renderer.GetArchiveStream(ArchiveFormat.Zip, null, true, MetsSchema.Default);
await File.WriteAllBytesAsync("C:\\mets-archive.zip", archive.ToArray());
```

## Validator
Inject *IMetsValidatorService* and use it

```
var doc = new XmlDocument();
doc.Load("C:\\file.xml");

var results = _validatorService.Validate(doc);
foreach (var item in results)
{
    Console.WriteLine(item.Information ?? item.XmlReslut.Message);
}
```

## E-ARK

Alongside the METS/MODS renderer above, this package builds **E-ARK** information packages — CSIP and SIP
version 2.1.0, which together make up Riksarkivet's *FGS Paketstruktur 2.0*. This is a separate renderer, not
a profile of the one above: an E-ARK package has a fixed folder structure, references its metadata instead of
embedding MODS, and requires an `amdSec` and a `structMap` labelled `CSIP`.

```
var package = new EArkPackageBuilder()
    .SetObjId($"uuid-{Guid.NewGuid()}")
    .SetLabel("www.example.se")
    .SetContentCategory(EArkConstants.ContentCategory.WebArchives)
    .SetContentInformationType(EArkPackage.EContentInformationType.OTHER, "Web archive")
    .SetSoftware("My Tool", "1.0.0")
    .AddAgent(EArkAgent.Submitter("My Organisation", identificationCode: "ORG:5560000000"))
    .AddDescriptiveMetadata(new EArkMetadataReference { Href = "metadata/descriptive/mods.xml", MdType = EArkMetadataReference.EMdType.MODS, Content = modsBytes })
    .AddRepresentationFile(new EArkFile { Href = "representations/rep_1/data/webarchive.zip", MimeType = "application/zip", SourcePath = @"C:\temp\webarchive.zip" })
    .Build();

await using var target = File.Create(@"C:\temp\delivery.zip");
new EArkPackageWriter(package).Write(target);
```

The writer computes sizes and SHA-256 checksums, carries the schemas it validates against inside the package,
and lays everything out under a single root folder as `CSIPSTR1` requires. A large payload is streamed from
`SourcePath` rather than held in memory.

### Riksarkivet's adaptation

Deliveries **to Riksarkivet** must additionally carry `documentation/additionalPackageInfo.xml`. Supply
`AdditionalPackageInfo` and the writer renders it, describes it, and carries its schema:

```
builder.SetAdditionalPackageInfo(new AdditionalPackageInfo
{
    CreationDate = DateTime.UtcNow,
    CreatorName = "My Organisation",
    CreatorCode = "5560000000",
    ArchiveName = "Webbarkiv 2026"
});
```

It is off by default, since plain E-ARK is what any other recipient expects.

### Validating

`EArkValidator` and `AdditionalPackageInfoValidator` validate offline against the embedded schemas:

```
var results = new EArkValidator().Validate(xmlDocument);
```

This component is created by [Eplicta](https://www.eplicta.se) and is licensed under the [MIT License](LICENSE).
