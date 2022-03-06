using AutoFixture;
using Eplicta.Html.Entities;
using FluentAssertions;
using Xunit;

namespace Eplicta.Html.Tests;

public class RendererTests
{
    [Fact]
    public void Basic()
    {
        //Arrange
        var fixture = new Fixture();
        var htmlTemplate = new HtmlTemplate { Root = fixture.Build<HtmlTemplate.Element>().Without(x => x.Children).Create() };
        var htmlData = fixture.Build<HtmlData>().Create();
        var sut = new Renderer(htmlTemplate, htmlData);

        //Act
        var result = sut.Render();

        //Assert
        result.Should().NotBeNull();
    }
}