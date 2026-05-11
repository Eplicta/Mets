using System.IO;
using System.Threading.Tasks;
using Eplicta.Mets.Entities;

namespace Eplicta.Mets.Console.Commands.Mets;

public class BuildFromStreamConsoleCommand : CreateConsoleCommand
{
    public BuildFromStreamConsoleCommand() : base("stream")
    {
    }

    protected override async Task AddResourceAsync(Builder metsDataBuilder)
    {
        var fileName = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
        var fileInfo = new FileInfo(fileName);

        var stream = await LoadFileIntoMemoryStreamAsync(fileName);
        var streamSource = new StreamFileSource
        {
            Name = fileInfo.Name,
            MimeType = FileExtensions.GetMimeType(fileName),
            Stream = stream
        };
        metsDataBuilder.AddFile(streamSource);
    }

    private async Task<MemoryStream> LoadFileIntoMemoryStreamAsync(string filePath)
    {
        var memoryStream = new MemoryStream();

        await using (var fileStream = new FileStream(
                         filePath,
                         FileMode.Open,
                         FileAccess.Read,
                         FileShare.Read,
                         bufferSize: 81920,
                         useAsync: true))
        {
            await fileStream.CopyToAsync(memoryStream);
        }

        memoryStream.Position = 0; // Reset position to beginning
        return memoryStream;
    }
}