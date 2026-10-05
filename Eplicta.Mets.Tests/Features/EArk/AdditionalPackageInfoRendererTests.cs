using System;
using System.Linq;
using System.Xml;
using Eplicta.Mets.Features.EArk;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests.Features.EArk;

public class AdditionalPackageInfoRendererTests
{
    private static readonly DateTime Created = new(2026, 3, 14, 9, 30, 0, DateTimeKind.Utc);

    private static AdditionalPackageInfo Info(Func<AdditionalPackageInfo, AdditionalPackageInfo> configure = null)
    {
        var info = new AdditionalPackageInfo
        {
            Identification = Guid.Parse("9ad11b55-969c-4306-8916-ff7869321e87"),
            CreationDate = Created,
            CreatorName = "Sjofartsverket",
            CreatorCode = "202100005810",
            ArchiveName = "Sjofartsverkets webbarkiv 2026"
        };

        return configure == null ? info : configure(info);
    }

    private static XmlDocument Render(AdditionalPackageInfo info = null, string packageId = "uuid-1")
    {
        return new AdditionalPackageInfoRenderer(info ?? Info(), packageId).Render();
    }

    private static XmlNamespaceManager Namespaces(XmlDocument document)
    {
        var manager = new XmlNamespaceManager(document.NameTable);
        manager.AddNamespace("a", EArkConstants.AdditionalPackageInfoNamespace);
        return manager;
    }

    private static XmlElement Select(XmlDocument document, string xpath)
    {
        return (XmlElement)document.SelectSingleNode(xpath, Namespaces(document));
    }

    [Fact]
    public void The_root_is_in_the_riksarkivet_namespace()
    {
        var root = Render().DocumentElement;

        root!.LocalName.Should().Be("additionalPackageInfo");
        root.NamespaceURI.Should().Be("http://xml.ra.se/e-arkiv/implementation-IP/version10/SNAadditionalPackageInfo");
    }

    [Fact]
    public void The_control_block_carries_the_identification_status_and_creator()
    {
        var document = Render();

        var identification = Select(document, "/a:additionalPackageInfo/a:control/a:identification");
        identification.GetAttribute("type").Should().Be("UUID");
        identification.InnerText.Should().Be("9ad11b55-969c-4306-8916-ff7869321e87");

        Select(document, "/a:additionalPackageInfo/a:control/a:status").InnerText.Should().Be("NEW");
        Select(document, "/a:additionalPackageInfo/a:control/a:creationDate").InnerText.Should().Be("2026-03-14T09:30:00Z");
        Select(document, "/a:additionalPackageInfo/a:control/a:creator/a:creatorName").InnerText.Should().Be("Sjofartsverket");

        var creatorCode = Select(document, "/a:additionalPackageInfo/a:control/a:creator/a:creatorCode");
        creatorCode.GetAttribute("type").Should().Be("ORG");
        creatorCode.InnerText.Should().Be("202100005810");
    }

    [Fact]
    public void The_package_block_carries_the_id_it_was_given()
    {
        Select(Render(packageId: "uuid-abc"), "/a:additionalPackageInfo/a:package").GetAttribute("id").Should().Be("uuid-abc");
    }

    [Fact]
    public void Disposal_and_restrictions_carry_their_flag_and_description()
    {
        var info = Info(x => x with
        {
            Disposable = true,
            DisposalDescription = "Gallras efter 10 ar",
            AccessRestricted = true,
            AccessRestrictDescription = "Sekretess enligt OSL",
            UseRestricted = true,
            UseRestrictDescription = "Upphovsratt"
        });

        var document = Render(info);

        Select(document, "/a:additionalPackageInfo/a:package/a:disposal").GetAttribute("disposable").Should().Be("true");
        Select(document, "/a:additionalPackageInfo/a:package/a:disposal/a:disposalDesc").InnerText.Should().Be("Gallras efter 10 ar");
        Select(document, "/a:additionalPackageInfo/a:package/a:accessRestrict").GetAttribute("restricted").Should().Be("true");
        Select(document, "/a:additionalPackageInfo/a:package/a:useRestrict").GetAttribute("restricted").Should().Be("true");
    }

    [Fact]
    public void Dates_are_written_as_plain_days_not_instants()
    {
        var info = Info(x => x with { StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31) });
        var document = Render(info);

        Select(document, "/a:additionalPackageInfo/a:package/a:startDate").InnerText.Should().Be("2026-01-01");
        Select(document, "/a:additionalPackageInfo/a:package/a:endDate").InnerText.Should().Be("2026-12-31");
    }

    [Fact]
    public void Optional_elements_are_omitted_when_they_have_no_value()
    {
        var document = Render(new AdditionalPackageInfo { CreationDate = Created, CreatorName = "Sjofartsverket" });

        document.SelectSingleNode("/a:additionalPackageInfo/a:package/a:archiveName", Namespaces(document)).Should().BeNull();
        document.SelectSingleNode("/a:additionalPackageInfo/a:package/a:startDate", Namespaces(document)).Should().BeNull();
        document.SelectSingleNode("/a:additionalPackageInfo/a:control/a:creator/a:creatorCode", Namespaces(document)).Should().BeNull();
    }

    [Fact]
    public void The_package_elements_follow_the_order_the_schema_fixes()
    {
        var info = Info(x => x with
        {
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            InformationClass = "Oppen",
            SecurityClassification = "Ingen"
        });

        var names = Select(Render(info), "/a:additionalPackageInfo/a:package").ChildNodes.Cast<XmlNode>().Select(x => x.LocalName).ToArray();

        names.Should().Equal("archiveName", "disposal", "accessRestrict", "useRestrict", "startDate", "endDate", "informationClass", "securityClassification", "audience");
    }

    [Fact]
    public void A_rendered_document_validates_against_the_riksarkivet_schema()
    {
        var info = Info(x => x with
        {
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            InformationClass = "Oppen",
            SecurityClassification = "Ingen",
            DisposalDescription = "Gallras ej",
            AccessRestrictDescription = "Ingen begransning",
            UseRestrictDescription = "Ingen begransning"
        });

        new AdditionalPackageInfoValidator().Validate(Render(info)).Should().BeEmpty();
    }

    [Fact]
    public void A_minimal_document_validates_against_the_riksarkivet_schema()
    {
        var info = new AdditionalPackageInfo { CreationDate = Created, CreatorName = "Sjofartsverket" };

        new AdditionalPackageInfoValidator().Validate(Render(info)).Should().BeEmpty();
    }

    [Theory]
    [InlineData(AdditionalPackageInfo.ECreatorCodeType.VAT)]
    [InlineData(AdditionalPackageInfo.ECreatorCodeType.DUNS)]
    [InlineData(AdditionalPackageInfo.ECreatorCodeType.ORG)]
    [InlineData(AdditionalPackageInfo.ECreatorCodeType.HSA)]
    [InlineData(AdditionalPackageInfo.ECreatorCodeType.Local)]
    [InlineData(AdditionalPackageInfo.ECreatorCodeType.URI)]
    [InlineData(AdditionalPackageInfo.ECreatorCodeType.OTHER)]
    public void Every_creator_code_type_we_offer_is_in_the_schema_vocabulary(AdditionalPackageInfo.ECreatorCodeType creatorCodeType)
    {
        var info = Info(x => x with { CreatorCodeType = creatorCodeType });

        new AdditionalPackageInfoValidator().Validate(Render(info)).Should().BeEmpty();
    }

    [Fact]
    public void Riksarkivets_own_example_validates_against_the_same_schema()
    {
        var document = new XmlDocument();
        document.LoadXml(TestResource.RiksarkivetExampleAdditionalPackageInfo());

        new AdditionalPackageInfoValidator().Validate(document).Should().BeEmpty();
    }
}
