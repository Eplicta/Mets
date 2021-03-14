using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests
{
    public class RendererTests
    {
        [Fact]
        public void Basic()
        {
            //Arrange
            var sut = new Renderer();

            //Act
            var result = sut.Render();

            //Assert
            result.Should().NotBeNull();
        }
    }
}