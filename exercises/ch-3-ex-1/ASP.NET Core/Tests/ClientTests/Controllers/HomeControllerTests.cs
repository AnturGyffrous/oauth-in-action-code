using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Io;

using AutoFixture;
using AutoFixture.AutoMoq;

using Client;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

using Moq;

using Xunit;

namespace ClientTests.Controllers
{
    public class HomeControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        public HomeControllerTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _fixture.Freeze<Mock<HttpMessageHandler>>();
        }

        private readonly WebApplicationFactory<Startup> _factory;

        private readonly IFixture _fixture;

        [Theory]
        [InlineData("/Home/Authorize")]
        [InlineData("/Home/Authorize/")]
        public async Task AuthorizeShouldRedirectToIndex(string requestUri)
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            // Act
            var response = await client.GetAsync(requestUri);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Should()
                    .StartWith("http://localhost:9001/authorize")
                    .And.Contain("response_type=code")
                    .And.Contain("client_id=oauth-client-1")
                    .And.ContainEquivalentOf(
                        $"redirect_uri={UrlEncoder.Default.Encode("http://localhost:9000/callback")}");
        }

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

        [Fact]
        public async Task AuthorizeShouldReturnMethodNotAllowedWhenPostVerbIsUsed()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsync("/Home/Authorize", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
        }

        [Fact]
        public async Task CallbackShouldRedirectToOriginalRequestUri()
        {
            // Arrange
            const string requestUri = "/Home/Authorize";
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            await client.GetAsync(requestUri);

            // Act
            var response = await client.GetAsync($"/callback?code={Guid.NewGuid():N}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Should().Be(requestUri);
        }

        [Fact]
        public async Task IndexShouldReturnMethodNotAllowedWhenPostVerbIsUsed()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsync("", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
        }
    }
}