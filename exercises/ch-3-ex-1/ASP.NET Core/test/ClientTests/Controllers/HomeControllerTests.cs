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

        [Theory]
        [InlineData("")]
        [InlineData("/")]
        [InlineData("/Home")]
        [InlineData("/Home/")]
        [InlineData("/Home/Index")]
        [InlineData("/Home/Index/")]
        public async Task IndexShouldReturnSuccessStatusCode(string requestUri)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(requestUri);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
        }
    }
}