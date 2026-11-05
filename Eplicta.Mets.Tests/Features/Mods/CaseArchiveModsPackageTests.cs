using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eplicta.Mets.Features.EArk;
using Eplicta.Mets.Features.EArk.Erms;
using Eplicta.Mets.Features.Mods;
using Eplicta.Mets.Tests.Features.EArk.Erms;
using Xunit;

namespace Eplicta.Mets.Tests.Features.Mods;

public class CaseArchiveModsPackageTests
{
    private const string Root = "IP_33333333-3333-3333-3333-333333333333";
    private const string ModsHref = "metadata/descriptive/mods.xml";

    private static string ReadEntry(ZipArchive archive, string path)
    {
        using var stream = archive.GetEntry($"{Root}/{path}")!.Open();
        using var reader = new StreamReader(stream, new UTF8Encoding(false));
        return reader.ReadToEnd();
    }

    [Fact]
    public async Task CaseArchivePackage_ShouldCarryModsWithCaseArchiveGenreThatReadsBack()
    {
        var mods = new ModsDocument { Identifier = "case-2026-0042", Title = "2026-0042", Genres = [EplictaPackageTypes.CaseArchive], DateIssued = ErmsSamples.Created };
        var package = new ErmsPackageBuilder()
            .SetObjId(Root)
            .SetLabel("2026-0042")
            .SetCreateDate(ErmsSamples.Created)
            .SetSoftware("FortDocs", "1.4.140")
            .AddAgent(EArkAgent.Submitter("Futuraskolan AB"))
            .SetErms(ErmsSamples.Minimal())
            .AddDescriptiveMetadata(new EArkMetadataReference
            {
                Href = ModsHref,
                MdType = EArkMetadataReference.EMdType.MODS,
                MimeType = "text/xml",
                Created = ErmsSamples.Created,
                Content = ModsDocumentRenderer.RenderBytes(mods)
            })
            .Build();
        var buffer = new MemoryStream();

        await new ErmsPackageWriter(package).WriteAsync(buffer, CancellationToken.None);

        buffer.Position = 0;
        using var archive = new ZipArchive(buffer, ZipArchiveMode.Read);
        var reader = new EArkReader();
        var read = reader.Read(ReadEntry(archive, "METS.xml"));
        var reference = Assert.Single(read.DescriptiveMetadata, x => x.MdType == EArkMetadataReference.EMdType.MODS);
        var actual = reader.ReadMods(ReadEntry(archive, reference.Href));
        var genre = Assert.Single(actual.Genres);
        Assert.True(EplictaPackageTypes.Matches(genre.Value, EplictaPackageTypes.CaseArchive));
        Assert.False(EplictaPackageTypes.Matches(genre.Value, EplictaPackageTypes.WebArchive));
        Assert.Equal("case-2026-0042", actual.Identifier.Value);
        Assert.Equal(ModsHref, reference.Href);
    }
}
