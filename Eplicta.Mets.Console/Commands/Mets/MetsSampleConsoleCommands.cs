using Tharga.Toolkit.Console.Commands.Base;

namespace Eplicta.Mets.Console.Commands.Mets;

public class MetsSampleConsoleCommands : ContainerCommandBase
{
    public MetsSampleConsoleCommands() : base("sample")
    {
        RegisterCommand<CreateFromFileConsoleCommand>();
        RegisterCommand<CreateFromStreamConsoleCommand>();
    }
}