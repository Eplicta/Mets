using Tharga.Toolkit.Console.Commands.Base;

namespace Eplicta.Mets.Console.Commands.Xml;

public class XmlConsoleCommands : ContainerCommandBase
{
    public XmlConsoleCommands()
        : base("xml")
    {
        RegisterCommand<XmlValidateConsoleCommand>();
    }
}