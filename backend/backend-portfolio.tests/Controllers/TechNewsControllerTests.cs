using System;
using System.Threading.Tasks;
using backend_portfolio.Controllers;
using backend_portfolio.DTO.TechNews;
using backend_portfolio.Models;
using backend_portfolio.Services.Abstractions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend_portfolio.tests.Controllers
{
    public class TechNewsControllerTests
    {
        private readonly Mock<ITechNewsSummaryService> _mockService;
        private readonly Mock<ILogger<TechNewsController>> _mockLogger;
        private readonly TechNewsController _controller;

        private void SetupServiceToServiceCall()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Service-Name"] = "backend-AI";
            httpContext.Request.Host = new HostString("localhost");
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
        }

        public TechNewsControllerTests()
        {
            _mockService = new Mock<ITechNewsSummaryService>();
            _mockLogger = new Mock<ILogger<TechNewsController>>();
            _controller = new TechNewsController(_mockService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetLatest_ShouldReturnOkWithDefaultMessage_WhenNoSummaryExists()
        {
            // Arrange
            _mockService.Setup(s => s.GetLatestAsync()).ReturnsAsync((TechNewsSummaryResponseDto?)null);

            // Act
            var result = await _controller.GetLatest();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(new { Summary = "No tech news for now" });

            _mockService.Verify(s => s.GetLatestAsync(), Times.Once);
        }

        [Fact]
        public async Task GetLatest_ShouldReturnOkWithSummary_WhenSummaryExists()
        {
            // Arrange
            var summary = new TechNewsSummaryResponseDto
            {
                Id = Guid.NewGuid(),
                Summary = "Test tech news summary",
                WorkflowCompleted = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            };

            _mockService.Setup(s => s.GetLatestAsync()).ReturnsAsync(summary);

            // Act
            var result = await _controller.GetLatest();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(new { Summary = summary.Summary });

            _mockService.Verify(s => s.GetLatestAsync(), Times.Once);
        }

        [Fact]
        public async Task GetLatest_ShouldReturn500_WhenServiceThrowsException()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Service error");
            _mockService.Setup(s => s.GetLatestAsync()).ThrowsAsync(expectedException);

            // Act
            var result = await _controller.GetLatest();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeEquivalentTo(new { error = "Internal server error" });

            _mockService.Verify(s => s.GetLatestAsync(), Times.Once);
        }

        [Fact]
        public async Task PostSummary_ShouldReturnOk_WhenValidRequestIsProvided()
        {
            // Arrange
            var request = new TechNewsSummaryRequestDto
            {
                Summary = "Valid tech news summary",
                WorkflowCompleted = true
            };

            var response = new TechNewsSummaryResponseDto
            {
                Id = Guid.NewGuid(),
                Summary = request.Summary,
                WorkflowCompleted = request.WorkflowCompleted,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockService.Setup(s => s.UpsertAsync(request)).ReturnsAsync(response);
            SetupServiceToServiceCall();

            // Act
            var result = await _controller.PostSummary(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(new { message = "Tech news summary received and stored successfully" });

            _mockService.Verify(s => s.UpsertAsync(It.IsAny<TechNewsSummaryRequestDto>()), Times.Once);
        }

        [Fact]
        public async Task PostSummary_ShouldReturnBadRequest_WhenSummaryIsEmpty()
        {
            // Arrange
            var request = new TechNewsSummaryRequestDto
            {
                Summary = "",
                WorkflowCompleted = false
            };

            SetupServiceToServiceCall();

            // Act
            var result = await _controller.PostSummary(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeEquivalentTo(new { error = "Summary content cannot be empty" });

            _mockService.Verify(s => s.UpsertAsync(It.IsAny<TechNewsSummaryRequestDto>()), Times.Never);
        }

        [Fact]
        public async Task PostSummary_ShouldReturnBadRequest_WhenSummaryIsWhitespace()
        {
            // Arrange
            var request = new TechNewsSummaryRequestDto
            {
                Summary = "   ",
                WorkflowCompleted = false
            };

            SetupServiceToServiceCall();

            // Act
            var result = await _controller.PostSummary(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeEquivalentTo(new { error = "Summary content cannot be empty" });

            _mockService.Verify(s => s.UpsertAsync(It.IsAny<TechNewsSummaryRequestDto>()), Times.Never);
        }

        [Fact]
        public async Task PostSummary_ShouldReturnBadRequest_WhenSummaryIsNull()
        {
            // Arrange
            var request = new TechNewsSummaryRequestDto
            {
                Summary = null!,
                WorkflowCompleted = false
            };

            SetupServiceToServiceCall();

            // Act
            var result = await _controller.PostSummary(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeEquivalentTo(new { error = "Summary content cannot be empty" });

            _mockService.Verify(s => s.UpsertAsync(It.IsAny<TechNewsSummaryRequestDto>()), Times.Never);
        }

        [Fact]
        public async Task PostSummary_ShouldReturn500_WhenServiceThrowsArgumentException()
        {
            // Arrange
            var request = new TechNewsSummaryRequestDto
            {
                Summary = "Valid summary",
                WorkflowCompleted = false
            };

            var expectedException = new ArgumentException("Invalid request", "request");
            _mockService.Setup(s => s.UpsertAsync(It.IsAny<TechNewsSummaryRequestDto>())).ThrowsAsync(expectedException);
            SetupServiceToServiceCall();

            // Act
            var result = await _controller.PostSummary(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeEquivalentTo(new { error = expectedException.Message });

            _mockService.Verify(s => s.UpsertAsync(It.IsAny<TechNewsSummaryRequestDto>()), Times.Once);
        }

        [Fact]
        public async Task PostSummary_ShouldReturn500_WhenServiceThrowsOtherException()
        {
            // Arrange
            var request = new TechNewsSummaryRequestDto
            {
                Summary = "Valid summary",
                WorkflowCompleted = false
            };

            var expectedException = new InvalidOperationException("Service error");
            _mockService.Setup(s => s.UpsertAsync(It.IsAny<TechNewsSummaryRequestDto>())).ThrowsAsync(expectedException);
            SetupServiceToServiceCall();

            // Act
            var result = await _controller.PostSummary(request);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeEquivalentTo(new { error = "Internal server error" });

            _mockService.Verify(s => s.UpsertAsync(It.IsAny<TechNewsSummaryRequestDto>()), Times.Once);
        }

        [Fact]
        public async Task PostSummary_ShouldReturn500_WhenRequestIsNull()
        {
            // Arrange
            TechNewsSummaryRequestDto? request = null;

            SetupServiceToServiceCall();

            // Act
            var result = await _controller.PostSummary(request!);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeEquivalentTo(new { error = "Request body is required" });

            _mockService.Verify(s => s.UpsertAsync(It.IsAny<TechNewsSummaryRequestDto>()), Times.Never);
        }
    }
}
