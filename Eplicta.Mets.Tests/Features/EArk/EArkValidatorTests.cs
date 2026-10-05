using System;
using System.Linq;
using System.Xml;
using Eplicta.Mets.Entities;
using Eplicta.Mets.Features.EArk;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests.Features.EArk;

public class EArkValidatorTests
{
    private static readonly DateTime Created = new(2026, 3, 14, 9, 30, 0, DateTimeKind.Utc);

    private static EArkPackageBuilder Builder()
    {
        return new EArkPackageBuilder()
            .SetObjId("uuid-11111111-1111-1111-1111-111111111111")
            .SetLabel("www.sjofartsverket.se")
            .SetCreateDate(Created)
            .SetSoftware("Eplicta WebConserver", "1.2.3")
            .AddAgent(EArkAgent.Submitter("Sjofartsverket", identificationCode: "ORG:202100005810"));
    }

    private static string Errors(XmlDocument document)
    {
        var results = new EArkValidator().Validate(document).ToArray();
        return string.Join(Environment.NewLine, results.Select(x => $"{x.XmlSeverityType}: {x.Message}"));
    }

    [Fact]
    public void A_minimal_package_validates_against_the_eark_schemas()
    {
        var package = Builder()
            .AddSchemaFile(new EArkFile { Href = "schemas/mets.xsd", MimeType = "text/xml", Size = 133920, Created = Created, Checksum = "aa" })
            .AddRepresentationFile(new EArkFile { Href = "representations/rep_1/data/webarchive.zip", MimeType = "application/zip", Size = 2048, Created = Created, Checksum = "bb" })
            .Build();

        Errors(new EArkRenderer(package).Render()).Should().BeEmpty();
    }

    [Fact]
    public void A_full_package_validates_against_the_eark_schemas()
    {
        var package = Builder()
            .SetContentInformationType(EArkPackage.EContentInformationType.OTHER, "Eplicta web archive")
            .SetLastModDate(Created.AddDays(1))
            .AddAgent(EArkAgent.Archivist("Sjofartsverket", identificationCode: "ORG:202100005810"))
            .AddAltRecordId(new EArkAltRecordId { Type = EArkAltRecordId.EType.SubmissionAgreement, Value = "RA 13-2011/5329" })
            .AddAltRecordId(new EArkAltRecordId { Type = EArkAltRecordId.EType.ReferenceCode, Value = "SE/RA/123456/24/P" })
            .AddDescriptiveMetadata(new EArkMetadataReference { Href = "metadata/descriptive/mods.xml", MdType = EArkMetadataReference.EMdType.MODS, Size = 1097, Created = Created, Checksum = "cc" })
            .AddPreservationMetadata(new EArkMetadataReference { Href = "metadata/preservation/premis.xml", MdType = EArkMetadataReference.EMdType.PREMIS, Size = 4321, Created = Created, Checksum = "dd" })
            .AddDocumentationFile(new EArkFile { Href = "documentation/metadatabilaga.xlsx", MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Size = 9, Created = Created, Checksum = "ee" })
            .AddSchemaFile(new EArkFile { Href = "schemas/mets.xsd", MimeType = "text/xml", Size = 133920, Created = Created, Checksum = "ff" })
            .AddRepresentationFile(new EArkFile { Href = "representations/rep_1/data/webarchive.zip", MimeType = "application/zip", Size = 2048, Created = Created, Checksum = "00" })
            .Build();

        Errors(new EArkRenderer(package).Render()).Should().BeEmpty();
    }

    [Theory]
    [InlineData(MetsData.EChecksumType.MD5)]
    [InlineData(MetsData.EChecksumType.SHA_1)]
    [InlineData(MetsData.EChecksumType.SHA_256)]
    [InlineData(MetsData.EChecksumType.SHA_512)]
    public void Every_checksum_type_we_emit_is_in_the_mets_vocabulary(MetsData.EChecksumType checksumType)
    {
        var package = Builder()
            .AddRepresentationFile(new EArkFile { Href = "representations/rep_1/data/webarchive.zip", MimeType = "application/zip", Created = Created, Checksum = "bb", ChecksumType = checksumType })
            .Build();

        Errors(new EArkRenderer(package).Render()).Should().BeEmpty();
    }

    [Theory]
    [InlineData(EArkPackage.ERecordStatus.New)]
    [InlineData(EArkPackage.ERecordStatus.Supplement)]
    [InlineData(EArkPackage.ERecordStatus.Replacement)]
    [InlineData(EArkPackage.ERecordStatus.Test)]
    [InlineData(EArkPackage.ERecordStatus.Version)]
    [InlineData(EArkPackage.ERecordStatus.Delete)]
    [InlineData(EArkPackage.ERecordStatus.Other)]
    public void Every_record_status_we_emit_validates(EArkPackage.ERecordStatus recordStatus)
    {
        var package = Builder().SetRecordStatus(recordStatus).AddRepresentationFile(new EArkFile { Href = "representations/rep_1/data/a.zip", Created = Created }).Build();

        Errors(new EArkRenderer(package).Render()).Should().BeEmpty();
    }

    [Theory]
    [InlineData(EArkPackage.EContentInformationType.ERMS)]
    [InlineData(EArkPackage.EContentInformationType.SIARD1)]
    [InlineData(EArkPackage.EContentInformationType.SIARD2)]
    [InlineData(EArkPackage.EContentInformationType.SIARDDK)]
    [InlineData(EArkPackage.EContentInformationType.GeoData)]
    [InlineData(EArkPackage.EContentInformationType.MIXED)]
    [InlineData(EArkPackage.EContentInformationType.OTHER)]
    public void Every_content_information_type_we_offer_is_in_the_csip_vocabulary(EArkPackage.EContentInformationType contentInformationType)
    {
        var package = Builder().SetContentInformationType(contentInformationType).AddRepresentationFile(new EArkFile { Href = "representations/rep_1/data/a.zip", Created = Created }).Build();

        Errors(new EArkRenderer(package).Render()).Should().BeEmpty();
    }

    [Theory]
    [InlineData(EArkAgent.ERole.Creator)]
    [InlineData(EArkAgent.ERole.Editor)]
    [InlineData(EArkAgent.ERole.Archivist)]
    [InlineData(EArkAgent.ERole.Preservation)]
    [InlineData(EArkAgent.ERole.Disseminator)]
    [InlineData(EArkAgent.ERole.Custodian)]
    [InlineData(EArkAgent.ERole.Other)]
    public void Every_agent_role_we_offer_is_in_the_mets_vocabulary(EArkAgent.ERole role)
    {
        var agent = new EArkAgent { Role = role, Type = EArkAgent.EType.Organization, Name = "An organisation", OtherRole = role == EArkAgent.ERole.Other ? "SUBMITTER" : null };
        var package = Builder().AddAgent(agent).AddRepresentationFile(new EArkFile { Href = "representations/rep_1/data/a.zip", Created = Created }).Build();

        Errors(new EArkRenderer(package).Render()).Should().BeEmpty();
    }

    [Fact]
    public void Riksarkivets_own_example_package_validates_against_the_same_schemas()
    {
        var document = new XmlDocument();
        document.LoadXml(TestResource.RiksarkivetExampleMets());

        Errors(document).Should().BeEmpty();
    }
}
