using Tharga.Toolkit.Console.Commands.Base;

namespace Eplicta.Mets.Console.Commands.Html;

public class HtmlConsoleCommands : ContainerCommandBase
{
    public HtmlConsoleCommands() : base("html")
    {
        RegisterCommand<HtmlSampleConsoleCommands>();
    }
}