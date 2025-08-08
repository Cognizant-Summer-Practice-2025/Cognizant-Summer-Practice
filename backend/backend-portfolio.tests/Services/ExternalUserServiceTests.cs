using Xunit;
using Moq;
using Moq.Protected;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using backend_portfolio.Services.External;
using backend_portfolio.Services.Abstractions;
using System.Net;
using System.Text.Json;

namespace backend_portfolio.tests.Services
{
    public class ExternalUserServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<ExternalUserService>> _mockLogger;
        private readonly ExternalUserService _service;
        private readonly HttpClient _httpClient;

        public ExternalUserServiceTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<ExternalUserService>>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            
            // Mock the configuration indexer
            _mockConfiguration.Setup(x => x["ExternalServices:UserService:BaseUrl"])
                .Returns("http://localhost:5200");

            _service = new ExternalUserService(
                _httpClient,
                _mockLogger.Object,
                _mockConfiguration.Object
            );
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenAllParametersAreValid()
        {
            // Act
            var service = new ExternalUserService(
                _httpClient,
                _mockLogger.Object,
                _mockConfiguration.Object
            );

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenHttpClientIsNull()
        {
            // Act
            var service = new ExternalUserService(
                null!,
                _mockLogger.Object,
                _mockConfiguration.Object
            );

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenLoggerIsNull()
        {
            // Act
            var service = new ExternalUserService(
                _httpClient,
                null!,
                _mockConfiguration.Object
            );

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenConfigurationIsNull()
        {
            // Act
            var service = new ExternalUserService(
                _httpClient,
                _mockLogger.Object,
                null!
            );

            // Assert
            service.Should().NotBeNull();
        }

        #endregion

        #region GetUserInformationAsync Tests

        [Fact]
        public async Task GetUserInformationAsync_ShouldReturnUserInformation_WhenValidResponseReceived()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userJson = new
            {
                firstName = "John",
                lastName = "Doe",
                professionalTitle = "Software Engineer",
                location = "New York",
                avatarUrl = "https://example.com/avatar.jpg"
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userJson))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.GetUserInformationAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result!.FullName.Should().Be("John Doe");
            result.JobTitle.Should().Be("Software Engineer");
            result.Location.Should().Be("New York");
            result.ProfilePictureUrl.Should().Be("https://example.com/avatar.jpg");
        }

        [Fact]
        public async Task GetUserInformationAsync_ShouldReturnUserInformation_WhenOnlyFirstNameProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userJson = new
            {
                firstName = "John"
                // Missing lastName, professionalTitle, location, avatarUrl
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userJson))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.GetUserInformationAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result!.FullName.Should().Be("John");
            result.JobTitle.Should().Be("Professional");
            result.Location.Should().Be("Remote");
            result.ProfilePictureUrl.Should().Be("/default-avatar.png");
        }

        [Fact]
        public async Task GetUserInformationAsync_ShouldReturnUserInformation_WhenOnlyLastNameProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userJson = new
            {
                lastName = "Doe"
                // Missing firstName, professionalTitle, location, avatarUrl
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userJson))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.GetUserInformationAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result!.FullName.Should().Be("Doe");
            result.JobTitle.Should().Be("Professional");
            result.Location.Should().Be("Remote");
            result.ProfilePictureUrl.Should().Be("/default-avatar.png");
        }

        [Fact]
        public async Task GetUserInformationAsync_ShouldReturnUserInformation_WhenNoNameProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userJson = new
            {
                professionalTitle = "Software Engineer"
                // Missing firstName, lastName, location, avatarUrl
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userJson))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.GetUserInformationAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result!.FullName.Should().Be("Anonymous User");
            result.JobTitle.Should().Be("Software Engineer");
            result.Location.Should().Be("Remote");
            result.ProfilePictureUrl.Should().Be("/default-avatar.png");
        }

        [Fact]
        public async Task GetUserInformationAsync_ShouldReturnUserInformation_WhenEmptyNamesProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userJson = new
            {
                firstName = "",
                lastName = "",
                professionalTitle = "Developer"
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userJson))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.GetUserInformationAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result!.FullName.Should().Be("Anonymous User");
            result.JobTitle.Should().Be("Developer");
        }

        [Fact]
        public async Task GetUserInformationAsync_ShouldReturnUserInformation_WhenWhitespaceNamesProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userJson = new
            {
                firstName = "   ",
                lastName = "   ",
                professionalTitle = "Developer"
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userJson))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.GetUserInformationAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result!.FullName.Should().Be("Anonymous User");
            result.JobTitle.Should().Be("Developer");
        }

        [Fact]
        public async Task GetUserInformationAsync_ShouldReturnNull_WhenHttpRequestFails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.GetUserInformationAsync(userId);

            // Assert
            result.Should().BeNull();

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to fetch user information")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetUserInformationAsync_ShouldReturnNull_WhenHttpClientThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            var result = await _service.GetUserInformationAsync(userId);

            // Assert
            result.Should().BeNull();

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while fetching user information")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetUserInformationAsync_ShouldReturnNull_WhenJsonDeserializationFails()
        {
            // Arrange
            var userId = Guid.NewGuid();
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
            var result = await _service.GetUserInformationAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserInformationAsync_ShouldUseDefaultBaseUrl_WhenConfigurationIsNull()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            var mockSection = new Mock<IConfigurationSection>();
            mockSection.SetupGet(x => x.Value).Returns((string?)null);
            mockConfig.Setup(x => x["ExternalServices:UserService:BaseUrl"]).Returns((string?)null);

            var service = new ExternalUserService(
                _httpClient,
                _mockLogger.Object,
                mockConfig.Object
            );

            var userId = Guid.NewGuid();
            var userJson = new
            {
                firstName = "John",
                lastName = "Doe"
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userJson))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await service.GetUserInformationAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result!.FullName.Should().Be("John Doe");
        }

        [Fact]
        public async Task GetUserInformationAsync_ShouldHandleEmptyGuid()
        {
            // Arrange
            var userId = Guid.Empty;
            var userJson = new
            {
                firstName = "John",
                lastName = "Doe"
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userJson))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.GetUserInformationAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result!.FullName.Should().Be("John Doe");
        }

        #endregion

        #region Edge Cases and Error Handling

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        public async Task GetUserInformationAsync_ShouldReturnNull_ForVariousErrorStatusCodes(HttpStatusCode statusCode)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var response = new HttpResponseMessage(statusCode);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.GetUserInformationAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserInformationAsync_ShouldHandleTimeoutException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new TaskCanceledException("Request timeout"));

            // Act
            var result = await _service.GetUserInformationAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserInformationAsync_ShouldHandleSocketException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new System.Net.Sockets.SocketException());

            // Act
            var result = await _service.GetUserInformationAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserInformationAsync_ShouldHandleEmptyResponseContent()
        {
            // Arrange
            var userId = Guid.NewGuid();
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
            var result = await _service.GetUserInformationAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserInformationAsync_ShouldHandleWhitespaceResponseContent()
        {
            // Arrange
            var userId = Guid.NewGuid();
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
            var result = await _service.GetUserInformationAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserInformationAsync_ShouldHandleNullResponseContent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = null
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.GetUserInformationAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserInformationAsync_ShouldHandleJsonWithNullValues()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userJson = new
            {
                firstName = (string?)null,
                lastName = (string?)null,
                professionalTitle = (string?)null,
                location = (string?)null,
                avatarUrl = (string?)null
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userJson))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.GetUserInformationAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result!.FullName.Should().Be("Anonymous User");
            result.JobTitle.Should().Be("Professional");
            result.Location.Should().Be("Remote");
            result.ProfilePictureUrl.Should().Be("/default-avatar.png");
        }

        [Fact]
        public async Task GetUserInformationAsync_ShouldHandleJsonWithSpecialCharacters()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userJson = new
            {
                firstName = "José",
                lastName = "García-López",
                professionalTitle = "Senior Developer (Full-Stack)",
                location = "San José, Costa Rica",
                avatarUrl = "https://example.com/avatar.jpg?size=150"
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userJson))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _service.GetUserInformationAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result!.FullName.Should().Be("José García-López");
            result.JobTitle.Should().Be("Senior Developer (Full-Stack)");
            result.Location.Should().Be("San José, Costa Rica");
            result.ProfilePictureUrl.Should().Be("https://example.com/avatar.jpg?size=150");
        }

        #endregion

        #region Concurrent Access Tests

        [Fact]
        public async Task GetUserInformationAsync_ShouldHandleConcurrentRequests()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userJson = new
            {
                firstName = "John",
                lastName = "Doe",
                professionalTitle = "Developer"
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userJson))
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
                .Select(_ => _service.GetUserInformationAsync(userId))
                .ToArray();

            var results = await Task.WhenAll(tasks);

            // Assert
            results.Should().HaveCount(10);
            results.Should().AllSatisfy(result => result.Should().NotBeNull());
            results.Should().AllSatisfy(result => result!.FullName.Should().Be("John Doe"));
        }

        [Fact]
        public async Task GetUserInformationAsync_ShouldHandleMixedConcurrentRequests()
        {
            // Arrange
            var validUserId = Guid.NewGuid();
            var invalidUserId = Guid.NewGuid();

            var userJson = new
            {
                firstName = "John",
                lastName = "Doe"
            };

            var validResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userJson))
            };

            var invalidResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains(validUserId.ToString())),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(validResponse);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains(invalidUserId.ToString())),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(invalidResponse);

            // Act
            var validTasks = Enumerable.Range(0, 5)
                .Select(_ => _service.GetUserInformationAsync(validUserId))
                .ToArray();

            var invalidTasks = Enumerable.Range(0, 5)
                .Select(_ => _service.GetUserInformationAsync(invalidUserId))
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
        public async Task GetUserInformationAsync_ShouldCompleteWithinReasonableTime()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userJson = new
            {
                firstName = "John",
                lastName = "Doe"
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userJson))
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
            var result = await _service.GetUserInformationAsync(userId);
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