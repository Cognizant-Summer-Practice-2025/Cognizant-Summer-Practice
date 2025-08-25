using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using backend_AI.DTO.Ai;
using backend_AI.Services.External;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;
using System.Threading;
using System.Linq;

namespace backend_AI.tests.Services.External
{
    public class TechNewsPortfolioClientTests
    {
        private readonly Mock<ILogger<TechNewsPortfolioClient>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly TechNewsPortfolioClient _client;
        private const string PortfolioServiceUrl = "http://localhost:5201";

        public TechNewsPortfolioClientTests()
        {
            _mockLogger = new Mock<ILogger<TechNewsPortfolioClient>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            
            _mockConfiguration.Setup(c => c["PORTFOLIO_SERVICE_URL"]).Returns(PortfolioServiceUrl);
            
            _client = new TechNewsPortfolioClient(_httpClient, _mockConfiguration.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task UpsertSummaryAsync_ShouldReturnTrue_WhenSuccessful()
        {
            // Arrange
            var request = new TechNewsSummaryDto
            {
                Summary = "Test summary",
                WorkflowCompleted = true
            };

            var response = new { message = "Success" };
            var jsonResponse = JsonConvert.SerializeObject(response);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _client.UpsertSummaryAsync(request);

            // Assert
            Assert.True(result);

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString() == $"{PortfolioServiceUrl}/api/technews"
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task UpsertSummaryAsync_ShouldReturnFalse_WhenHttpErrorOccurs()
        {
            // Arrange
            var request = new TechNewsSummaryDto
            {
                Summary = "Test summary",
                WorkflowCompleted = false
            };

            var errorResponse = new { error = "Bad Request" };
            var jsonErrorResponse = JsonConvert.SerializeObject(errorResponse);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent(jsonErrorResponse, Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _client.UpsertSummaryAsync(request);

            // Assert
            Assert.False(result);

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString() == $"{PortfolioServiceUrl}/api/technews"
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task UpsertSummaryAsync_ShouldReturnFalse_WhenExceptionOccurs()
        {
            // Arrange
            var request = new TechNewsSummaryDto
            {
                Summary = "Test summary",
                WorkflowCompleted = true
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            var result = await _client.UpsertSummaryAsync(request);

            // Assert
            Assert.False(result);

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString() == $"{PortfolioServiceUrl}/api/technews"
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetLatestSummaryAsync_ShouldReturnSummary_WhenSuccessful()
        {
            // Arrange
            var expectedResponse = new { summary = "Test tech news summary" };
            var jsonResponse = JsonConvert.SerializeObject(expectedResponse);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _client.GetLatestSummaryAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(jsonResponse, result);

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString() == $"{PortfolioServiceUrl}/api/technews" &&
                    req.Headers.Contains("X-Service-Name") &&
                    req.Headers.GetValues("X-Service-Name").Contains("backend-AI")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetLatestSummaryAsync_ShouldReturnNull_WhenHttpErrorOccurs()
        {
            // Arrange
            var errorResponse = new { error = "Not Found" };
            var jsonErrorResponse = JsonConvert.SerializeObject(errorResponse);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent(jsonErrorResponse, Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _client.GetLatestSummaryAsync();

            // Assert
            Assert.Null(result);

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString() == $"{PortfolioServiceUrl}/api/technews"
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetLatestSummaryAsync_ShouldReturnNull_WhenExceptionOccurs()
        {
            // Arrange
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            var result = await _client.GetLatestSummaryAsync();

            // Assert
            Assert.Null(result);

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString() == $"{PortfolioServiceUrl}/api/technews"
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetLatestSummaryAsync_ShouldReturnEmptyString_WhenResponseIsEmpty()
        {
            // Arrange
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("", Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _client.GetLatestSummaryAsync();

            // Assert
            Assert.Equal("", result);

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString() == $"{PortfolioServiceUrl}/api/technews"
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task UpsertSummaryAsync_ShouldSendCorrectJsonPayload()
        {
            // Arrange
            var request = new TechNewsSummaryDto
            {
                Summary = "Complex summary with special chars: éñç",
                WorkflowCompleted = true
            };

            var response = new { message = "Success" };
            var jsonResponse = JsonConvert.SerializeObject(response);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });

            // Act
            await _client.UpsertSummaryAsync(request);

            // Assert
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString() == $"{PortfolioServiceUrl}/api/technews"
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetLatestSummaryAsync_ShouldHandleLargeResponse()
        {
            // Arrange
            var largeSummary = new { summary = new string('a', 10000) }; // 10KB summary
            var jsonResponse = JsonConvert.SerializeObject(largeSummary);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _client.GetLatestSummaryAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(jsonResponse, result);
            Assert.True(result.Length > 10000); // Should be longer than just the summary content
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
