using System;
using System.Linq;
using System.Xml;
using Eplicta.Mets.Entities;
using Eplicta.Mets.Features.EArk;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests.Features.EArk;

public class EArkRendererTests
{
    private static readonly DateTime Created = new(2026, 3, 14, 9, 30, 0, DateTimeKind.Utc);

    private static EArkPackage Package(Action<EArkPackageBuilder> configure = null)
    {
        var builder = new EArkPackageBuilder()
            .SetObjId("uuid-11111111-1111-1111-1111-111111111111")
            .SetLabel("www.sjofartsverket.se")
            .SetCreateDate(Created)
            .SetSoftware("Eplicta WebConserver", "1.2.3")
            .AddAgent(EArkAgent.Submitter("Sjofartsverket", identificationCode: "ORG:202100005810"))
            .AddSchemaFile(new EArkFile { Id = "uuid-schema", Href = "schemas/mets.xsd", MimeType = "text/xml", Size = 10, Created = Created, Checksum = "aa" })
            .AddRepresentationFile(new EArkFile { Id = "uuid-rep", Href = "representations/rep_1/data/webarchive.zip", MimeType = "application/zip", Size = 20, Created = Created, Checksum = "bb" });

        configure?.Invoke(builder);
        return builder.Build();
    }

    private static XmlDocument Render(EArkPackage package = null)
    {
        return new EArkRenderer(package ?? Package()).Render();
    }

    private static XmlNamespaceManager Namespaces(XmlDocument document)
    {
        var manager = new XmlNamespaceManager(document.NameTable);
        manager.AddNamespace("m", EArkConstants.MetsNamespace);
        manager.AddNamespace("csip", EArkConstants.CsipNamespace);
        manager.AddNamespace("xlink", EArkConstants.XlinkNamespace);
        return manager;
    }

    private static XmlElement Select(XmlDocument document, string xpath)
    {
        return (XmlElement)document.SelectSingleNode(xpath, Namespaces(document));
    }

    [Fact]
    public void The_root_is_a_mets_element_in_the_mets_namespace()
    {
        var root = Render().DocumentElement;

        root!.LocalName.Should().Be("mets");
        root.NamespaceURI.Should().Be(EArkConstants.MetsNamespace);
    }

    [Fact]
    public void The_root_carries_the_sip_profile()
    {
        Render().DocumentElement!.GetAttribute("PROFILE").Should().Be("https://earksip.dilcis.eu/profile/E-ARK-SIP.xml");
    }

    [Fact]
    public void The_root_carries_the_objid_label_and_content_category()
    {
        var root = Render().DocumentElement;

        root!.GetAttribute("OBJID").Should().Be("uuid-11111111-1111-1111-1111-111111111111");
        root.GetAttribute("LABEL").Should().Be("www.sjofartsverket.se");
        root.GetAttribute("TYPE").Should().Be("Web Archives");
    }

    [Fact]
    public void The_content_information_type_is_written_in_the_csip_namespace()
    {
        var package = Package(x => x.SetContentInformationType(EArkPackage.EContentInformationType.OTHER, "Eplicta web archive"));
        var root = Render(package).DocumentElement;

        root!.GetAttribute("CONTENTINFORMATIONTYPE", EArkConstants.CsipNamespace).Should().Be("OTHER");
        root.GetAttribute("OTHERCONTENTINFORMATIONTYPE", EArkConstants.CsipNamespace).Should().Be("Eplicta web archive");
    }

    [Fact]
    public void The_schema_location_points_at_the_schemas_carried_in_the_package()
    {
        var schemaLocation = Render().DocumentElement!.GetAttribute("schemaLocation", EArkConstants.XsiNamespace);

        schemaLocation.Should().Contain($"{EArkConstants.MetsNamespace} schemas/mets.xsd");
        schemaLocation.Should().Contain($"{EArkConstants.CsipNamespace} schemas/DILCISExtensionMETS.xsd");
        schemaLocation.Should().Contain($"{EArkConstants.SipNamespace} schemas/DILCISExtensionSIPMETS.xsd");
        schemaLocation.Should().Contain($"{EArkConstants.XlinkNamespace} schemas/xlink.xsd");
    }

    [Fact]
    public void The_header_carries_the_create_date_record_status_and_package_type()
    {
        var header = Select(Render(), "/m:mets/m:metsHdr");

        header.GetAttribute("CREATEDATE").Should().Be("2026-03-14T09:30:00Z");
        header.GetAttribute("RECORDSTATUS").Should().Be("NEW");
        header.GetAttribute("OAISPACKAGETYPE", EArkConstants.CsipNamespace).Should().Be("SIP");
    }

    [Fact]
    public void The_last_modification_date_is_omitted_until_the_package_is_modified()
    {
        Select(Render(), "/m:mets/m:metsHdr").HasAttribute("LASTMODDATE").Should().BeFalse();

        var modified = Package(x => x.SetLastModDate(Created.AddDays(1)));
        Select(Render(modified), "/m:mets/m:metsHdr").GetAttribute("LASTMODDATE").Should().Be("2026-03-15T09:30:00Z");
    }

    [Fact]
    public void Dates_are_rendered_as_the_true_utc_instant()
    {
        var local = new DateTime(2026, 3, 14, 9, 30, 0, DateTimeKind.Local);
        var package = Package(x => x.SetCreateDate(local));

        Select(Render(package), "/m:mets/m:metsHdr").GetAttribute("CREATEDATE").Should().Be(local.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss'Z'"));
    }

    [Fact]
    public void The_software_agent_carries_the_values_csip_fixes()
    {
        var agent = Select(Render(), "/m:mets/m:metsHdr/m:agent[@OTHERTYPE='SOFTWARE']");

        agent.GetAttribute("ROLE").Should().Be("CREATOR");
        agent.GetAttribute("TYPE").Should().Be("OTHER");
        agent.SelectSingleNode("m:name", Namespaces(agent.OwnerDocument))!.InnerText.Should().Be("Eplicta WebConserver");

        var note = (XmlElement)agent.SelectSingleNode("m:note", Namespaces(agent.OwnerDocument));
        note!.InnerText.Should().Be("1.2.3");
        note.GetAttribute("NOTETYPE", EArkConstants.CsipNamespace).Should().Be("SOFTWARE VERSION");
    }

    [Fact]
    public void The_submitting_agent_carries_its_identification_code()
    {
        var agent = Select(Render(), "/m:mets/m:metsHdr/m:agent[@TYPE='ORGANIZATION']");

        agent.GetAttribute("ROLE").Should().Be("CREATOR");

        var note = (XmlElement)agent.SelectSingleNode("m:note", Namespaces(agent.OwnerDocument));
        note!.InnerText.Should().Be("ORG:202100005810");
        note.GetAttribute("NOTETYPE", EArkConstants.CsipNamespace).Should().Be("IDENTIFICATIONCODE");
    }

    [Fact]
    public void Alt_record_ids_are_written_with_their_uppercase_type()
    {
        var package = Package(x => x.AddAltRecordId(new EArkAltRecordId { Type = EArkAltRecordId.EType.SubmissionAgreement, Value = "RA 13-2011/5329" }));

        var altRecordId = Select(Render(package), "/m:mets/m:metsHdr/m:altRecordID");
        altRecordId.GetAttribute("TYPE").Should().Be("SUBMISSIONAGREEMENT");
        altRecordId.InnerText.Should().Be("RA 13-2011/5329");
    }

    [Fact]
    public void Descriptive_metadata_is_referenced_rather_than_embedded()
    {
        var package = Package(x => x.AddDescriptiveMetadata(new EArkMetadataReference
        {
            Id = "uuid-dmd",
            Href = "metadata/descriptive/mods.xml",
            MdType = EArkMetadataReference.EMdType.MODS,
            Size = 1097,
            Created = Created,
            Checksum = "abc123"
        }));

        var document = Render(package);
        var dmdSec = Select(document, "/m:mets/m:dmdSec");
        dmdSec.GetAttribute("ID").Should().Be("uuid-dmd");
        dmdSec.GetAttribute("CREATED").Should().Be("2026-03-14T09:30:00Z");
        dmdSec.GetAttribute("STATUS").Should().Be("CURRENT");

        document.SelectSingleNode("/m:mets/m:dmdSec/m:mdWrap", Namespaces(document)).Should().BeNull();

        var mdRef = Select(document, "/m:mets/m:dmdSec/m:mdRef");
        mdRef.GetAttribute("LOCTYPE").Should().Be("URL");
        mdRef.GetAttribute("type", EArkConstants.XlinkNamespace).Should().Be("simple");
        mdRef.GetAttribute("href", EArkConstants.XlinkNamespace).Should().Be("metadata/descriptive/mods.xml");
        mdRef.GetAttribute("MDTYPE").Should().Be("MODS");
        mdRef.GetAttribute("MIMETYPE").Should().Be("text/xml");
        mdRef.GetAttribute("SIZE").Should().Be("1097");
        mdRef.GetAttribute("CHECKSUM").Should().Be("abc123");
        mdRef.GetAttribute("CHECKSUMTYPE").Should().Be("SHA-256");
    }

    [Fact]
    public void Preservation_metadata_becomes_a_digiprov_entry_in_a_single_amdsec()
    {
        var package = Package(x => x
            .AddPreservationMetadata(new EArkMetadataReference { Id = "uuid-p1", Href = "metadata/preservation/premis1.xml", MdType = EArkMetadataReference.EMdType.PREMIS, Created = Created })
            .AddPreservationMetadata(new EArkMetadataReference { Id = "uuid-p2", Href = "metadata/preservation/premis2.xml", MdType = EArkMetadataReference.EMdType.PREMIS, Created = Created }));

        var document = Render(package);

        document.SelectNodes("/m:mets/m:amdSec", Namespaces(document))!.Count.Should().Be(1);
        document.SelectNodes("/m:mets/m:amdSec/m:digiprovMD", Namespaces(document))!.Count.Should().Be(2);
        Select(document, "/m:mets/m:amdSec/m:digiprovMD[@ID='uuid-p1']/m:mdRef").GetAttribute("MDTYPE").Should().Be("PREMIS");
    }

    [Fact]
    public void An_amdsec_is_omitted_when_there_is_no_preservation_metadata()
    {
        var document = Render();

        document.SelectSingleNode("/m:mets/m:amdSec", Namespaces(document)).Should().BeNull();
    }

    [Fact]
    public void Files_are_grouped_by_use()
    {
        var package = Package(x => x.AddDocumentationFile(new EArkFile { Id = "uuid-doc", Href = "documentation/metadatabilaga.xlsx", MimeType = "application/vnd.ms-excel", Size = 5, Created = Created, Checksum = "cc" }));
        var document = Render(package);

        Select(document, "/m:mets/m:fileSec/m:fileGrp[@USE='Documentation']").Should().NotBeNull();
        Select(document, "/m:mets/m:fileSec/m:fileGrp[@USE='Schemas']").Should().NotBeNull();
        Select(document, "/m:mets/m:fileSec/m:fileGrp[@USE='Representations']").Should().NotBeNull();
    }

    [Fact]
    public void A_file_group_without_files_is_not_written()
    {
        var document = Render();

        document.SelectSingleNode("/m:mets/m:fileSec/m:fileGrp[@USE='Documentation']", Namespaces(document)).Should().BeNull();
    }

    [Fact]
    public void A_file_carries_its_checksum_size_and_location()
    {
        var file = Select(Render(), "/m:mets/m:fileSec/m:fileGrp[@USE='Representations']/m:file");

        file.GetAttribute("ID").Should().Be("uuid-rep");
        file.GetAttribute("MIMETYPE").Should().Be("application/zip");
        file.GetAttribute("SIZE").Should().Be("20");
        file.GetAttribute("CREATED").Should().Be("2026-03-14T09:30:00Z");
        file.GetAttribute("CHECKSUM").Should().Be("bb");
        file.GetAttribute("CHECKSUMTYPE").Should().Be("SHA-256");

        var fLocat = (XmlElement)file.SelectSingleNode("m:FLocat", Namespaces(file.OwnerDocument));
        fLocat!.GetAttribute("LOCTYPE").Should().Be("URL");
        fLocat.GetAttribute("type", EArkConstants.XlinkNamespace).Should().Be("simple");
        fLocat.GetAttribute("href", EArkConstants.XlinkNamespace).Should().Be("representations/rep_1/data/webarchive.zip");
    }

    [Fact]
    public void Checksum_types_are_written_with_the_hyphen_the_vocabulary_uses()
    {
        var package = Package(x => x.AddDocumentationFile(new EArkFile { Href = "documentation/a.txt", ChecksumType = MetsData.EChecksumType.MD5, Created = Created, Checksum = "dd" }));

        Select(Render(package), "/m:mets/m:fileSec/m:fileGrp[@USE='Documentation']/m:file").GetAttribute("CHECKSUMTYPE").Should().Be("MD5");
    }

    [Fact]
    public void The_structural_map_is_physical_and_labelled_csip()
    {
        var structMap = Select(Render(), "/m:mets/m:structMap");

        structMap.GetAttribute("TYPE").Should().Be("PHYSICAL");
        structMap.GetAttribute("LABEL").Should().Be("CSIP");
    }

    [Fact]
    public void The_structural_map_has_a_single_root_division()
    {
        var document = Render();

        document.SelectNodes("/m:mets/m:structMap/m:div", Namespaces(document))!.Count.Should().Be(1);
    }

    [Fact]
    public void Each_structural_division_points_at_its_file_group()
    {
        var package = Package(x => x.AddDocumentationFile(new EArkFile { Id = "uuid-doc", Href = "documentation/a.txt", Created = Created, Checksum = "cc" }));
        var document = Render(package);

        foreach (var use in new[] { "Documentation", "Schemas", "Representations" })
        {
            var fileGrpId = Select(document, $"/m:mets/m:fileSec/m:fileGrp[@USE='{use}']").GetAttribute("ID");
            var fptr = Select(document, $"/m:mets/m:structMap/m:div/m:div[@LABEL='{use}']/m:fptr");

            fptr.GetAttribute("FILEID").Should().Be(fileGrpId);
        }
    }

    [Fact]
    public void The_metadata_division_references_the_current_metadata_sections()
    {
        var package = Package(x => x
            .AddDescriptiveMetadata(new EArkMetadataReference { Id = "uuid-dmd", Href = "metadata/descriptive/mods.xml", Created = Created })
            .AddPreservationMetadata(new EArkMetadataReference { Id = "uuid-amd", Href = "metadata/preservation/premis.xml", Created = Created }));

        var division = Select(Render(package), "/m:mets/m:structMap/m:div/m:div[@LABEL='Metadata']");

        division.GetAttribute("DMDID").Should().Be("uuid-dmd");
        division.GetAttribute("ADMID").Should().Be("uuid-amd");
    }

    [Fact]
    public void Superseded_metadata_is_left_out_of_the_metadata_division()
    {
        var package = Package(x => x
            .AddDescriptiveMetadata(new EArkMetadataReference { Id = "uuid-current", Href = "metadata/descriptive/a.xml", Created = Created })
            .AddDescriptiveMetadata(new EArkMetadataReference { Id = "uuid-old", Href = "metadata/descriptive/b.xml", Created = Created, Status = EArkMetadataReference.EStatus.Superseded }));

        Select(Render(package), "/m:mets/m:structMap/m:div/m:div[@LABEL='Metadata']").GetAttribute("DMDID").Should().Be("uuid-current");
    }

    [Fact]
    public void Element_order_follows_the_mets_content_model()
    {
        var package = Package(x => x
            .AddDescriptiveMetadata(new EArkMetadataReference { Href = "metadata/descriptive/mods.xml", Created = Created })
            .AddPreservationMetadata(new EArkMetadataReference { Href = "metadata/preservation/premis.xml", Created = Created }));

        var names = Render(package).DocumentElement!.ChildNodes.Cast<XmlNode>().Select(x => x.LocalName).ToArray();

        names.Should().Equal("metsHdr", "dmdSec", "amdSec", "fileSec", "structMap");
    }
}
