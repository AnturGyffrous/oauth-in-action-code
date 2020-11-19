using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Io;

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
        public async Task IndexShouldReturnWithoutAnAccessTokenWhenNotAuthenticated(string requestUri)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(requestUri);

            // Assert
            var document = await GetDocumentAsync(response);
            document.StatusCode.Should().Be(HttpStatusCode.OK);
            document.ContentType.Should().Be("text/html");
            document.GetElementById("accessToken").Text().Should().Be("NONE");
        }

        private static async Task<IHtmlDocument> GetDocumentAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var document = await BrowsingContext.New().OpenAsync(ResponseFactory, CancellationToken.None);
            return (IHtmlDocument)document;

            void ResponseFactory(VirtualResponse htmlResponse)
            {
                htmlResponse
                    .Address(response.RequestMessage.RequestUri)
                    .Status(response.StatusCode);

                MapHeaders(response.Headers);
                MapHeaders(response.Content.Headers);

                htmlResponse.Content(content);

                void MapHeaders(HttpHeaders headers)
                {
                    foreach (var header in headers)
                    {
                        foreach (var value in header.Value)
                        {
                            htmlResponse.Header(header.Key, value);
                        }
                    }
                }
            }
        }
    }
}