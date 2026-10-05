using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Eplicta.Mets.Features.EArk;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests.Features.EArk;

public class EArkPackageWriterTests : IDisposable
{
    private static readonly DateTime Created = new(2026, 3, 14, 9, 30, 0, DateTimeKind.Utc);
    private readonly string _scratch = Path.Combine(Path.GetTempPath(), $"eark-{Guid.NewGuid():N}");

    public EArkPackageWriterTests()
    {
        Directory.CreateDirectory(_scratch);
    }

    public void Dispose()
    {
        if (Directory.Exists(_scratch)) Directory.Delete(_scratch, true);
    }

    private static EArkPackageBuilder Builder()
    {
        return new EArkPackageBuilder()
            .SetObjId("uuid-11111111-1111-1111-1111-111111111111")
            .SetLabel("www.sjofartsverket.se")
            .SetCreateDate(Created)
            .SetSoftware("Eplicta WebConserver", "1.2.3")
            .AddAgent(EArkAgent.Submitter("Sjofartsverket", identificationCode: "ORG:202100005810"));
    }

    private static ZipArchive Write(EArkPackage package)
    {
        var buffer = new MemoryStream();
        new EArkPackageWriter(package).Write(buffer);
        buffer.Position = 0;
        return new ZipArchive(buffer, ZipArchiveMode.Read);
    }

    private static string[] Entries(ZipArchive archive)
    {
        return archive.Entries.Select(x => x.FullName).ToArray();
    }

    private static XmlDocument Mets(ZipArchive archive)
    {
        var entry = archive.Entries.Single(x => x.FullName.EndsWith("/METS.xml", StringComparison.Ordinal));
        using var stream = entry.Open();
        var document = new XmlDocument();
        document.Load(stream);
        return document;
    }

    private static XmlElement FileFor(ZipArchive archive, string hrefSuffix)
    {
        return Mets(archive).GetElementsByTagName("file", EArkConstants.MetsNamespace)
            .Cast<XmlElement>()
            .Single(x => ((XmlElement)x.FirstChild!).GetAttribute("href", EArkConstants.XlinkNamespace).EndsWith(hrefSuffix, StringComparison.Ordinal));
    }

    private static EArkPackage WithWebArchive(EArkPackageBuilder builder, byte[] content)
    {
        return builder.AddRepresentationFile(new EArkFile { Href = "representations/rep_1/data/webarchive.zip", MimeType = "application/zip", Created = Created, Content = content }).Build();
    }

    [Fact]
    public void Everything_unpacks_into_a_single_root_folder_named_after_the_objid()
    {
        using var archive = Write(WithWebArchive(Builder(), [1, 2, 3]));

        Entries(archive).Should().OnlyContain(x => x.StartsWith("uuid-11111111-1111-1111-1111-111111111111/", StringComparison.Ordinal));
    }

    [Fact]
    public void The_mets_document_sits_at_the_root_of_that_folder()
    {
        using var archive = Write(WithWebArchive(Builder(), [1, 2, 3]));

        Entries(archive).Should().Contain("uuid-11111111-1111-1111-1111-111111111111/METS.xml");
    }

    [Fact]
    public void The_schemas_the_package_validates_against_travel_inside_it()
    {
        using var archive = Write(WithWebArchive(Builder(), [1, 2, 3]));
        var entries = Entries(archive);

        entries.Should().Contain("uuid-11111111-1111-1111-1111-111111111111/schemas/mets.xsd");
        entries.Should().Contain("uuid-11111111-1111-1111-1111-111111111111/schemas/DILCISExtensionMETS.xsd");
        entries.Should().Contain("uuid-11111111-1111-1111-1111-111111111111/schemas/DILCISExtensionSIPMETS.xsd");
        entries.Should().Contain("uuid-11111111-1111-1111-1111-111111111111/schemas/xlink.xsd");
    }

    [Fact]
    public void The_representation_payload_is_written_where_the_mets_says_it_is()
    {
        using var archive = Write(WithWebArchive(Builder(), [1, 2, 3]));

        Entries(archive).Should().Contain("uuid-11111111-1111-1111-1111-111111111111/representations/rep_1/data/webarchive.zip");
    }

    [Fact]
    public void Descriptive_metadata_is_written_beside_the_mets_document()
    {
        var package = Builder()
            .AddDescriptiveMetadata(new EArkMetadataReference { Href = "metadata/descriptive/mods.xml", MdType = EArkMetadataReference.EMdType.MODS, Created = Created, Content = Encoding.UTF8.GetBytes("<mods/>") })
            .AddRepresentationFile(new EArkFile { Href = "representations/rep_1/data/a.zip", Created = Created, Content = [1] })
            .Build();

        using var archive = Write(package);

        Entries(archive).Should().Contain("uuid-11111111-1111-1111-1111-111111111111/metadata/descriptive/mods.xml");
    }

    [Fact]
    public void Documentation_is_written_where_the_mets_says_it_is()
    {
        var package = Builder()
            .AddDocumentationFile(new EArkFile { Href = "documentation/metadatabilaga.xlsx", Created = Created, Content = [9, 9] })
            .AddRepresentationFile(new EArkFile { Href = "representations/rep_1/data/a.zip", Created = Created, Content = [1] })
            .Build();

        using var archive = Write(package);

        Entries(archive).Should().Contain("uuid-11111111-1111-1111-1111-111111111111/documentation/metadatabilaga.xlsx");
    }

    [Fact]
    public void Every_entry_is_separated_by_forward_slashes()
    {
        using var archive = Write(WithWebArchive(Builder(), [1, 2, 3]));

        Entries(archive).Should().OnlyContain(x => !x.Contains('\\'));
    }

    [Fact]
    public void A_size_and_checksum_are_computed_for_content_that_carries_none()
    {
        var content = new byte[] { 1, 2, 3, 4, 5 };
        var expected = Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant();

        using var archive = Write(WithWebArchive(Builder(), content));
        var file = FileFor(archive, "webarchive.zip");

        file.GetAttribute("SIZE").Should().Be("5");
        file.GetAttribute("CHECKSUM").Should().Be(expected);
        file.GetAttribute("CHECKSUMTYPE").Should().Be("SHA-256");
    }

    [Fact]
    public void A_checksum_the_caller_supplied_is_not_recomputed()
    {
        var package = Builder()
            .AddRepresentationFile(new EArkFile { Href = "representations/rep_1/data/a.zip", Created = Created, Content = [1, 2, 3], Checksum = "suppliedchecksum", Size = 99 })
            .Build();

        using var archive = Write(package);
        var file = FileFor(archive, "data/a.zip");

        file.GetAttribute("CHECKSUM").Should().Be("suppliedchecksum");
        file.GetAttribute("SIZE").Should().Be("99");
    }

    [Fact]
    public void Content_is_streamed_from_disk_when_a_source_path_is_given()
    {
        var path = Path.Combine(_scratch, "webarchive.zip");
        var content = new byte[] { 7, 7, 7, 7 };
        File.WriteAllBytes(path, content);

        var package = Builder()
            .AddRepresentationFile(new EArkFile { Href = "representations/rep_1/data/webarchive.zip", Created = Created, SourcePath = path })
            .Build();

        using var archive = Write(package);
        var entry = archive.Entries.Single(x => x.FullName.EndsWith("webarchive.zip", StringComparison.Ordinal));

        using var stream = entry.Open();
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        buffer.ToArray().Should().Equal(content);
    }

    [Fact]
    public void The_checksum_of_a_file_on_disk_matches_its_bytes()
    {
        var path = Path.Combine(_scratch, "webarchive.zip");
        var content = new byte[] { 7, 7, 7, 7 };
        File.WriteAllBytes(path, content);
        var expected = Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant();

        var package = Builder()
            .AddRepresentationFile(new EArkFile { Href = "representations/rep_1/data/webarchive.zip", Created = Created, SourcePath = path })
            .Build();

        using var archive = Write(package);
        var file = FileFor(archive, "data/webarchive.zip");

        file.GetAttribute("CHECKSUM").Should().Be(expected);
        file.GetAttribute("SIZE").Should().Be("4");
    }

    [Fact]
    public void The_mets_document_inside_the_written_package_validates()
    {
        var package = Builder()
            .AddDescriptiveMetadata(new EArkMetadataReference { Href = "metadata/descriptive/mods.xml", MdType = EArkMetadataReference.EMdType.MODS, Created = Created, Content = Encoding.UTF8.GetBytes("<mods/>") })
            .AddDocumentationFile(new EArkFile { Href = "documentation/metadatabilaga.xlsx", Created = Created, Content = [9] })
            .AddRepresentationFile(new EArkFile { Href = "representations/rep_1/data/webarchive.zip", Created = Created, Content = [1, 2, 3] })
            .Build();

        using var archive = Write(package);

        new EArkValidator().Validate(Mets(archive)).Should().BeEmpty();
    }

    [Fact]
    public void The_schemas_carried_in_the_package_are_described_in_the_mets_document()
    {
        using var archive = Write(WithWebArchive(Builder(), [1, 2, 3]));
        var hrefs = Mets(archive).GetElementsByTagName("FLocat", EArkConstants.MetsNamespace)
            .Cast<XmlElement>()
            .Select(x => x.GetAttribute("href", EArkConstants.XlinkNamespace))
            .ToArray();

        hrefs.Should().Contain("schemas/mets.xsd");
        hrefs.Should().Contain("schemas/DILCISExtensionMETS.xsd");
    }

    [Fact]
    public void The_root_folder_name_can_be_set_when_the_objid_is_not_a_legal_folder_name()
    {
        var package = Builder().SetObjId("urn:uuid:1111/2222").AddRepresentationFile(new EArkFile { Href = "representations/rep_1/data/a.zip", Created = Created, Content = [1] }).Build();

        var buffer = new MemoryStream();
        new EArkPackageWriter(package) { RootFolderName = "delivery-001" }.Write(buffer);
        buffer.Position = 0;
        using var archive = new ZipArchive(buffer, ZipArchiveMode.Read);

        Entries(archive).Should().OnlyContain(x => x.StartsWith("delivery-001/", StringComparison.Ordinal));
    }
}
