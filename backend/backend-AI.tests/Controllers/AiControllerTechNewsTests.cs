using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend_AI.Controllers;
using backend_AI.DTO.Ai;
using backend_AI.Services.Abstractions;
using backend_AI.Services.External;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend_AI.tests.Controllers
{
    public class AiControllerTechNewsTests : IDisposable
    {
        private readonly Mock<IAiChatService> _mockAiChatService;
        private readonly Mock<IPortfolioRankingService> _mockPortfolioRankingService;
        private readonly Mock<ITechNewsPortfolioClient> _mockTechNewsPortfolioClient;
        private readonly Mock<ILogger<AiController>> _mockLogger;
        private readonly AiController _controller;
        private readonly string _originalAirflowSecret;

        public AiControllerTechNewsTests()
        {
            _mockAiChatService = new Mock<IAiChatService>();
            _mockPortfolioRankingService = new Mock<IPortfolioRankingService>();
            _mockTechNewsPortfolioClient = new Mock<ITechNewsPortfolioClient>();
            _mockLogger = new Mock<ILogger<AiController>>();
            
            var mockPortfolioApiClient = new Mock<backend_AI.Services.External.IPortfolioApiClient>();
            
            _controller = new AiController(
                _mockAiChatService.Object,
                mockPortfolioApiClient.Object,
                _mockPortfolioRankingService.Object,
                _mockLogger.Object,
                _mockTechNewsPortfolioClient.Object
            );

            // Set up environment variable for tests
            _originalAirflowSecret = Environment.GetEnvironmentVariable("AIRFLOW_SECRET");
            Environment.SetEnvironmentVariable("AIRFLOW_SECRET", "test-secret");
        }

        public void Dispose()
        {
            // Restore original environment variable
            if (_originalAirflowSecret != null)
            {
                Environment.SetEnvironmentVariable("AIRFLOW_SECRET", _originalAirflowSecret);
            }
            else
            {
                Environment.SetEnvironmentVariable("AIRFLOW_SECRET", null);
            }
        }

        [Fact]
        public async Task GetLatestTechNews_ShouldReturnOkWithDefaultMessage_WhenNoSummaryExists()
        {
            // Arrange
            _mockTechNewsPortfolioClient.Setup(c => c.GetLatestSummaryAsync()).ReturnsAsync((string?)null);

            // Act
            var result = await _controller.GetLatestTechNews();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.Equal(200, okResult!.StatusCode);
            
            // Use reflection to access the anonymous object property
            var responseType = okResult.Value!.GetType();
            var summaryProperty = responseType.GetProperty("Summary");
            Assert.NotNull(summaryProperty);
            var summaryValue = summaryProperty.GetValue(okResult.Value);
            Assert.Equal("No tech news for now", summaryValue);

            _mockTechNewsPortfolioClient.Verify(c => c.GetLatestSummaryAsync(), Times.Once);
        }

        [Fact]
        public async Task GetLatestTechNews_ShouldReturnOkWithSummary_WhenValidSummaryExists()
        {
            // Arrange
            var rawResponse = "{\"summary\":\"Latest tech news summary\"}";
            _mockTechNewsPortfolioClient.Setup(c => c.GetLatestSummaryAsync()).ReturnsAsync(rawResponse);

            // Act
            var result = await _controller.GetLatestTechNews();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.Equal(200, okResult!.StatusCode);
            
            // Use reflection to access the anonymous object property
            var responseType = okResult.Value!.GetType();
            var summaryProperty = responseType.GetProperty("Summary");
            Assert.NotNull(summaryProperty);
            var summaryValue = summaryProperty.GetValue(okResult.Value);
            Assert.Equal("Latest tech news summary", summaryValue);

            _mockTechNewsPortfolioClient.Verify(c => c.GetLatestSummaryAsync(), Times.Once);
        }

        [Fact]
        public async Task GetLatestTechNews_ShouldReturnOkWithDefaultMessage_WhenSummaryIsEmpty()
        {
            // Arrange
            var rawResponse = "{\"summary\":\"\"}";
            _mockTechNewsPortfolioClient.Setup(c => c.GetLatestSummaryAsync()).ReturnsAsync(rawResponse);

            // Act
            var result = await _controller.GetLatestTechNews();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.Equal(200, okResult!.StatusCode);
            
            // Use reflection to access the anonymous object property
            var responseType = okResult.Value!.GetType();
            var summaryProperty = responseType.GetProperty("Summary");
            Assert.NotNull(summaryProperty);
            var summaryValue = summaryProperty.GetValue(okResult.Value);
            Assert.Equal("No tech news for now", summaryValue);

            _mockTechNewsPortfolioClient.Verify(c => c.GetLatestSummaryAsync(), Times.Once);
        }

        [Fact]
        public async Task GetLatestTechNews_ShouldReturnOkWithDefaultMessage_WhenSummaryIsWhitespace()
        {
            // Arrange
            var rawResponse = "{\"summary\":\"   \"}";
            _mockTechNewsPortfolioClient.Setup(c => c.GetLatestSummaryAsync()).ReturnsAsync(rawResponse);

            // Act
            var result = await _controller.GetLatestTechNews();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.Equal(200, okResult!.StatusCode);
            
            // Use reflection to access the anonymous object property
            var responseType = okResult.Value!.GetType();
            var summaryProperty = responseType.GetProperty("Summary");
            Assert.NotNull(summaryProperty);
            var summaryValue = summaryProperty.GetValue(okResult.Value);
            Assert.Equal("No tech news for now", summaryValue);

            _mockTechNewsPortfolioClient.Verify(c => c.GetLatestSummaryAsync(), Times.Once);
        }

        [Fact]
        public async Task GetLatestTechNews_ShouldReturnOkWithDefaultMessage_WhenJsonParsingFails()
        {
            // Arrange
            var invalidJson = "invalid json content";
            _mockTechNewsPortfolioClient.Setup(c => c.GetLatestSummaryAsync()).ReturnsAsync(invalidJson);

            // Act
            var result = await _controller.GetLatestTechNews();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.Equal(200, okResult!.StatusCode);
            
            // Use reflection to access the anonymous object property
            var responseType = okResult.Value!.GetType();
            var summaryProperty = responseType.GetProperty("Summary");
            Assert.NotNull(summaryProperty);
            var summaryValue = summaryProperty.GetValue(okResult.Value);
            Assert.Equal("No tech news for now", summaryValue);

            _mockTechNewsPortfolioClient.Verify(c => c.GetLatestSummaryAsync(), Times.Once);
        }

        [Fact]
        public async Task GetLatestTechNews_ShouldReturnOkWithDefaultMessage_WhenSummaryPropertyMissing()
        {
            // Arrange
            var jsonWithoutSummary = "{\"other\":\"property\"}";
            _mockTechNewsPortfolioClient.Setup(c => c.GetLatestSummaryAsync()).ReturnsAsync(jsonWithoutSummary);

            // Act
            var result = await _controller.GetLatestTechNews();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.Equal(200, okResult!.StatusCode);
            
            // Use reflection to access the anonymous object property
            var responseType = okResult.Value!.GetType();
            var summaryProperty = responseType.GetProperty("Summary");
            Assert.NotNull(summaryProperty);
            var summaryValue = summaryProperty.GetValue(okResult.Value);
            Assert.Equal("No tech news for now", summaryValue);

            _mockTechNewsPortfolioClient.Verify(c => c.GetLatestSummaryAsync(), Times.Once);
        }

        [Fact]
        public async Task GetLatestTechNews_ShouldReturn500_WhenPortfolioClientThrowsException()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Portfolio service error");
            _mockTechNewsPortfolioClient.Setup(c => c.GetLatestSummaryAsync()).ThrowsAsync(expectedException);

            // Act
            var result = await _controller.GetLatestTechNews();

            // Assert
            Assert.IsType<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(500, objectResult!.StatusCode);
            
            // Use reflection to access the anonymous object property
            var responseType = objectResult.Value!.GetType();
            var errorProperty = responseType.GetProperty("error");
            Assert.NotNull(errorProperty);
            var errorValue = errorProperty.GetValue(objectResult.Value);
            Assert.Equal("Failed to retrieve tech news summary", errorValue);

            _mockTechNewsPortfolioClient.Verify(c => c.GetLatestSummaryAsync(), Times.Once);
        }

        [Fact]
        public async Task GetLatestTechNews_ShouldReturn500_WhenPortfolioClientThrowsHttpRequestException()
        {
            // Arrange
            var expectedException = new HttpRequestException("Network error");
            _mockTechNewsPortfolioClient.Setup(c => c.GetLatestSummaryAsync()).ThrowsAsync(expectedException);

            // Act
            var result = await _controller.GetLatestTechNews();

            // Assert
            Assert.IsType<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(500, objectResult!.StatusCode);
            
            // Use reflection to access the anonymous object property
            var responseType = objectResult.Value!.GetType();
            var errorProperty = responseType.GetProperty("error");
            Assert.NotNull(errorProperty);
            var errorValue = errorProperty.GetValue(objectResult.Value);
            Assert.Equal("Failed to retrieve tech news summary", errorValue);

            _mockTechNewsPortfolioClient.Verify(c => c.GetLatestSummaryAsync(), Times.Once);
        }

        [Fact]
        public async Task UpsertTechNews_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var request = new TechNewsSummaryDto
            {
                Summary = "Test tech news summary",
                WorkflowCompleted = true
            };

            // Set up the controller context with the authorization header
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "Bearer test-secret";
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            _mockTechNewsPortfolioClient.Setup(c => c.UpsertSummaryAsync(It.IsAny<TechNewsSummaryDto>())).ReturnsAsync(true);

            // Act
            var result = await _controller.UpsertTechNews(request);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.Equal(200, okResult!.StatusCode);
            
            // Use reflection to access the anonymous object property
            var responseType = okResult.Value!.GetType();
            var messageProperty = responseType.GetProperty("message");
            Assert.NotNull(messageProperty);
            var messageValue = messageProperty.GetValue(okResult.Value);
            Assert.Equal("Tech news summary forwarded to portfolio service", messageValue);

            _mockTechNewsPortfolioClient.Verify(c => c.UpsertSummaryAsync(request), Times.Once);
        }

        [Fact]
        public async Task UpsertTechNews_ShouldReturn500_WhenPortfolioClientReturnsFalse()
        {
            // Arrange
            var request = new TechNewsSummaryDto
            {
                Summary = "Test tech news summary",
                WorkflowCompleted = false
            };

            // Set up the controller context with the authorization header
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "Bearer test-secret";
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            _mockTechNewsPortfolioClient.Setup(c => c.UpsertSummaryAsync(It.IsAny<TechNewsSummaryDto>())).ReturnsAsync(false);

            // Act
            var result = await _controller.UpsertTechNews(request);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(500, objectResult!.StatusCode);
            
            // Use reflection to access the anonymous object property
            var responseType = objectResult.Value!.GetType();
            var errorProperty = responseType.GetProperty("error");
            Assert.NotNull(errorProperty);
            var errorValue = errorProperty.GetValue(objectResult.Value);
            Assert.Equal("Failed to store tech news summary", errorValue);

            _mockTechNewsPortfolioClient.Verify(c => c.UpsertSummaryAsync(request), Times.Once);
        }

        [Fact]
        public async Task UpsertTechNews_ShouldReturn500_WhenPortfolioClientThrowsException()
        {
            // Arrange
            var request = new TechNewsSummaryDto
            {
                Summary = "Test tech news summary",
                WorkflowCompleted = true
            };

            // Set up the controller context with the authorization header
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "Bearer test-secret";
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            var expectedException = new InvalidOperationException("Portfolio service error");
            _mockTechNewsPortfolioClient.Setup(c => c.UpsertSummaryAsync(It.IsAny<TechNewsSummaryDto>())).ThrowsAsync(expectedException);

            // Act
            var result = await _controller.UpsertTechNews(request);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(500, objectResult!.StatusCode);
            
            // Use reflection to access the anonymous object property
            var responseType = objectResult.Value!.GetType();
            var errorProperty = responseType.GetProperty("error");
            Assert.NotNull(errorProperty);
            var errorValue = errorProperty.GetValue(objectResult.Value);
            Assert.Equal("Internal server error", errorValue);

            _mockTechNewsPortfolioClient.Verify(c => c.UpsertSummaryAsync(It.IsAny<TechNewsSummaryDto>()), Times.Once);
        }

        [Fact]
        public async Task UpsertTechNews_ShouldReturn400_WhenRequestIsNull()
        {
            // Arrange
            TechNewsSummaryDto? request = null;
            
            // Set up the controller context with the authorization header
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "Bearer test-secret";
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.UpsertTechNews(request!);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.Equal(400, badRequestResult!.StatusCode);
            
            // Use reflection to access the anonymous object property
            var responseType = badRequestResult.Value!.GetType();
            var errorProperty = responseType.GetProperty("error");
            Assert.NotNull(errorProperty);
            var errorValue = errorProperty.GetValue(badRequestResult.Value);
            Assert.Equal("Request body is required", errorValue);

            _mockTechNewsPortfolioClient.Verify(c => c.UpsertSummaryAsync(It.IsAny<TechNewsSummaryDto>()), Times.Never);
        }

        [Fact]
        public async Task GetLatestTechNews_ShouldHandleComplexJsonResponse()
        {
            // Arrange
            var complexResponse = "{\"summary\":\"<category>AI</category><title><bold>OpenAI</bold></title> releases GPT-5\",\"metadata\":{\"source\":\"techcrunch\"}}";
            _mockTechNewsPortfolioClient.Setup(c => c.GetLatestSummaryAsync()).ReturnsAsync(complexResponse);

            // Act
            var result = await _controller.GetLatestTechNews();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.Equal(200, okResult!.StatusCode);
            
            // Use reflection to access the anonymous object property
            var responseType = okResult.Value!.GetType();
            var summaryProperty = responseType.GetProperty("Summary");
            Assert.NotNull(summaryProperty);
            var summaryValue = summaryProperty.GetValue(okResult.Value);
            Assert.Equal("<category>AI</category><title><bold>OpenAI</bold></title> releases GPT-5", summaryValue);

            _mockTechNewsPortfolioClient.Verify(c => c.GetLatestSummaryAsync(), Times.Once);
        }

        [Fact]
        public async Task GetLatestTechNews_ShouldHandleUnicodeCharacters()
        {
            // Arrange
            var unicodeResponse = "{\"summary\":\"AI news: émojis 🚀 and special chars ñçáéíóú\"}";
            _mockTechNewsPortfolioClient.Setup(c => c.GetLatestSummaryAsync()).ReturnsAsync(unicodeResponse);

            // Act
            var result = await _controller.GetLatestTechNews();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.Equal(200, okResult!.StatusCode);
            
            // Use reflection to access the anonymous object property
            var responseType = okResult.Value!.GetType();
            var summaryProperty = responseType.GetProperty("Summary");
            Assert.NotNull(summaryProperty);
            var summaryValue = summaryProperty.GetValue(okResult.Value);
            Assert.Equal("AI news: émojis 🚀 and special chars ñçáéíóú", summaryValue);

            _mockTechNewsPortfolioClient.Verify(c => c.GetLatestSummaryAsync(), Times.Once);
        }
    }
}
