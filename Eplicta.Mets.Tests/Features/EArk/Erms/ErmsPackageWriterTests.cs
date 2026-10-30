using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eplicta.Mets.Features.EArk;
using Eplicta.Mets.Features.EArk.Erms;
using Eplicta.Mets.Features.EArk.Premis;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests.Features.EArk.Erms;

public class ErmsPackageWriterTests
{
    private const string Root = "IP_11111111-1111-1111-1111-111111111111";
    private static readonly byte[] Letter = Encoding.UTF8.GetBytes("Anmälan om kränkande behandling");
    private static readonly byte[] CasePdf = Encoding.UTF8.GetBytes("%PDF-1.7 ärendeblad");

    private static string Sha256(byte[] bytes)
    {
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private static EArkStreamedFile Streamed(string fileName, byte[] bytes, bool declared = true)
    {
        return new EArkStreamedFile
        {
            Href = fileName,
            MimeType = "text/plain",
            Created = ErmsSamples.Created,
            Size = declared ? bytes.Length : 0,
            Checksum = declared ? Sha256(bytes) : null,
            Open = _ => Task.FromResult<Stream>(new MemoryStream(bytes))
        };
    }

    private static ErmsLogDocument Logs()
    {
        return new ErmsLogDocument
        {
            Identification = "log-1",
            CreationDate = ErmsSamples.Created,
            CreatorName = "Futuraskolan AB",
            CaseLogs = [new ErmsCaseLog { Id = "2026-0042", Modifications = [new ErmsLogModification { Number = "1", Date = ErmsSamples.Created, Agent = "Henrik", CaseFields = [new ErmsLogField { Name = "Ärendemening", OriginalValue = "A", NewValue = "B" }] }] }]
        };
    }

    private static PremisDocument Premis()
    {
        return new PremisDocument { Files = [new PremisFile { Identifier = PremisIdentifier.Local("file-1"), FormatName = "text/plain", Fixities = [new PremisFixity { Digest = Sha256(Letter) }] }] };
    }

    private static ErmsPackageBuilder Builder()
    {
        return new ErmsPackageBuilder()
            .SetObjId(Root)
            .SetLabel("2026-0042 Kränkande behandling")
            .SetCreateDate(ErmsSamples.Created)
            .SetSoftware("FortDocs", "1.4.140")
            .AddAgent(EArkAgent.Submitter("Futuraskolan AB", identificationCode: "ORG:5560000000"))
            .SetErms(ErmsSamples.Full());
    }

    private static EArkPackage Full()
    {
        return Builder()
            .SetLogs(Logs())
            .SetPreservation(Premis())
            .AddDataFile(Streamed("2026-0042-1_1_Anmalan.txt", Letter))
            .AddDocumentationFile(new EArkFile { Href = "documentation/arendeblad.pdf", MimeType = "application/pdf", Created = ErmsSamples.Created, Content = CasePdf })
            .Build();
    }

    private static async Task<ZipArchive> WriteAsync(EArkPackage package)
    {
        var buffer = new MemoryStream();
        await new ErmsPackageWriter(package).WriteAsync(buffer, CancellationToken.None);
        buffer.Position = 0;
        return new ZipArchive(buffer, ZipArchiveMode.Read);
    }

    private static byte[] Bytes(ZipArchive archive, string path)
    {
        using var stream = archive.GetEntry($"{Root}/{path}")!.Open();
        using var copy = new MemoryStream();
        stream.CopyTo(copy);
        return copy.ToArray();
    }

    private static XmlDocument Xml(ZipArchive archive, string path)
    {
        var document = new XmlDocument();
        document.Load(new MemoryStream(Bytes(archive, path)));
        return document;
    }

    [Fact]
    public async Task A_package_whose_erms_the_schema_rejects_is_not_written_at_all()
    {
        var package = Builder().SetErms(ErmsSamples.Minimal() with { Aggregations = [ErmsSamples.MinimalCaseFile() with { ObjectId = null }] }).Build();
        var buffer = new MemoryStream();

        var refused = await FluentActions.Awaiting(() => new ErmsPackageWriter(package).WriteAsync(buffer, CancellationToken.None)).Should().ThrowAsync<ErmsPackageValidationException>();

        refused.Which.Failures.Should().NotBeEmpty().And.OnlyContain(x => x.StartsWith("representations/rep_1/data/ERMS.xml: ", StringComparison.Ordinal));
        buffer.Length.Should().Be(0);
    }

    [Fact]
    public async Task A_control_character_in_the_label_still_gives_a_well_formed_mets()
    {
        using var archive = await WriteAsync(Builder().SetLabel("2026-0042Kränkande behandling").Build());

        Xml(archive, "METS.xml").DocumentElement!.GetAttribute("LABEL").Should().Be("2026-0042 Kränkande behandling");
    }

    [Fact]
    public async Task The_contact_person_is_an_individual_creator_reached_as_the_note_says()
    {
        using var archive = await WriteAsync(Builder().AddAgent(EArkAgent.ContactPerson("Rita Registrator", "rita@futuraskolan.se")).Build());

        var mets = Xml(archive, "METS.xml");
        var contact = (XmlElement)mets.SelectSingleNode("//m:metsHdr/m:agent[@ROLE='CREATOR' and @TYPE='INDIVIDUAL']", MetsNamespaces(mets));
        contact.Should().NotBeNull();
        contact!["name", EArkConstants.MetsNamespace]!.InnerText.Should().Be("Rita Registrator");
        var note = contact["note", EArkConstants.MetsNamespace]!;
        note.InnerText.Should().Be("rita@futuraskolan.se");
        note.HasAttribute("NOTETYPE", EArkConstants.CsipNamespace).Should().BeFalse();
    }

    private static XmlNamespaceManager MetsNamespaces(XmlDocument document)
    {
        var manager = new XmlNamespaceManager(document.NameTable);
        manager.AddNamespace("m", EArkConstants.MetsNamespace);
        manager.AddNamespace("csip", EArkConstants.CsipNamespace);
        manager.AddNamespace("xlink", EArkConstants.XlinkNamespace);
        return manager;
    }

    [Fact]
    public async Task The_package_holds_the_erms_document_its_logs_its_preservation_metadata_and_the_files()
    {
        using var archive = await WriteAsync(Full());
        var entries = archive.Entries.Select(x => x.FullName).ToArray();

        entries.Should().Contain($"{Root}/METS.xml");
        entries.Should().Contain($"{Root}/representations/rep_1/data/ERMS.xml");
        entries.Should().Contain($"{Root}/representations/rep_1/data/ermsLogs.xml");
        entries.Should().Contain($"{Root}/representations/rep_1/data/2026-0042-1_1_Anmalan.txt");
        entries.Should().Contain($"{Root}/metadata/preservation/premis.xml");
        entries.Should().Contain($"{Root}/documentation/arendeblad.pdf");
    }

    [Fact]
    public async Task Every_schema_the_package_validates_against_travels_inside_it()
    {
        using var archive = await WriteAsync(Full());
        var schemas = archive.Entries.Select(x => x.FullName).Where(x => x.StartsWith($"{Root}/schemas/", StringComparison.Ordinal) && !x.EndsWith('/')).Select(Path.GetFileName);

        schemas.Should().BeEquivalentTo("mets.xsd", "DILCISExtensionMETS.xsd", "DILCISExtensionSIPMETS.xsd", "xlink.xsd", "ERMS_v3.xsd", "erms_v3.sch", "SNAermsLogs.xsd", "premis-v3-0.xsd");
    }

    [Fact]
    public async Task Every_standard_folder_exists_even_when_it_holds_nothing()
    {
        using var archive = await WriteAsync(Full());
        var entries = archive.Entries.Select(x => x.FullName).ToArray();

        foreach (var folder in new[] { "metadata/", "metadata/descriptive/", "metadata/preservation/", "metadata/other/", "representations/", "representations/rep_1/", "representations/rep_1/data/", "schemas/", "documentation/" })
        {
            entries.Should().Contain($"{Root}/{folder}");
        }
    }

    [Fact]
    public async Task The_mets_says_the_content_is_erms_on_the_package_and_on_its_representation()
    {
        using var archive = await WriteAsync(Full());
        var mets = Xml(archive, "METS.xml");
        var namespaces = MetsNamespaces(mets);
        var root = mets.DocumentElement!;
        var representations = (XmlElement)mets.SelectSingleNode("//m:fileGrp[@USE='Representations']", namespaces)!;

        root.GetAttribute("TYPE").Should().Be("Datasets");
        root.GetAttribute("CONTENTINFORMATIONTYPE", EArkConstants.CsipNamespace).Should().Be("OTHER");
        root.GetAttribute("OTHERCONTENTINFORMATIONTYPE", EArkConstants.CsipNamespace).Should().Be("citserms_v3_0");
        representations.GetAttribute("CONTENTINFORMATIONTYPE", EArkConstants.CsipNamespace).Should().Be("OTHER");
        representations.GetAttribute("OTHERCONTENTINFORMATIONTYPE", EArkConstants.CsipNamespace).Should().Be("citserms_v3_0");
    }

    [Fact]
    public async Task The_mets_and_everything_it_describes_validate_against_their_schemas()
    {
        using var archive = await WriteAsync(Full());

        new EArkValidator().Validate(Xml(archive, "METS.xml")).Should().BeEmpty();
        new ErmsValidator().Validate(Xml(archive, "representations/rep_1/data/ERMS.xml")).Should().BeEmpty();
        new ErmsLogValidator().Validate(Xml(archive, "representations/rep_1/data/ermsLogs.xml")).Should().BeEmpty();
        new PremisValidator().Validate(Xml(archive, "metadata/preservation/premis.xml")).Should().BeEmpty();
    }

    [Fact]
    public async Task Every_checksum_in_the_mets_matches_the_bytes_written_beside_it()
    {
        using var archive = await WriteAsync(Full());
        var mets = Xml(archive, "METS.xml");
        var namespaces = MetsNamespaces(mets);

        var described = mets.SelectNodes("//m:file", namespaces)!.Cast<XmlElement>()
            .Select(x => (Href: ((XmlElement)x.SelectSingleNode("m:FLocat", namespaces)!).GetAttribute("href", EArkConstants.XlinkNamespace), Checksum: x.GetAttribute("CHECKSUM")))
            .Concat(mets.SelectNodes("//m:mdRef", namespaces)!.Cast<XmlElement>().Select(x => (Href: x.GetAttribute("href", EArkConstants.XlinkNamespace), Checksum: x.GetAttribute("CHECKSUM"))))
            .ToArray();

        described.Should().NotBeEmpty();
        foreach (var (href, checksum) in described)
        {
            checksum.Should().Be(Sha256(Bytes(archive, href)), href);
        }
    }

    [Fact]
    public async Task A_streamed_file_without_a_declared_checksum_is_measured_from_its_stream()
    {
        var package = Builder().AddDataFile(Streamed("a.txt", Letter, declared: false)).Build();

        using var archive = await WriteAsync(package);
        var mets = Xml(archive, "METS.xml");
        var file = mets.SelectNodes("//m:file", MetsNamespaces(mets))!.Cast<XmlElement>().Single(x => x.InnerXml.Contains("data/a.txt", StringComparison.Ordinal));

        file.GetAttribute("CHECKSUM").Should().Be(Sha256(Letter));
        file.GetAttribute("SIZE").Should().Be(Letter.Length.ToString());
    }

    [Fact]
    public async Task A_streamed_file_whose_bytes_differ_from_its_declared_checksum_stops_the_package()
    {
        var tampered = Streamed("a.txt", Letter) with { Checksum = Sha256(CasePdf) };
        var package = Builder().AddDataFile(tampered).Build();

        var act = () => new ErmsPackageWriter(package).WriteAsync(new MemoryStream(), CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidDataException>()).Which.Message.Should().Contain("representations/rep_1/data/a.txt");
    }

    [Fact]
    public async Task A_described_file_with_no_content_stops_the_package_rather_than_being_left_out()
    {
        var package = Builder().AddDataFile(new EArkFile { Href = "empty.txt", Created = ErmsSamples.Created }).Build();

        var act = () => new ErmsPackageWriter(package).WriteAsync(new MemoryStream(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public void A_package_without_an_erms_document_is_refused()
    {
        var act = () => new ErmsPackageBuilder()
            .SetObjId(Root)
            .SetSoftware("FortDocs", "1.4.140")
            .AddAgent(EArkAgent.Submitter("Futuraskolan AB"))
            .Build();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void The_appendix_path_of_a_data_file_is_relative_to_the_representation()
    {
        ErmsPackageBuilder.AppendixPath("2026-0042-1_1_Anmalan.pdf").Should().Be("data/2026-0042-1_1_Anmalan.pdf");
    }

    [Fact]
    public void The_general_writer_is_left_as_it_was()
    {
        var package = new EArkPackageBuilder()
            .SetObjId("uuid-1")
            .SetSoftware("Eplicta WebConserver", "1.2.3")
            .AddAgent(EArkAgent.Submitter("Sjofartsverket"))
            .SetContentInformationType(EArkPackage.EContentInformationType.OTHER, "Web archive")
            .AddRepresentationFile(new EArkFile { Href = "representations/rep_1/data/webarchive.zip", Created = ErmsSamples.Created, Content = [1, 2, 3] })
            .Build();
        var buffer = new MemoryStream();

        new EArkPackageWriter(package).Write(buffer);

        buffer.Position = 0;
        using var archive = new ZipArchive(buffer, ZipArchiveMode.Read);
        archive.Entries.Should().NotContain(x => x.FullName.EndsWith('/'));
        var mets = new XmlDocument();
        mets.Load(archive.GetEntry("uuid-1/METS.xml")!.Open());
        ((XmlElement)mets.SelectSingleNode("//m:fileGrp[@USE='Representations']", MetsNamespaces(mets))!).HasAttribute("OTHERCONTENTINFORMATIONTYPE", EArkConstants.CsipNamespace).Should().BeFalse();
    }
}
