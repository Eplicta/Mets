using System;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Tharga.Toolkit.Console.Commands.Base;
using Tharga.Toolkit.Console.Entities;

namespace Eplicta.Mets.Console.Commands.Xml;

public class XmlValidateConsoleCommand : AsyncActionCommandBase
{
    public XmlValidateConsoleCommand()
        : base("Validate")
    {
    }

    public override async Task InvokeAsync(string[] param)
    {
        var xmlPath = QueryParam<string>("Sip Path", param);

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            ValidationType = ValidationType.Schema,
            ValidationFlags =
                XmlSchemaValidationFlags.ProcessSchemaLocation |
                XmlSchemaValidationFlags.ProcessInlineSchema |
                XmlSchemaValidationFlags.ReportValidationWarnings
        };

        // Allow fetching XSDs from the URLs in xsi:schemaLocation
        settings.XmlResolver = new XmlUrlResolver();

        int errors = 0;
        settings.ValidationEventHandler += (sender, e) =>
        {
            errors++;

            var level = ToLevel(e);
            Output($"{e.Severity}: {e.Message}", level);
            if (e.Exception != null)
            {
                Output($"  Line {e.Exception.LineNumber}, Position {e.Exception.LinePosition}", level);
            }
        };

        try
        {
            using var reader = XmlReader.Create(xmlPath, settings);
            while (reader.Read()) { /* just consume */ }

            if (errors == 0)
            {
                OutputInformation("OK: XML is valid against referenced XSDs.");
            }
            else
            {
                OutputError($"NOT OK: {errors} validation issue(s).");
            }
        }
        catch (Exception ex)
        {
            OutputError("FAILED: " + ex.Message);
        }
    }

    private static OutputLevel ToLevel(ValidationEventArgs e)
    {
        OutputLevel level;
        switch (e.Severity)
        {
            case XmlSeverityType.Error:
                level = OutputLevel.Error;
                break;
            case XmlSeverityType.Warning:
                level = OutputLevel.Warning;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return level;
    }
}