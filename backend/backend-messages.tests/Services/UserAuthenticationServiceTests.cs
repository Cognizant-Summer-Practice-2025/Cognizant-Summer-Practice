using System;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using BackendMessages.Services;
using BackendMessages.Services.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace BackendMessages.Tests.Services
{
    public class UserAuthenticationServiceTests
    {
        private readonly Mock<ILogger<UserAuthenticationService>> _loggerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly HttpClient _httpClient;
        private readonly UserAuthenticationService _service;

        public UserAuthenticationServiceTests()
        {
            _loggerMock = new Mock<ILogger<UserAuthenticationService>>();
            _configurationMock = new Mock<IConfiguration>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpClientFactoryMock.Setup(f => f.CreateClient("UserService")).Returns(_httpClient);
            
            _configurationMock
                .Setup(x => x["UserServiceUrl"])
                .Returns("http://localhost:5200");

            _service = new UserAuthenticationService(_httpClientFactoryMock.Object, _configurationMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ValidateTokenAsync_WithValidToken_ShouldReturnClaimsPrincipal()
        {
            // Arrange
            var token = "valid-token";
            var userInfo = new
            {
                UserId = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser",
                IsAdmin = false
            };

            var jsonResponse = JsonSerializer.Serialize(userInfo);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse)
                });

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().NotBeNull();
            result!.Identity.Should().NotBeNull();
            result.Identity!.Name.Should().Be("testuser");
            result.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "test@example.com");
            result.Claims.Should().Contain(c => c.Type == "IsAdmin" && c.Value == "False");
        }

        [Fact]
        public async Task ValidateTokenAsync_WithInvalidToken_ShouldReturnNull()
        {
            // Arrange
            var token = "invalid-token";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Unauthorized));

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithNullResponse_ShouldReturnNull()
        {
            // Arrange
            var token = "valid-token";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("null")
                });

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithInvalidJson_ShouldReturnNull()
        {
            // Arrange
            var token = "valid-token";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("invalid json")
                });

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ValidateTokenAsync_WhenHttpExceptionOccurs_ShouldReturnNull()
        {
            // Arrange
            var token = "valid-token";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithAdminUser_ShouldIncludeAdminClaim()
        {
            // Arrange
            var token = "admin-token";
            var userInfo = new
            {
                UserId = Guid.NewGuid(),
                Email = "admin@example.com",
                Username = "admin",
                IsAdmin = true
            };

            var jsonResponse = JsonSerializer.Serialize(userInfo);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse)
                });

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().NotBeNull();
            result!.Claims.Should().Contain(c => c.Type == "IsAdmin" && c.Value == "True");
        }

        [Fact]
        public async Task ValidateTokenAsync_WithCustomUserServiceUrl_ShouldUseCustomUrl()
        {
            // Arrange
            var token = "valid-token";
            var customUrl = "http://custom-user-service:5200";
            var userInfo = new
            {
                UserId = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser",
                IsAdmin = false
            };

            _configurationMock
                .Setup(x => x["UserServiceUrl"])
                .Returns(customUrl);

            var jsonResponse = JsonSerializer.Serialize(userInfo);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().StartsWith(customUrl)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse)
                });

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithNullUserServiceUrl_ShouldUseDefaultUrl()
        {
            // Arrange
            var token = "valid-token";
            var userInfo = new
            {
                UserId = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser",
                IsAdmin = false
            };

            _configurationMock
                .Setup(x => x["UserServiceUrl"])
                .Returns((string?)null);

            var jsonResponse = JsonSerializer.Serialize(userInfo);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().StartsWith("http://localhost:5200")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse)
                });

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new UserAuthenticationService(null!, _configurationMock.Object, _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new UserAuthenticationService(_httpClientFactoryMock.Object, null!, _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new UserAuthenticationService(_httpClientFactoryMock.Object, _configurationMock.Object, null!));
        }
    }
}
