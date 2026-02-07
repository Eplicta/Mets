using Eplicta.Mets.Entities;
using System.IO;
using System.Threading.Tasks;
using Tharga.Toolkit.Console.Commands.Base;

namespace Eplicta.Mets.Console.Commands.Mets;

public class BuildBasicCommand : AsyncActionCommandBase
{
    private readonly IMetsValidatorService _metsValidatorService;

    public BuildBasicCommand(IMetsValidatorService metsValidatorService) : base("basic")
    {
        _metsValidatorService = metsValidatorService;
    }

    public override async Task InvokeAsync(string[] param)
    {
        //var schema = QueryParam("Schema", param, MetsSchema.All().ToDictionary(x => x, x => x.Name));
        var schema = MetsSchema.Riksarkivet;

        var metsData = new Builder()
            .SetMetsAttributes([
                new MetsData.MetsAttribute
                {
                    Name = MetsData.EMetsAttributeName.ObjId,
                    Value = "UUID:test ID"
                }
            ])
            .Build();
        var renderer = new Renderer(metsData);

        var xmlDocument = renderer.Render(null, schema);
        var rows = _metsValidatorService.Validate(xmlDocument);
        foreach (var row in rows)
        {
            Output(row.ToMessage(), row.ToLevel());
        }

        using var archive = renderer.GetArchiveStream(ArchiveFormat.Zip, null, true, MetsSchema.Default);
        await File.WriteAllBytesAsync("C:\\temp\\mets-archive.zip", archive.ToArray());
    }
}