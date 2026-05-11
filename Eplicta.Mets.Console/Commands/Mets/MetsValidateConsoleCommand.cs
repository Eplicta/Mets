using System.Threading.Tasks;
using System.Xml;
using Tharga.Toolkit.Console.Commands.Base;

namespace Eplicta.Mets.Console.Commands.Mets;

public class MetsValidateConsoleCommand : AsyncActionCommandBase
{
    private readonly IMetsValidatorService _validatorService;

    public MetsValidateConsoleCommand(IMetsValidatorService validatorService)
        : base("Validate")
    {
        _validatorService = validatorService;
    }

    public override async Task InvokeAsync(string[] param)
    {
        var xmlPath = QueryParam<string>("Sip Path", param);

        var doc = new XmlDocument();
        doc.Load(xmlPath);

        var results = _validatorService.Validate(doc);
        foreach (var item in results)
        {
            Output(item.ToMessage(), item.ToLevel());
        }
    }
}