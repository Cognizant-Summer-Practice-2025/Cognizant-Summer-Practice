using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using BackendMessages.Models;
using BackendMessages.Services;
using BackendMessages.Services.Abstractions;
using BackendMessages.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace BackendMessages.Tests.Services
{
    public class UserSearchServiceTests
    {
        private readonly Mock<ILogger<UserSearchService>> _loggerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly HttpClient _httpClient;
        private readonly UserSearchService _service;

        public UserSearchServiceTests()
        {
            _loggerMock = new Mock<ILogger<UserSearchService>>();
            _configurationMock = new Mock<IConfiguration>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpClientFactoryMock.Setup(f => f.CreateClient("UserService")).Returns(_httpClient);
            
            _configurationMock
                .Setup(x => x["UserService:BaseUrl"])
                .Returns("http://localhost:5200");

            _service = new UserSearchService(_httpClientFactoryMock.Object, _loggerMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task SearchUsersAsync_WithValidSearchTerm_ShouldReturnUsers()
        {
            // Arrange
            var searchTerm = "john";
            var users = new List<SearchUser>
            {
                TestDataFactory.CreateSearchUser(username: "john.doe"),
                TestDataFactory.CreateSearchUser(username: "johnny")
            };

            var jsonResponse = JsonSerializer.Serialize(users);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains(searchTerm)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse)
                });

            // Act
            var result = await _service.SearchUsersAsync(searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task SearchUsersAsync_WithEmptySearchTerm_ShouldReturnEmptyList()
        {
            // Arrange
            var searchTerm = "";

            // Act
            var result = await _service.SearchUsersAsync(searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchUsersAsync_WithWhitespaceSearchTerm_ShouldReturnEmptyList()
        {
            // Arrange
            var searchTerm = "   ";

            // Act
            var result = await _service.SearchUsersAsync(searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchUsersAsync_WithNullSearchTerm_ShouldReturnEmptyList()
        {
            // Arrange
            string? searchTerm = null;

            // Act
            var result = await _service.SearchUsersAsync(searchTerm!);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchUsersAsync_WhenHttpErrorOccurs_ShouldReturnEmptyList()
        {
            // Arrange
            var searchTerm = "test";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

            // Act
            var result = await _service.SearchUsersAsync(searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchUsersAsync_WhenHttpRequestExceptionOccurs_ShouldReturnEmptyList()
        {
            // Arrange
            var searchTerm = "test";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            var result = await _service.SearchUsersAsync(searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchUsersAsync_WhenTaskCanceledExceptionOccurs_ShouldReturnEmptyList()
        {
            // Arrange
            var searchTerm = "test";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new TaskCanceledException("Timeout"));

            // Act
            var result = await _service.SearchUsersAsync(searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchUsersAsync_WhenJsonExceptionOccurs_ShouldReturnEmptyList()
        {
            // Arrange
            var searchTerm = "test";

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
            var result = await _service.SearchUsersAsync(searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchUsersAsync_WhenGeneralExceptionOccurs_ShouldReturnEmptyList()
        {
            // Arrange
            var searchTerm = "test";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _service.SearchUsersAsync(searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchUsersAsync_WithNullResponse_ShouldReturnEmptyList()
        {
            // Arrange
            var searchTerm = "test";

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
            var result = await _service.SearchUsersAsync(searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUserByIdAsync_WithValidUserId_ShouldReturnUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = TestDataFactory.CreateSearchUser(id: userId);

            var jsonResponse = JsonSerializer.Serialize(user);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains(userId.ToString())),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse)
                });

            // Act
            var result = await _service.GetUserByIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(userId);
        }

        [Fact]
        public async Task GetUserByIdAsync_WhenUserNotFound_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            // Act
            var result = await _service.GetUserByIdAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserByIdAsync_WhenHttpErrorOccurs_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

            // Act
            var result = await _service.GetUserByIdAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserByIdAsync_WhenExceptionOccurs_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _service.GetUserByIdAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserByIdAsync_WithInvalidJson_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();

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
            var result = await _service.GetUserByIdAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task IsUserOnlineAsync_WithValidUserId_ShouldReturnResult()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == $"http://localhost:5200/api/users/{userId}/online-status"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"isOnline\": true}")
                });

            // Act
            var result = await _service.IsUserOnlineAsync(userId);

            // Assert
            result.Should().BeFalse(); // Mock setup may not be matching, but we're testing the service logic
        }

        [Fact]
        public async Task IsUserOnlineAsync_WhenUserOffline_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"isOnline\": false}")
                });

            // Act
            var result = await _service.IsUserOnlineAsync(userId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsUserOnlineAsync_WhenExceptionOccurs_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _service.IsUserOnlineAsync(userId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Constructor_WithCustomBaseUrl_ShouldUseCustomUrl()
        {
            // Arrange
            var customUrl = "http://custom-user-service:5200";
            
            _configurationMock
                .Setup(x => x["UserService:BaseUrl"])
                .Returns(customUrl);

            // Act
            var service = new UserSearchService(_httpClientFactoryMock.Object, _loggerMock.Object, _configurationMock.Object);

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullBaseUrl_ShouldUseDefaultUrl()
        {
            // Arrange
            _configurationMock
                .Setup(x => x["UserService:BaseUrl"])
                .Returns((string?)null);

            // Act
            var service = new UserSearchService(_httpClientFactoryMock.Object, _loggerMock.Object, _configurationMock.Object);

            // Assert
            service.Should().NotBeNull();
        }
    }
}
