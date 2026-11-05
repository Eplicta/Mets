using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eplicta.Mets.Features.EArk;
using Eplicta.Mets.Features.EArk.Erms;
using Xunit;

namespace Eplicta.Mets.Tests.Features.EArk.Erms;

public class ErmsPackageBuilderDescriptiveMetadataTests
{
    private const string Root = "IP_22222222-2222-2222-2222-222222222222";
    private const string ModsHref = "metadata/descriptive/mods.xml";
    private static readonly byte[] ModsContent = Encoding.UTF8.GetBytes("<mods:mods xmlns:mods=\"http://www.loc.gov/mods/v3\"><mods:identifier type=\"local\">c-1</mods:identifier></mods:mods>");

    private static EArkMetadataReference ModsReference()
    {
        return new EArkMetadataReference { Href = ModsHref, MdType = EArkMetadataReference.EMdType.MODS, MimeType = "text/xml", Created = ErmsSamples.Created, Content = ModsContent };
    }

    private static ErmsPackageBuilder Builder()
    {
        return new ErmsPackageBuilder().SetObjId(Root).SetLabel("Case").SetCreateDate(ErmsSamples.Created).SetSoftware("FortDocs", "1.4.140").AddAgent(EArkAgent.Submitter("Futuraskolan AB")).SetErms(ErmsSamples.Minimal());
    }

    [Fact]
    public void AddDescriptiveMetadata_ShouldReturnTheBuilder()
    {
        var builder = Builder();

        Assert.Same(builder, builder.AddDescriptiveMetadata(ModsReference()));
    }

    [Fact]
    public void Build_WhenDescriptiveMetadataAdded_ShouldCarryTheModsReference()
    {
        var package = Builder().AddDescriptiveMetadata(ModsReference()).Build();

        var reference = Assert.Single(package.DescriptiveMetadata);
        Assert.Equal(EArkMetadataReference.EMdType.MODS, reference.MdType);
        Assert.Equal(ModsHref, reference.Href);
        Assert.Equal(ModsContent, reference.Content);
    }

    [Fact]
    public void Build_WhenNoDescriptiveMetadataAdded_ShouldHaveNone()
    {
        var package = Builder().Build();

        Assert.Empty(package.DescriptiveMetadata);
    }

    [Fact]
    public void Build_WhenDescriptiveMetadataAdded_ShouldStillIdentifyThePackageAsErms()
    {
        var package = Builder().AddDescriptiveMetadata(ModsReference()).Build();

        Assert.Equal(EArkConstants.ContentCategory.Datasets, package.ContentCategory);
        Assert.Equal(EArkPackage.EContentInformationType.OTHER, package.ContentInformationType);
        Assert.Equal(ErmsConstants.ContentInformationType, package.OtherContentInformationType);
        Assert.Equal("citserms_v3_0", package.OtherContentInformationType);
        Assert.EndsWith(ErmsConstants.FileName, package.RepresentationFiles.First().Href);
    }

    [Fact]
    public void Build_WhenDescriptiveMetadataAdded_ShouldNotChangeTheRepresentationFiles()
    {
        var without = Builder().Build();
        var with = Builder().AddDescriptiveMetadata(ModsReference()).Build();

        Assert.Equal(without.RepresentationFiles.Select(x => x.Href), with.RepresentationFiles.Select(x => x.Href));
        Assert.Equal(without.SchemaFiles.Select(x => x.Href), with.SchemaFiles.Select(x => x.Href));
    }

    [Fact]
    public async Task WriteAsync_WhenDescriptiveMetadataAdded_ShouldWriteModsUnderTheRootFolderAndNotThrow()
    {
        var package = Builder().AddDescriptiveMetadata(ModsReference()).Build();
        var buffer = new MemoryStream();

        await new ErmsPackageWriter(package).WriteAsync(buffer, CancellationToken.None);

        buffer.Position = 0;
        using var archive = new ZipArchive(buffer, ZipArchiveMode.Read);
        var entry = archive.GetEntry($"{Root}/{ModsHref}");
        Assert.NotNull(entry);
        using var stream = entry.Open();
        using var copy = new MemoryStream();
        await stream.CopyToAsync(copy);
        Assert.Equal(ModsContent, copy.ToArray());
    }

    [Fact]
    public async Task WriteAsync_WhenNoDescriptiveMetadata_ShouldWriteNoMods()
    {
        var buffer = new MemoryStream();

        await new ErmsPackageWriter(Builder().Build()).WriteAsync(buffer, CancellationToken.None);

        buffer.Position = 0;
        using var archive = new ZipArchive(buffer, ZipArchiveMode.Read);
        Assert.Null(archive.GetEntry($"{Root}/{ModsHref}"));
    }
}
