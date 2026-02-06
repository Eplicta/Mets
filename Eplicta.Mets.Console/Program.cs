using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Eplicta.Mets;
using Eplicta.Mets.Console.Commands.Html;
using Eplicta.Mets.Console.Commands.Mets;
using Eplicta.Mets.Console.Commands.Xml;
using Microsoft.Extensions.DependencyInjection;
using Tharga.Toolkit.Console;
using Tharga.Toolkit.Console.Commands;
using Tharga.Toolkit.Console.Consoles;
using Tharga.Toolkit.Console.Helpers;
using Tharga.Toolkit.Console.Interfaces;

var services = new ServiceCollection();

services.AddHttpClient();

services.AddEplictaMets();

RegisterCommands(services, Assembly.GetExecutingAssembly());

using var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
{
    ValidateOnBuild = true,
    ValidateScopes = true
});

using var console = new ClientConsole();
var root = new RootCommand(console, new CommandResolver(type => (ICommand)serviceProvider.GetRequiredService(type)));

root.RegisterCommand<MetsConsoleCommands>();
root.RegisterCommand<XmlConsoleCommands>();
root.RegisterCommand<HtmlConsoleCommands>();

var engine = new CommandEngine(root);
engine.Start(args);

static void RegisterCommands(IServiceCollection services, Assembly assembly)
{
    var commandTypes = assembly
        .GetTypes()
        .Where(x => !x.IsAbstract)
        .Where(x => typeof(ICommand).IsAssignableFrom(x));

    foreach (var type in commandTypes)
    {
        Debug.WriteLine($"Registered in IOC: {type.Name}");
        services.AddTransient(type);
    }
}