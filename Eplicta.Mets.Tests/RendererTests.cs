using Eplicta.Mets.Entities;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests
{

    public class RendererTests
    {
        [Fact(Skip = "Fix")]
        public void Basic()
        {
            //Arrange
            var modsData = new ModsData();
            var sut = new Renderer(modsData);

            //Act
            var result = sut.Render();

            //Assert
            result.Should().NotBeNull();
        }
    }
}