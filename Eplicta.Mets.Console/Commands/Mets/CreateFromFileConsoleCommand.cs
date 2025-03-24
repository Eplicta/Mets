using System.Threading.Tasks;

namespace Eplicta.Mets.Console.Commands.Mets;

public class CreateFromFileConsoleCommand : CreateConsoleCommand
{
    public CreateFromFileConsoleCommand() : base("File")
    {
    }

    protected override async Task AddResourceAsync(Builder metsDataBuilder)
    {
        metsDataBuilder.AddFile(System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName);
    }
}