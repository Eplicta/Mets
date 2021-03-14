using System;
using Tharga.Toolkit.Console.Commands;
using Tharga.Toolkit.Console.Consoles;

namespace Eplicta.Mets.Console
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            using var console = new ClientConsole();
            var command = new RootCommand(console);
            var engine = new Tharga.Toolkit.Console.CommandEngine(command);
            engine.Start(args);
        }
    }
}