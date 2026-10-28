using System;
using System.IO;
using System.Linq;
using System.Xml;
using Eplicta.Mets.Entities;
using Eplicta.Mets.Features.EArk;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Xunit;

namespace Eplicta.Mets.Tests.Features.EArk;

public class EArkReaderTests
{
    private const string DemoObjId = "uuid-7c9f2e14-5a3b-4d82-9f61-0b4e8a1c3d57";
    private static readonly DateTime Created = new(2026, 3, 14, 9, 30, 0, DateTimeKind.Utc);
    private static readonly DateTime Modified = new(2026, 3, 15, 10, 45, 30, DateTimeKind.Utc);

    private static EArkPackage MinimalPackage(Action<EArkPackageBuilder> configure = null)
    {
        var builder = new EArkPackageBuilder()
            .SetObjId("uuid-11111111-1111-1111-1111-111111111111")
            .SetLabel("www.sjofartsverket.se")
            .SetCreateDate(Created)
            .SetSoftware("Eplicta WebConserver", "1.2.3")
            .AddAgent(EArkAgent.Submitter("Sjofartsverket", identificationCode: "ORG:202100005810"))
            .AddRepresentationFile(new EArkFile { Id = "uuid-rep", Href = "representations/rep_1/data/webarchive.zip", MimeType = "application/zip", Size = 20, Created = Created, Checksum = "bb" });

        configure?.Invoke(builder);
        return builder.Build();
    }

    private static EArkMetadataReference Reference(string id, string href, EArkMetadataReference.EMdType mdType, long size = 100, string checksum = "cc")
    {
        return new EArkMetadataReference { Id = id, Href = href, MdType = mdType, MimeType = "text/xml", Size = size, Created = Created, Checksum = checksum };
    }

    private static EArkPackage Render_and_read(EArkPackage package)
    {
        var xml = new EArkRenderer(package).Render().OuterXml;
        return new EArkReader().Read(xml);
    }

    // What a reader can reproduce: everything except the file contents, source paths and additional package info.
    private static EArkPackage WithoutContent(EArkPackage package)
    {
        return package with
        {
            AdditionalPackageInfo = null,
            DescriptiveMetadata = package.DescriptiveMetadata.Select(x => x with { Content = null }).ToArray(),
            PreservationMetadata = package.PreservationMetadata.Select(x => x with { Content = null }).ToArray(),
            DocumentationFiles = package.DocumentationFiles.Select(x => x with { Content = null, SourcePath = null }).ToArray(),
            SchemaFiles = package.SchemaFiles.Select(x => x with { Content = null, SourcePath = null }).ToArray(),
            RepresentationFiles = package.RepresentationFiles.Select(x => x with { Content = null, SourcePath = null }).ToArray()
        };
    }

    private static void AssertRoundTrip(EArkPackage package)
    {
        var actual = Render_and_read(package);

        actual.Should().BeEquivalentTo(WithoutContent(package), Options);
        actual.CreateDate.Kind.Should().Be(DateTimeKind.Utc);
        actual.AdditionalPackageInfo.Should().BeNull();
    }

    private static EquivalencyOptions<EArkPackage> Options(EquivalencyOptions<EArkPackage> options)
    {
        return options.WithStrictOrdering();
    }

    // ---- Round trips -------------------------------------------------------------------------------------------

    [Fact]
    public void Read_WhenMinimalPackageIsRendered_ShouldReturnTheSamePackage()
    {
        AssertRoundTrip(MinimalPackage());
    }

    [Fact]
    public void Read_WhenPackageHasEadAndPremis_ShouldReturnTheSamePackage()
    {
        var package = MinimalPackage(x => x
            .AddDescriptiveMetadata(Reference("uuid-mods", "metadata/descriptive/mods.xml", EArkMetadataReference.EMdType.MODS, 518, "9f7c"))
            .AddDescriptiveMetadata(Reference("uuid-ead", "metadata/descriptive/ead.xml", EArkMetadataReference.EMdType.EAD, 1369, "b89d"))
            .AddPreservationMetadata(Reference("uuid-premis", "metadata/preservation/premis.xml", EArkMetadataReference.EMdType.PREMIS, 2551, "b4c0")));

        AssertRoundTrip(package);
    }

    [Fact]
    public void Read_WhenPackageHasDescriptiveMetadata_ShouldKeepDocumentOrder()
    {
        var package = MinimalPackage(x => x
            .AddDescriptiveMetadata(Reference("uuid-3", "metadata/descriptive/c.xml", EArkMetadataReference.EMdType.DC))
            .AddDescriptiveMetadata(Reference("uuid-1", "metadata/descriptive/a.xml", EArkMetadataReference.EMdType.MODS))
            .AddDescriptiveMetadata(Reference("uuid-2", "metadata/descriptive/b.xml", EArkMetadataReference.EMdType.EAD)));

        var actual = Render_and_read(package);

        actual.DescriptiveMetadata.Select(x => x.Id).Should().Equal("uuid-3", "uuid-1", "uuid-2");
    }

    [Fact]
    public void Read_WhenPackageHasDocumentationFiles_ShouldReturnTheSamePackage()
    {
        var package = MinimalPackage(x => x
            .AddDocumentationFile(new EArkFile { Id = "uuid-doc-1", Href = "documentation/package-description.txt", MimeType = "text/plain", Size = 594, Created = Created, Checksum = "1047" })
            .AddDocumentationFile(new EArkFile { Id = "uuid-doc-2", Href = "documentation/additionalPackageInfo.xml", MimeType = "text/xml", Size = 781, Created = Created, Checksum = "11f3" })
            .AddSchemaFile(new EArkFile { Id = "uuid-schema", Href = "schemas/mets.xsd", MimeType = "text/xml", Size = 10, Created = Created, Checksum = "aa" }));

        AssertRoundTrip(package);
    }

    [Fact]
    public void Read_WhenPackageIsRiksarkivetAdaptation_ShouldReturnTheSamePackage()
    {
        var package = MinimalPackage(x => x
            .SetContentCategory(EArkConstants.ContentCategory.Other, "Riksarkivet webbarkiv")
            .SetContentInformationType(EArkPackage.EContentInformationType.OTHER, "Eplicta web archive")
            .SetRecordStatus(EArkPackage.ERecordStatus.Supplement)
            .SetLastModDate(Modified)
            .AddAgent(EArkAgent.Archivist("Sjofartsverket", identificationCode: "ORG:202100005810"))
            .AddAgent(new EArkAgent { Role = EArkAgent.ERole.Other, OtherRole = "PRODUCER", Type = EArkAgent.EType.Other, OtherType = "SOFTWARE", Name = "W3D3", Note = "1.19", NoteType = EArkAgent.ENoteType.SoftwareVersion })
            .AddAgent(new EArkAgent { Role = EArkAgent.ERole.Creator, Type = EArkAgent.EType.Individual, Name = "Sven Svensson", Note = "sven.svensson@fm.se" })
            .AddAltRecordId(new EArkAltRecordId { Type = EArkAltRecordId.EType.SubmissionAgreement, Value = "Demo - no submission agreement" })
            .AddAltRecordId(new EArkAltRecordId { Type = EArkAltRecordId.EType.PreviousSubmissionAgreement, Value = "RA 12-2387/12726" })
            .AddAltRecordId(new EArkAltRecordId { Type = EArkAltRecordId.EType.ReferenceCode, Value = "SE/RA/123456/24/P" })
            .AddAltRecordId(new EArkAltRecordId { Type = EArkAltRecordId.EType.PreviousReferenceCode, Value = "SE/RA/123456/23/P" }));

        AssertRoundTrip(package);
    }

    [Fact]
    public void Read_WhenRepresentationHasCustomName_ShouldDeriveRepresentationNameFromTheHref()
    {
        var package = MinimalPackage(x => x
            .SetRepresentationName("rep_2")
            .AddRepresentationFile(new EArkFile { Id = "uuid-rep-2", Href = "representations/rep_2/data/webarchive.zip", MimeType = "application/zip", Size = 20, Created = Created, Checksum = "bb" }));
        package = package with { RepresentationFiles = package.RepresentationFiles.Skip(1).ToArray() };

        var actual = Render_and_read(package);

        actual.RepresentationName.Should().Be("rep_2");
    }

    // Assumption: with no representation file there is nothing to derive the name from, so the EArkPackage default applies.
    [Fact]
    public void Read_WhenPackageHasNoRepresentationFiles_ShouldUseTheDefaultRepresentationName()
    {
        var package = MinimalPackage() with { RepresentationFiles = [] };

        var actual = Render_and_read(package);

        actual.RepresentationName.Should().Be(EArkConstants.DefaultRepresentationName);
        actual.RepresentationFiles.Should().BeEmpty();
    }

    [Fact]
    public void Read_WhenPackageHasNoOptionalParts_ShouldReturnEmptyCollectionsAndNulls()
    {
        var package = MinimalPackage(x => x.SetLabel(null));

        var actual = Render_and_read(package);

        actual.Label.Should().BeNull();
        actual.OtherContentCategory.Should().BeNull();
        actual.OtherContentInformationType.Should().BeNull();
        actual.LastModDate.Should().BeNull();
        actual.AltRecordIds.Should().BeEmpty();
        actual.DescriptiveMetadata.Should().BeEmpty();
        actual.PreservationMetadata.Should().BeEmpty();
        actual.DocumentationFiles.Should().BeEmpty();
        actual.SchemaFiles.Should().BeEmpty();
    }

    [Fact]
    public void Read_WhenPackageIsRendered_ShouldReturnCreateDateAsUtcAndNeverSetContentOrSourcePath()
    {
        var package = MinimalPackage(x => x.SetLastModDate(Modified));

        var actual = Render_and_read(package);

        actual.CreateDate.Should().Be(Created);
        actual.CreateDate.Kind.Should().Be(DateTimeKind.Utc);
        actual.LastModDate!.Value.ToUniversalTime().Should().Be(Modified);
        actual.RepresentationFiles.Should().OnlyContain(x => x.Content == null && x.SourcePath == null);
        actual.AdditionalPackageInfo.Should().BeNull();
    }

    [Theory]
    [InlineData(EArkPackage.EOaisPackageType.SIP)]
    [InlineData(EArkPackage.EOaisPackageType.AIP)]
    [InlineData(EArkPackage.EOaisPackageType.DIP)]
    [InlineData(EArkPackage.EOaisPackageType.AIU)]
    [InlineData(EArkPackage.EOaisPackageType.AIC)]
    public void Read_WhenOaisPackageTypeVaries_ShouldReturnIt(EArkPackage.EOaisPackageType type)
    {
        var package = MinimalPackage() with { OaisPackageType = type };

        Render_and_read(package).OaisPackageType.Should().Be(type);
    }

    [Theory]
    [InlineData(EArkPackage.ERecordStatus.New)]
    [InlineData(EArkPackage.ERecordStatus.Supplement)]
    [InlineData(EArkPackage.ERecordStatus.Replacement)]
    [InlineData(EArkPackage.ERecordStatus.Test)]
    [InlineData(EArkPackage.ERecordStatus.Version)]
    [InlineData(EArkPackage.ERecordStatus.Delete)]
    [InlineData(EArkPackage.ERecordStatus.Other)]
    public void Read_WhenRecordStatusVaries_ShouldReturnIt(EArkPackage.ERecordStatus status)
    {
        var package = MinimalPackage(x => x.SetRecordStatus(status));

        Render_and_read(package).RecordStatus.Should().Be(status);
    }

    [Theory]
    [InlineData(EArkPackage.EContentInformationType.ERMS)]
    [InlineData(EArkPackage.EContentInformationType.SIARD1)]
    [InlineData(EArkPackage.EContentInformationType.SIARD2)]
    [InlineData(EArkPackage.EContentInformationType.SIARDDK)]
    [InlineData(EArkPackage.EContentInformationType.GeoData)]
    [InlineData(EArkPackage.EContentInformationType.MIXED)]
    [InlineData(EArkPackage.EContentInformationType.OTHER)]
    public void Read_WhenContentInformationTypeVaries_ShouldReturnIt(EArkPackage.EContentInformationType type)
    {
        var package = MinimalPackage(x => x.SetContentInformationType(type));

        Render_and_read(package).ContentInformationType.Should().Be(type);
    }

    [Theory]
    [InlineData(MetsData.EChecksumType.MD5)]
    [InlineData(MetsData.EChecksumType.SHA_1)]
    [InlineData(MetsData.EChecksumType.SHA_256)]
    [InlineData(MetsData.EChecksumType.SHA_384)]
    [InlineData(MetsData.EChecksumType.SHA_512)]
    public void Read_WhenChecksumTypeVaries_ShouldMapItOnFilesAndMetadataReferences(MetsData.EChecksumType checksumType)
    {
        var package = MinimalPackage(x => x
            .AddDescriptiveMetadata(Reference("uuid-mods", "metadata/descriptive/mods.xml", EArkMetadataReference.EMdType.MODS) with { ChecksumType = checksumType })
            .AddPreservationMetadata(Reference("uuid-premis", "metadata/preservation/premis.xml", EArkMetadataReference.EMdType.PREMIS) with { ChecksumType = checksumType })
            .AddRepresentationFile(new EArkFile { Id = "uuid-rep-b", Href = "representations/rep_1/data/b.zip", MimeType = "application/zip", Size = 1, Created = Created, Checksum = "dd", ChecksumType = checksumType }));

        var actual = Render_and_read(package);

        actual.DescriptiveMetadata.Single().ChecksumType.Should().Be(checksumType);
        actual.PreservationMetadata.Single().ChecksumType.Should().Be(checksumType);
        actual.RepresentationFiles.Last().ChecksumType.Should().Be(checksumType);
    }

    [Theory]
    [InlineData(EArkMetadataReference.EMdType.MODS)]
    [InlineData(EArkMetadataReference.EMdType.EAD)]
    [InlineData(EArkMetadataReference.EMdType.DC)]
    [InlineData(EArkMetadataReference.EMdType.PREMIS)]
    [InlineData(EArkMetadataReference.EMdType.METSRIGHTS)]
    [InlineData(EArkMetadataReference.EMdType.OTHER)]
    public void Read_WhenMdTypeVaries_ShouldReturnIt(EArkMetadataReference.EMdType mdType)
    {
        var package = MinimalPackage(x => x.AddDescriptiveMetadata(Reference("uuid-md", "metadata/descriptive/md.xml", mdType)));

        Render_and_read(package).DescriptiveMetadata.Single().MdType.Should().Be(mdType);
    }

    [Fact]
    public void Read_WhenMetadataReferenceIsSuperseded_ShouldReturnStatusSuperseded()
    {
        var package = MinimalPackage(x => x
            .AddDescriptiveMetadata(Reference("uuid-old", "metadata/descriptive/old.xml", EArkMetadataReference.EMdType.MODS) with { Status = EArkMetadataReference.EStatus.Superseded })
            .AddPreservationMetadata(Reference("uuid-premis", "metadata/preservation/premis.xml", EArkMetadataReference.EMdType.PREMIS) with { Status = EArkMetadataReference.EStatus.Superseded }));

        var actual = Render_and_read(package);

        actual.DescriptiveMetadata.Single().Status.Should().Be(EArkMetadataReference.EStatus.Superseded);
        actual.PreservationMetadata.Single().Status.Should().Be(EArkMetadataReference.EStatus.Superseded);
    }

    [Fact]
    public void Read_WhenAgentsAreRendered_ShouldReturnRoleTypeOtherValuesNameAndNote()
    {
        var package = MinimalPackage(x => x.AddAgent(new EArkAgent { Role = EArkAgent.ERole.IpOwner, Type = EArkAgent.EType.Other, OtherType = "SOFTWARE", Name = "Owner", Note = "n", NoteType = EArkAgent.ENoteType.IdentificationCode }));

        var actual = Render_and_read(package);

        actual.Agents.Should().BeEquivalentTo(package.Agents, o => o.WithStrictOrdering());
        actual.Agents.Select(a => a.Role).Should().Contain(EArkAgent.ERole.IpOwner);
        actual.Agents.First().NoteType.Should().Be(EArkAgent.ENoteType.SoftwareVersion);
    }

    [Fact]
    public void Read_WhenAgentNoteHasNoNoteType_ShouldReturnNullNoteType()
    {
        var package = MinimalPackage(x => x.AddAgent(new EArkAgent { Role = EArkAgent.ERole.Creator, Type = EArkAgent.EType.Individual, Name = "Sven", Note = "sven@example.com" }));

        var agent = Render_and_read(package).Agents.Last();

        agent.Note.Should().Be("sven@example.com");
        agent.NoteType.Should().BeNull();
    }

    // ---- Real documents ----------------------------------------------------------------------------------------

    [Fact]
    public void Read_WhenMinimalDemoDocumentIsRead_ShouldReturnItsValues()
    {
        var actual = new EArkReader().Read(TestResource.EplictaDemoMets());

        actual.ObjId.Should().Be(DemoObjId);
        actual.Label.Should().Be("www.eplicta.se");
        actual.ContentCategory.Should().Be("Web Archives");
        actual.ContentInformationType.Should().Be(EArkPackage.EContentInformationType.OTHER);
        actual.OtherContentInformationType.Should().Be("Eplicta web archive");
        actual.OaisPackageType.Should().Be(EArkPackage.EOaisPackageType.SIP);
        actual.RecordStatus.Should().Be(EArkPackage.ERecordStatus.New);
        actual.CreateDate.Should().Be(new DateTime(2026, 10, 6, 14, 0, 0, DateTimeKind.Utc));
        actual.CreateDate.Kind.Should().Be(DateTimeKind.Utc);
        actual.AdditionalPackageInfo.Should().BeNull();
    }

    [Fact]
    public void Read_WhenMinimalDemoDocumentIsRead_ShouldReturnTheModsReference()
    {
        var actual = new EArkReader().Read(TestResource.EplictaDemoMets());

        var mods = actual.DescriptiveMetadata.Single();
        mods.Id.Should().Be("uuid-e2fc14e9-39f0-45b6-aab9-fe700fa3ee91");
        mods.Href.Should().Be("metadata/descriptive/mods.xml");
        mods.MdType.Should().Be(EArkMetadataReference.EMdType.MODS);
        mods.MimeType.Should().Be("text/xml");
        mods.Size.Should().Be(518);
        mods.Created.Should().Be(new DateTime(2026, 10, 6, 14, 0, 0, DateTimeKind.Utc));
        mods.Checksum.Should().Be("909fb4aed5b17cb57da66218e19343faeee47ae74342387b382d8e6f635b8876");
        mods.ChecksumType.Should().Be(MetsData.EChecksumType.SHA_256);
        mods.Status.Should().Be(EArkMetadataReference.EStatus.Current);
        mods.Content.Should().BeNull();
        actual.PreservationMetadata.Should().BeEmpty();
    }

    [Fact]
    public void Read_WhenMinimalDemoDocumentIsRead_ShouldReturnTheWebArchiveRepresentationFile()
    {
        var actual = new EArkReader().Read(TestResource.EplictaDemoMets());

        actual.RepresentationName.Should().Be("rep_1");
        var file = actual.RepresentationFiles.Single(x => x.Href == "representations/rep_1/data/webarchive.zip");
        file.MimeType.Should().Be("application/zip");
        file.Size.Should().Be(29326);
        file.Checksum.Should().Be("18d3ff1fe5e7cc8cc8a68e3e03137deac8ce7c123d0bf18e7c7ebe2831d75bf7");
        file.ChecksumType.Should().Be(MetsData.EChecksumType.SHA_256);
        file.Content.Should().BeNull();
        file.SourcePath.Should().BeNull();
    }

    [Fact]
    public void Read_WhenDemoDocumentIsRead_ShouldReturnTheAgentsAndAltRecordIds()
    {
        var actual = new EArkReader().Read(TestResource.EplictaDemoFullMets());

        actual.Agents.Should().HaveCount(3);
        var software = actual.Agents[0];
        software.Role.Should().Be(EArkAgent.ERole.Creator);
        software.Type.Should().Be(EArkAgent.EType.Other);
        software.OtherType.Should().Be("SOFTWARE");
        software.Name.Should().Be("Eplicta WebConserver");
        software.Note.Should().Be("1.4.2");
        software.NoteType.Should().Be(EArkAgent.ENoteType.SoftwareVersion);
        actual.Agents[1].Role.Should().Be(EArkAgent.ERole.Creator);
        actual.Agents[1].Type.Should().Be(EArkAgent.EType.Organization);
        actual.Agents[1].NoteType.Should().Be(EArkAgent.ENoteType.IdentificationCode);
        actual.Agents[2].Role.Should().Be(EArkAgent.ERole.Archivist);
        actual.AltRecordIds.Select(x => (x.Type, x.Value)).Should().Equal(
            (EArkAltRecordId.EType.SubmissionAgreement, "Demo - no submission agreement"),
            (EArkAltRecordId.EType.ReferenceCode, "SE/DEMO/2026/1"));
    }

    [Fact]
    public void Read_WhenFullDemoDocumentIsRead_ShouldReturnTheModsReference()
    {
        var actual = new EArkReader().Read(TestResource.EplictaDemoFullMets());

        actual.ObjId.Should().Be(DemoObjId);
        actual.Label.Should().Be("www.eplicta.se");
        var mods = actual.DescriptiveMetadata.First();
        mods.Href.Should().Be("metadata/descriptive/mods.xml");
        mods.MdType.Should().Be(EArkMetadataReference.EMdType.MODS);
        mods.Size.Should().Be(518);
        mods.Checksum.Should().Be("9f7cf2c61c38e8b021e7fab2e3a694c1335bf719c505ebd73a4ceb816e43878f");
        mods.ChecksumType.Should().Be(MetsData.EChecksumType.SHA_256);
    }

    [Fact]
    public void Read_WhenFullDemoDocumentIsRead_ShouldReturnEadAfterModsInDocumentOrder()
    {
        var actual = new EArkReader().Read(TestResource.EplictaDemoFullMets());

        actual.DescriptiveMetadata.Select(x => x.MdType).Should().Equal(EArkMetadataReference.EMdType.MODS, EArkMetadataReference.EMdType.EAD);
        actual.DescriptiveMetadata.Last().Href.Should().Be("metadata/descriptive/ead.xml");
        actual.DescriptiveMetadata.Last().Size.Should().Be(1369);
    }

    [Fact]
    public void Read_WhenFullDemoDocumentIsRead_ShouldReturnThePremisDigiprovReference()
    {
        var actual = new EArkReader().Read(TestResource.EplictaDemoFullMets());

        var premis = actual.PreservationMetadata.Single();
        premis.Id.Should().Be("uuid-9f089f8f-a7ce-4655-b1f9-7b9bcd5b298d");
        premis.Href.Should().Be("metadata/preservation/premis.xml");
        premis.MdType.Should().Be(EArkMetadataReference.EMdType.PREMIS);
        premis.Size.Should().Be(2551);
        premis.Status.Should().Be(EArkMetadataReference.EStatus.Current);
    }

    [Fact]
    public void Read_WhenFullDemoDocumentIsRead_ShouldReturnTheDocumentationFiles()
    {
        var actual = new EArkReader().Read(TestResource.EplictaDemoFullMets());

        actual.DocumentationFiles.Select(x => x.Href).Should().Equal(
            "documentation/package-description.txt",
            "documentation/metadatabilaga.xlsx",
            "documentation/additionalPackageInfo.xml");
        actual.DocumentationFiles.Single(x => x.Href == "documentation/additionalPackageInfo.xml").Size.Should().Be(781);
    }

    [Fact]
    public void Read_WhenFullDemoDocumentIsRead_ShouldReturnTheSchemaFiles()
    {
        var actual = new EArkReader().Read(TestResource.EplictaDemoFullMets());

        actual.SchemaFiles.Should().HaveCount(7);
        actual.SchemaFiles.Select(x => x.Href).Should().Contain(["schemas/mets.xsd", "schemas/premis-v3-0.xsd", "schemas/SNAadditionalPackageInfo.xsd"]);
    }

    [Fact]
    public void Read_WhenFullDemoDocumentIsRead_ShouldReturnTheRepresentationFileAndName()
    {
        var actual = new EArkReader().Read(TestResource.EplictaDemoFullMets());

        actual.RepresentationName.Should().Be("rep_1");
        var file = actual.RepresentationFiles.Single();
        file.Href.Should().Be("representations/rep_1/data/webarchive.zip");
        file.Checksum.Should().Be("a928c5f0f383461b317047a147dac43c819659f325e035984cc8b66c63ded349");
        file.ChecksumType.Should().Be(MetsData.EChecksumType.SHA_256);
        file.Size.Should().Be(29801);
    }

    [Fact]
    public void Read_WhenDemoDocumentHasUnknownAttributes_ShouldIgnoreThem()
    {
        // The demo documents carry xsi:schemaLocation and sip:/csip: attributes that have no counterpart on EArkPackage.
        var act = () => new EArkReader().Read(TestResource.EplictaDemoFullMets());

        act.Should().NotThrow();
    }

    [Fact]
    public void Read_WhenRootCarriesUnknownExtensionAttributes_ShouldIgnoreThem()
    {
        var document = new EArkRenderer(MinimalPackage()).Render();
        document.DocumentElement!.SetAttribute("xmlns:ext", "http://example.com/ext");
        document.DocumentElement.SetAttribute("FOO", "http://example.com/ext", "bar");

        var actual = new EArkReader().Read(document.OuterXml);

        actual.ObjId.Should().Be("uuid-11111111-1111-1111-1111-111111111111");
    }

    [Theory]
    [InlineData("DDI")]
    [InlineData("MARC")]
    public void Read_WhenMdTypeIsOutsideTheModel_ShouldReadItAsOther(string mdType)
    {
        var xml = TestResource.EplictaDemoFullMets().Replace("MDTYPE=\"EAD\"", $"MDTYPE=\"{mdType}\"");

        var actual = new EArkReader().Read(xml);

        actual.DescriptiveMetadata.Select(x => x.MdType).Should().Equal(EArkMetadataReference.EMdType.MODS, EArkMetadataReference.EMdType.OTHER);
    }

    [Fact]
    public void Read_WhenAgentRoleAndTypeAreOutsideTheModel_ShouldReadThemAsOther()
    {
        var xml = TestResource.EplictaDemoFullMets().Replace("ROLE=\"ARCHIVIST\" TYPE=\"ORGANIZATION\"", "ROLE=\"SUBMITTER\" TYPE=\"GROUP\"");

        var actual = new EArkReader().Read(xml);

        actual.Agents.Last().Role.Should().Be(EArkAgent.ERole.Other);
        actual.Agents.Last().Type.Should().Be(EArkAgent.EType.Other);
    }

    [Fact]
    public void Read_WhenChecksumTypeIsUnknown_ShouldThrow()
    {
        var xml = TestResource.EplictaDemoFullMets().Replace("CHECKSUMTYPE=\"SHA-256\"", "CHECKSUMTYPE=\"CRC32\"");

        var act = () => new EArkReader().Read(xml);

        act.Should().Throw<InvalidDataException>().WithMessage("*CHECKSUMTYPE*");
    }

    // ---- Riksarkivet example -----------------------------------------------------------------------------------

    // The Riksarkivet example has backslash hrefs, zone-less dates and a CONTENTINFORMATIONTYPE outside the enum.
    [Fact]
    public void Read_WhenRiksarkivetExampleIsRead_ShouldNotThrow()
    {
        var act = () => new EArkReader().Read(TestResource.RiksarkivetExampleMets());

        act.Should().NotThrow();
    }

    [Fact]
    public void Read_WhenRiksarkivetExampleIsRead_ShouldReturnHeaderValues()
    {
        var actual = new EArkReader().Read(TestResource.RiksarkivetExampleMets());

        actual.ObjId.Should().Be("IP_exempel_1");
        actual.Label.Should().Be("ERMS_records serie RA-KS 2022");
        actual.ContentCategory.Should().Be("Datasets");
        actual.OaisPackageType.Should().Be(EArkPackage.EOaisPackageType.SIP);
        actual.RecordStatus.Should().Be(EArkPackage.ERecordStatus.New);
        actual.CreateDate.Should().Be(new DateTime(2022, 11, 1, 14, 4, 23, DateTimeKind.Utc));
        actual.CreateDate.Kind.Should().Be(DateTimeKind.Utc);
        actual.LastModDate!.Value.ToUniversalTime().Should().Be(new DateTime(2022, 11, 1, 14, 4, 23, DateTimeKind.Utc));
        actual.Agents.Should().HaveCount(7);
        actual.AltRecordIds.Select(x => x.Type).Should().Equal(
            EArkAltRecordId.EType.SubmissionAgreement,
            EArkAltRecordId.EType.PreviousSubmissionAgreement,
            EArkAltRecordId.EType.ReferenceCode,
            EArkAltRecordId.EType.PreviousReferenceCode);
    }

    [Fact]
    public void Read_WhenRiksarkivetExampleIsRead_ShouldReturnItsAgentRoles()
    {
        var actual = new EArkReader().Read(TestResource.RiksarkivetExampleMets());

        actual.Agents.Select(x => x.Role).Should().Equal(
            EArkAgent.ERole.Creator,
            EArkAgent.ERole.Archivist,
            EArkAgent.ERole.Creator,
            EArkAgent.ERole.Creator,
            EArkAgent.ERole.Preservation,
            EArkAgent.ERole.Editor,
            EArkAgent.ERole.Other);
        actual.Agents.Last().OtherRole.Should().Be("PRODUCER");
    }

    [Fact]
    public void Read_WhenRiksarkivetExampleIsRead_ShouldReturnItsMetadataReferences()
    {
        var actual = new EArkReader().Read(TestResource.RiksarkivetExampleMets());

        actual.DescriptiveMetadata.Should().ContainSingle();
        actual.DescriptiveMetadata.Single().MdType.Should().Be(EArkMetadataReference.EMdType.EAD);
        actual.DescriptiveMetadata.Single().Size.Should().Be(1097);
        actual.PreservationMetadata.Should().HaveCount(2);
        actual.PreservationMetadata.Should().OnlyContain(x => x.MdType == EArkMetadataReference.EMdType.PREMIS && x.Size == 26832);
    }

    [Fact]
    public void Read_WhenRiksarkivetExampleIsRead_ShouldReturnItsFiles()
    {
        var actual = new EArkReader().Read(TestResource.RiksarkivetExampleMets());

        actual.DocumentationFiles.Should().ContainSingle();
        actual.DocumentationFiles.Single().Href.Should().EndWith("additionalPackageInfo.xml");
        actual.SchemaFiles.Should().HaveCount(9);
        actual.RepresentationFiles.Should().HaveCount(11);
        actual.RepresentationFiles.First().Href.Should().EndWith("ERMS.xml");
        actual.RepresentationFiles.First().Size.Should().Be(78443);
        actual.RepresentationFiles.First().ChecksumType.Should().Be(MetsData.EChecksumType.SHA_256);
        actual.RepresentationName.Should().Be("rep_1");
    }

    // ---- ReadMods ----------------------------------------------------------------------------------------------

    [Fact]
    public void ReadMods_WhenDemoModsIsRead_ShouldReturnIdentifier()
    {
        var actual = new EArkReader().ReadMods(TestResource.EplictaDemoMods());

        actual.Identifier.Value.Should().Be(DemoObjId);
        actual.Identifier.Type.Should().Be("local");
    }

    [Fact]
    public void ReadMods_WhenDemoModsIsRead_ShouldReturnLocationUrlAndTitle()
    {
        var actual = new EArkReader().ReadMods(TestResource.EplictaDemoMods());

        actual.Location.Url.Value.Should().Be("https://www.eplicta.se/");
        actual.TitleInfo.Title.Value.Should().Be("www.eplicta.se");
    }

    [Fact]
    public void ReadMods_WhenDemoModsIsRead_ShouldReturnDateIssuedAsUtcInstantWithEncoding()
    {
        var actual = new EArkReader().ReadMods(TestResource.EplictaDemoMods());

        actual.OriginInfo.DateIssued.Encoding.Should().Be("w3cdtf");
        actual.OriginInfo.DateIssued.Value.ToUniversalTime().Should().Be(new DateTime(2026, 10, 9, 13, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void ReadMods_WhenDemoModsHasEmptyRelatedItem_ShouldReturnRelatedItem()
    {
        var actual = new EArkReader().ReadMods(TestResource.EplictaDemoMods());

        actual.RelatedItem.Should().NotBeNull();
    }

    // ---- Error cases -------------------------------------------------------------------------------------------

    [Fact]
    public void Read_WhenRootIsNotMets_ShouldThrowInvalidDataException()
    {
        var act = () => new EArkReader().Read("<foo OBJID=\"a\" />");

        act.Should().Throw<InvalidDataException>();
    }

    [Fact]
    public void Read_WhenRootIsMetsInAnotherNamespace_ShouldThrowInvalidDataException()
    {
        var act = () => new EArkReader().Read("<mets xmlns=\"http://example.com/not-mets\" OBJID=\"a\" />");

        act.Should().Throw<InvalidDataException>();
    }

    [Fact]
    public void Read_WhenRootIsMetsWithoutNamespace_ShouldThrowInvalidDataException()
    {
        var act = () => new EArkReader().Read("<mets OBJID=\"a\" />");

        act.Should().Throw<InvalidDataException>();
    }

    [Fact]
    public void Read_WhenObjIdIsMissing_ShouldThrowInvalidDataException()
    {
        var document = new EArkRenderer(MinimalPackage()).Render();
        document.DocumentElement!.RemoveAttribute("OBJID");

        var act = () => new EArkReader().Read(document.OuterXml);

        act.Should().Throw<InvalidDataException>().WithMessage("*OBJID*");
    }

    // Assumption: an empty OBJID counts as lacking it.
    [Fact]
    public void Read_WhenObjIdIsEmpty_ShouldThrowInvalidDataException()
    {
        var document = new EArkRenderer(MinimalPackage()).Render();
        document.DocumentElement!.SetAttribute("OBJID", "");

        var act = () => new EArkReader().Read(document.OuterXml);

        act.Should().Throw<InvalidDataException>();
    }

    [Fact]
    public void ReadMods_WhenRootIsNotMods_ShouldThrowInvalidDataException()
    {
        var act = () => new EArkReader().ReadMods("<mods:modsCollection xmlns:mods=\"http://www.loc.gov/mods/v3\" />");

        act.Should().Throw<InvalidDataException>();
    }

    [Fact]
    public void ReadMods_WhenRootIsModsInAnotherNamespace_ShouldThrowInvalidDataException()
    {
        var act = () => new EArkReader().ReadMods("<mods xmlns=\"http://example.com/other\" />");

        act.Should().Throw<InvalidDataException>();
    }

    [Fact]
    public void ReadMods_WhenDocumentIsMets_ShouldThrowInvalidDataException()
    {
        var act = () => new EArkReader().ReadMods(TestResource.EplictaDemoMets());

        act.Should().Throw<InvalidDataException>();
    }
}
