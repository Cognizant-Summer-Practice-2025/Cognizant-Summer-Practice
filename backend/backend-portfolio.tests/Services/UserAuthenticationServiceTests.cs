using Xunit;
using Moq;
using Moq.Protected;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using backend_portfolio.Services;
using backend_portfolio.Services.Abstractions;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace backend_portfolio.tests.Services
{
    public class UserAuthenticationServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<UserAuthenticationService>> _mockLogger;
        private readonly UserAuthenticationService _service;
        private readonly HttpClient _httpClient;

        public UserAuthenticationServiceTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<UserAuthenticationService>>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            
            _mockConfiguration.Setup(x => x["UserServiceUrl"]).Returns("http://localhost:5200");

            _service = new UserAuthenticationService(
                _httpClient,
                _mockConfiguration.Object,
                _mockLogger.Object
            );
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenAllParametersAreValid()
        {
            // Act
            var service = new UserAuthenticationService(
                _httpClient,
                _mockConfiguration.Object,
                _mockLogger.Object
            );

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenHttpClientIsNull()
        {
            // Act & Assert
            var action = () => new UserAuthenticationService(
                null!,
                _mockConfiguration.Object,
                _mockLogger.Object
            );

            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("httpClient");
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
        {
            // Act & Assert
            var action = () => new UserAuthenticationService(
                _httpClient,
                null!,
                _mockLogger.Object
            );

            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("configuration");
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
        {
            // Act & Assert
            var action = () => new UserAuthenticationService(
                _httpClient,
                _mockConfiguration.Object,
                null!
            );

            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        #endregion

        #region ValidateTokenAsync Tests

        [Fact]
        public async Task ValidateTokenAsync_ShouldReturnClaimsPrincipal_WhenTokenIsValid()
        {
            // Arrange
            var token = "valid-token";
            var userInfo = new
            {
                userId = Guid.NewGuid().ToString(),
                email = "test@example.com",
                username = "testuser",
                isAdmin = true
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userInfo))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().NotBeNull();
            result!.Identity.Should().NotBeNull();
            result.Identity!.IsAuthenticated.Should().BeTrue();
            result.Claims.Should().HaveCount(4);
            result.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userInfo.userId);
            result.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == userInfo.email);
            result.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == userInfo.username);
            result.Claims.Should().Contain(c => c.Type == "IsAdmin" && c.Value == "True");
        }

        [Fact]
        public async Task ValidateTokenAsync_ShouldReturnNull_WhenTokenIsInvalid()
        {
            // Arrange
            var token = "invalid-token";
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().BeNull();

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Token validation failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ValidateTokenAsync_ShouldReturnNull_WhenHttpRequestFails()
        {
            // Arrange
            var token = "valid-token";
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ValidateTokenAsync_ShouldReturnNull_WhenJsonDeserializationFails()
        {
            // Arrange
            var token = "valid-token";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("invalid json")
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().BeNull();

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error validating token")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ValidateTokenAsync_ShouldReturnNull_WhenUserInfoIsNull()
        {
            // Arrange
            var token = "valid-token";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null")
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ValidateTokenAsync_ShouldReturnNull_WhenHttpClientThrowsException()
        {
            // Arrange
            var token = "valid-token";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().BeNull();

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error validating token")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ValidateTokenAsync_ShouldUseDefaultUserServiceUrl_WhenConfigurationIsNull()
        {
            // Arrange
            var token = "valid-token";
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(x => x["UserServiceUrl"]).Returns((string?)null);

            var service = new UserAuthenticationService(
                _httpClient,
                mockConfig.Object,
                _mockLogger.Object
            );

            var userInfo = new
            {
                userId = Guid.NewGuid().ToString(),
                email = "test@example.com",
                username = "testuser",
                isAdmin = false
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userInfo))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await service.ValidateTokenAsync(token);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task ValidateTokenAsync_ShouldHandleEmptyToken()
        {
            // Arrange
            var token = "";
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ValidateTokenAsync_ShouldHandleNullToken()
        {
            // Arrange
            string? token = null;
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.ValidateTokenAsync(token!);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ValidateTokenAsync_ShouldHandleUserInfoWithMissingProperties()
        {
            // Arrange
            var token = "valid-token";
            var userInfo = new
            {
                userId = Guid.NewGuid().ToString()
                // Missing email, username, isAdmin
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userInfo))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().NotBeNull();
            result!.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier);
            result.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "");
            result.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "");
            result.Claims.Should().Contain(c => c.Type == "IsAdmin" && c.Value == "False");
        }

        [Fact]
        public async Task ValidateTokenAsync_ShouldHandleUserInfoWithNullValues()
        {
            // Arrange
            var token = "valid-token";
            var userInfo = new
            {
                userId = Guid.NewGuid().ToString(),
                email = (string?)null,
                username = (string?)null,
                isAdmin = false
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userInfo))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().NotBeNull();
            result!.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "");
            result.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "");
        }

        [Fact]
        public async Task ValidateTokenAsync_ShouldHandleUserInfoWithEmptyUserId()
        {
            // Arrange
            var token = "valid-token";
            var userInfo = new
            {
                userId = "",
                email = "test@example.com",
                username = "testuser",
                isAdmin = false
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userInfo))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().NotBeNull();
            result!.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "");
        }

        [Fact]
        public async Task ValidateTokenAsync_ShouldHandleUserInfoWithInvalidUserId()
        {
            // Arrange
            var token = "valid-token";
            var userInfo = new
            {
                userId = "invalid-guid",
                email = "test@example.com",
                username = "testuser",
                isAdmin = false
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userInfo))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().NotBeNull();
            result!.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "invalid-guid");
        }

        #endregion

        #region Edge Cases and Error Handling

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.TooManyRequests)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        public async Task ValidateTokenAsync_ShouldReturnNull_ForVariousErrorStatusCodes(HttpStatusCode statusCode)
        {
            // Arrange
            var token = "valid-token";
            var response = new HttpResponseMessage(statusCode);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ValidateTokenAsync_ShouldHandleTimeoutException()
        {
            // Arrange
            var token = "valid-token";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new TaskCanceledException("Request timeout"));

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ValidateTokenAsync_ShouldHandleSocketException()
        {
            // Arrange
            var token = "valid-token";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new System.Net.Sockets.SocketException());

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ValidateTokenAsync_ShouldHandleJsonException()
        {
            // Arrange
            var token = "valid-token";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ invalid json }")
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ValidateTokenAsync_ShouldHandleEmptyResponseContent()
        {
            // Arrange
            var token = "valid-token";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("")
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ValidateTokenAsync_ShouldHandleWhitespaceResponseContent()
        {
            // Arrange
            var token = "valid-token";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("   ")
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region Concurrent Access Tests

        [Fact]
        public async Task ValidateTokenAsync_ShouldHandleConcurrentRequests()
        {
            // Arrange
            var token = "valid-token";
            var userInfo = new
            {
                userId = Guid.NewGuid().ToString(),
                email = "test@example.com",
                username = "testuser",
                isAdmin = false
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userInfo))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var tasks = Enumerable.Range(0, 10)
                .Select(_ => _service.ValidateTokenAsync(token))
                .ToArray();

            var results = await Task.WhenAll(tasks);

            // Assert
            results.Should().HaveCount(10);
            results.Should().AllSatisfy(result => result.Should().NotBeNull());
        }

        [Fact]
        public async Task ValidateTokenAsync_ShouldHandleMixedConcurrentRequests()
        {
            // Arrange
            var validToken = "valid-token";
            var invalidToken = "invalid-token";

            var userInfo = new
            {
                userId = Guid.NewGuid().ToString(),
                email = "test@example.com",
                username = "testuser",
                isAdmin = false
            };

            var validResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userInfo))
            };

            var invalidResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Headers.Authorization != null && req.Headers.Authorization.Parameter == validToken),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(validResponse);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Headers.Authorization != null && req.Headers.Authorization.Parameter == invalidToken),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(invalidResponse);

            // Act
            var validTasks = Enumerable.Range(0, 5)
                .Select(_ => _service.ValidateTokenAsync(validToken))
                .ToArray();

            var invalidTasks = Enumerable.Range(0, 5)
                .Select(_ => _service.ValidateTokenAsync(invalidToken))
                .ToArray();

            var allTasks = validTasks.Concat(invalidTasks).ToArray();
            var results = await Task.WhenAll(allTasks);

            // Assert
            results.Should().HaveCount(10);
            results.Take(5).Should().AllSatisfy(result => result.Should().NotBeNull());
            results.Skip(5).Should().AllSatisfy(result => result.Should().BeNull());
        }

        #endregion

        #region Performance Tests

        [Fact]
        public async Task ValidateTokenAsync_ShouldCompleteWithinReasonableTime()
        {
            // Arrange
            var token = "valid-token";
            var userInfo = new
            {
                userId = Guid.NewGuid().ToString(),
                email = "test@example.com",
                username = "testuser",
                isAdmin = false
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userInfo))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = await _service.ValidateTokenAsync(token);
            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should complete within 1 second
        }

        #endregion

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
} 