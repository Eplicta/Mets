using Eplicta.Mets.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests
{
    public class MetsValidatorTests
    {
        [Fact]
        public void Basic()
        {
            //Arrange
            var document = Resource.GetXml("sample.xml");
            var sut = new MetsValidator();

            //Act
            var result = sut.Validate(document);

            //Assert
            result.Should().BeEmpty();
        }
    }
}