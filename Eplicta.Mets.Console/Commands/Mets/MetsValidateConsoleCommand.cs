using System;
using System.Threading.Tasks;
using System.Xml;
using Tharga.Toolkit.Console.Commands.Base;
using Tharga.Toolkit.Console.Entities;

namespace Eplicta.Mets.Console.Commands.Mets;

public class MetsValidateConsoleCommand : AsyncActionCommandBase
{
    private readonly IValidatorService _validatorService;

    public MetsValidateConsoleCommand(IValidatorService validatorService)
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
            Output(item.Information ?? $"{item.XmlReslut.Message} (Line: {item.XmlReslut.XmlSchemaException.LineNumber}, Position: {item.XmlReslut.XmlSchemaException.LinePosition})", ToLevel(item));
        }
    }

    private static OutputLevel ToLevel(ValidatorResult item)
    {
        OutputLevel level;
        switch (item.Severity)
        {
            case SeverityType.Error:
                level = OutputLevel.Error;
                break;
            case SeverityType.Warning:
                level = OutputLevel.Warning;
                break;
            case SeverityType.Information:
                level = OutputLevel.Information;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return level;
    }
}