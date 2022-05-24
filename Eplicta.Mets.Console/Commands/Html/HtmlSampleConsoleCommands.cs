using Tharga.Toolkit.Console.Commands.Base;

namespace Eplicta.Mets.Console.Commands.Html;

public class HtmlSampleConsoleCommands : ContainerCommandBase
{
    public HtmlSampleConsoleCommands() : base("sample")
    {
        RegisterCommand<CreateConsoleCommand>();
    }
}