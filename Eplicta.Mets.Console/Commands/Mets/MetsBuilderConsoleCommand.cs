using Tharga.Toolkit.Console.Commands.Base;

namespace Eplicta.Mets.Console.Commands.Mets;

public class MetsBuilderConsoleCommand : ContainerCommandBase
{
    public MetsBuilderConsoleCommand() : base("build")
    {
        RegisterCommand<BuildBasicCommand>();
        RegisterCommand<BuildFromStreamConsoleCommand>();
    }
}