using System.Xml;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests
{
    public class XmlValidatorTests
    {
        [Fact]
        public void Basic()
        {
            //Arrange
            var document = new XmlDocument();
            document.Load(@"C:\dev\Eplicta\mets\Resources\sample.xml");
            //var root = document.CreateElement("root");
            //var nsAttribute = document.CreateAttribute("xmlns", "xx", "http://www.w3.org/2000/xmlns/");
            //nsAttribute.Value = "AAA";
            //root.Attributes.Append(nsAttribute);

            var schema = new XmlDocument();
            schema.Load(@"C:\dev\Eplicta\mets\Resources\sample.xsd");

            var sut = new XmlValidator();

            //Act
            var result = sut.Validate(document, schema);

            //Assert
            result.Should().BeEmpty();
        }
    }
}