using Eplicta.Mets.Features.EArk;
using Xunit;

namespace Eplicta.Mets.Tests.Features.EArk;

public class EplictaPackageTypesTests
{
    [Fact]
    public void Constants_ShouldHaveTheAgreedValues()
    {
        Assert.Equal("Eplicta Web Archive", EplictaPackageTypes.WebArchive);
        Assert.Equal("FortDocs Case Archive", EplictaPackageTypes.CaseArchive);
    }

    [Theory]
    [InlineData("Eplicta Web Archive")]
    [InlineData("eplicta web archive")]
    [InlineData("EPLICTA WEB ARCHIVE")]
    [InlineData("  Eplicta Web Archive  ")]
    [InlineData("\tEplicta Web Archive\r\n")]
    public void Matches_WhenValueEqualsIgnoringCaseAndWhitespace_ShouldBeTrue(string value)
    {
        Assert.True(EplictaPackageTypes.Matches(value, EplictaPackageTypes.WebArchive));
    }

    [Theory]
    [InlineData("FortDocs Case Archive")]
    [InlineData("fortdocs case archive")]
    [InlineData(" FORTDOCS CASE ARCHIVE ")]
    public void Matches_WhenCaseArchiveValueIsDifferentlyWritten_ShouldMatchCaseArchive(string value)
    {
        Assert.True(EplictaPackageTypes.Matches(value, EplictaPackageTypes.CaseArchive));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Something else")]
    [InlineData("Eplicta Web Archiv")]
    public void Matches_WhenValueIsMissingOrDifferent_ShouldBeFalse(string value)
    {
        Assert.False(EplictaPackageTypes.Matches(value, EplictaPackageTypes.WebArchive));
    }

    [Fact]
    public void Matches_WhenWebArchiveIsComparedToCaseArchive_ShouldBeFalse()
    {
        Assert.False(EplictaPackageTypes.Matches(EplictaPackageTypes.WebArchive, EplictaPackageTypes.CaseArchive));
        Assert.False(EplictaPackageTypes.Matches(EplictaPackageTypes.CaseArchive, EplictaPackageTypes.WebArchive));
    }
}
