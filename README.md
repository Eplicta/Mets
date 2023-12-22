# Eplicta Mets

This code helps build and verify packages used for electronic archives. The two standards **Mets** (Metadata Object Description Schema) / **Mets** (Metadata Encoding and Transmission Standard) are used for storing digital documents in electronic archives.

Nuget package `Eplicta.Mets` can be found on [nuget.org](https://www.nuget.org/packages/Eplicta.Mets/).

Basic example on how to create a simple Mets package and have it stored as a zip-file.
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

To validate a Mets document against a specific Mods version schema.
```
var validator = new MetsValidator();
var result = validator.Validate(xmlDocument, ModsVersion.Mods_3_7, MetsSchema.Default)?.ToArray() ?? Array.Empty<XmlValidatorResult>();
var errorMessage = result.FirstOrDefault()?.Message;
```

This component is created by [Eplicta](https://www.eplicta.se) and is licensed under the [MIT License].