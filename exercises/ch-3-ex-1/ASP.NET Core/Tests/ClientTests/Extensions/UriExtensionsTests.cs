using System;

using Client.Extensions;

using FluentAssertions;

using Xunit;

namespace ClientTests.Extensions
{
    public class UriExtensionsTests
    {
        [Fact]
        public void AddQueryStringShouldDoNothingIfParameterIsNull()
        {
            // Arrange
            var uri = new Uri("https://www.example.com");

            // Act
            var result = uri.AddQueryString(null);

            // Assert
            result.Should().Be(uri);
        }

        [Fact]
        public void AddQueryStringShouldReflectOverParameterProperties()
        {
            // Arrange
            var uri = new Uri("https://www.example.com");

            // Act
            var result = uri.AddQueryString(new
                { userId = 3, email = "user+test@example.com", password = "Pas://&word=123" });

            // Assert
            result.Should().BeEquivalentTo(new Uri(
                "https://www.example.com?userId=3&email=user%2btest%40example.com&password=Pas%3a%2f%2f%26word%3d123"));
        }
    }
}