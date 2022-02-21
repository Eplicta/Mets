using Tharga.Toolkit.Console.Commands.Base;

namespace Eplicta.Mets.Console.Commands.Mets
{
    public class MetsConsoleCommands : ContainerCommandBase
    {
        public MetsConsoleCommands() : base("mets")
        {
            RegisterCommand<CreateConsoleCommand>();
        }
    }
}