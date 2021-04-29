using System.Threading.Tasks;

using Client;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

using Xunit;

namespace ClientTests.Controllers
{
    public class HomeControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        public HomeControllerTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        private readonly WebApplicationFactory<Startup> _factory;

        [Fact]
        public async Task IndexShouldReturnSuccessStatusCode()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(string.Empty);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
        }
    }
}