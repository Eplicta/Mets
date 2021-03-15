using System.Linq;
using System.Xml;
using System.Xml.Schema;
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
            var document = Resource.Get("sample.xml");
            var schema = Resource.Get("sample.xsd");

            var sut = new XmlValidator();

            //Act
            var result = sut.Validate(document, schema);

            //Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void Invalid()
        {
            //Arrange
            var document = new XmlDocument();
            var root = document.CreateElement("root");
            document.AppendChild(root);
            root.SetAttribute("xmlns", "http://www.contoso.com/books");
            var child1 = document.CreateElement("Child");
            root.AppendChild(child1);
            child1.SetAttribute("A", "B");

            var schema = Resource.Get("sample.xsd");

            var sut = new XmlValidator();

            //Act
            var result = sut.Validate(document, schema).ToArray();

            //Assert
            result.Should().NotBeEmpty();
            result.Should().HaveCount(1);
            result.First().Message.Should().Be("The 'http://www.contoso.com/books:root' element is not declared.");
            result.First().XmlSeverityType.Should().Be(XmlSeverityType.Error);
            result.First().XmlSchemaException.Should().NotBeNull();
        }
    }
}