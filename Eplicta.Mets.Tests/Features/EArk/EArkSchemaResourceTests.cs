using Eplicta.Mets.Features.EArk;
using Eplicta.Mets.Helpers;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests.Features.EArk;

public class EArkSchemaResourceTests
{
    [Theory]
    [InlineData("EArk.mets.xsd")]
    [InlineData("EArk.xlink.xsd")]
    [InlineData("EArk.DILCISExtensionMETS.xsd")]
    [InlineData("EArk.DILCISExtensionSIPMETS.xsd")]
    [InlineData("EArk.SNAadditionalPackageInfo.xsd")]
    public void The_schema_is_embedded_and_parses(string name)
    {
        var document = Resource.GetXml(name);

        document.DocumentElement.Should().NotBeNull();
        document.DocumentElement!.LocalName.Should().Be("schema");
    }

    [Fact]
    public void The_csip_extension_schema_declares_the_namespace_the_renderer_writes()
    {
        var document = Resource.GetXml("EArk.DILCISExtensionMETS.xsd");

        document.DocumentElement!.GetAttribute("targetNamespace").Should().Be(EArkConstants.CsipNamespace);
    }

    [Fact]
    public void The_sip_extension_schema_declares_the_namespace_the_renderer_writes()
    {
        var document = Resource.GetXml("EArk.DILCISExtensionSIPMETS.xsd");

        document.DocumentElement!.GetAttribute("targetNamespace").Should().Be(EArkConstants.SipNamespace);
    }

    [Fact]
    public void The_eark_mets_schema_is_kept_apart_from_the_one_the_existing_renderer_uses()
    {
        Resource.Get("EArk.mets.xsd").Should().NotBe(Resource.Get("mets.xsd"));
    }
}
