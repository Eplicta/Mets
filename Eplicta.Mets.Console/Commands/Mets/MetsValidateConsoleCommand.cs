using System.Threading.Tasks;
using System.Xml;
using Tharga.Toolkit.Console.Commands.Base;

namespace Eplicta.Mets.Console.Commands.Mets;

public class MetsValidateConsoleCommand : AsyncActionCommandBase
{
    public MetsValidateConsoleCommand()
        : base("Validate")
    {
    }

    public override async Task InvokeAsync(string[] param)
    {
        var xmlPath = QueryParam<string>("Sip Path", param);

        var doc = new XmlDocument();
        doc.Load(xmlPath);

        var validator = new XmlValidator();
        var result = validator.Validate(doc);
        foreach (var item in result)
        {
            OutputInformation($"{item.XmlSeverityType} {item.Message} ({item.XmlSchemaException.Message})");
        }
    }
}