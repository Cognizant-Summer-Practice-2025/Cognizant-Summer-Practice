using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using backend_portfolio.Controllers;
using backend_portfolio.Services.Abstractions;
using backend_portfolio.DTO.Portfolio.Response;
using backend_portfolio.DTO.Portfolio.Request;

namespace backend_portfolio.tests.Controllers
{
    public class PortfolioControllerTests
    {
        private readonly Mock<IPortfolioQueryService> _mockPortfolioQueryService;
        private readonly Mock<IPortfolioCommandService> _mockPortfolioCommandService;
        private readonly Mock<ILogger<PortfolioController>> _mockLogger;
        private readonly PortfolioController _controller;

        public PortfolioControllerTests()
        {
            _mockPortfolioQueryService = new Mock<IPortfolioQueryService>();
            _mockPortfolioCommandService = new Mock<IPortfolioCommandService>();
            _mockLogger = new Mock<ILogger<PortfolioController>>();

            _controller = new PortfolioController(
                _mockPortfolioQueryService.Object,
                _mockPortfolioCommandService.Object,
                _mockLogger.Object
            );
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenPortfolioQueryServiceIsNull()
        {
            // Act
            var controller = new PortfolioController(null!, _mockPortfolioCommandService.Object, _mockLogger.Object);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenPortfolioCommandServiceIsNull()
        {
            // Act
            var controller = new PortfolioController(_mockPortfolioQueryService.Object, null!, _mockLogger.Object);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenLoggerIsNull()
        {
            // Act
            var controller = new PortfolioController(_mockPortfolioQueryService.Object, _mockPortfolioCommandService.Object, null!);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenAllParametersAreValid()
        {
            // Act
            var controller = new PortfolioController(
                _mockPortfolioQueryService.Object,
                _mockPortfolioCommandService.Object,
                _mockLogger.Object
            );

            // Assert
            controller.Should().NotBeNull();
        }

        #endregion

        #region GetAllPortfolios Tests

        [Fact]
        public async Task GetAllPortfolios_ShouldReturnOkResult_WhenPortfoliosExist()
        {
            // Arrange
            var portfolios = new List<PortfolioSummaryResponse>
            {
                new PortfolioSummaryResponse { Id = Guid.NewGuid(), Title = "Portfolio 1" },
                new PortfolioSummaryResponse { Id = Guid.NewGuid(), Title = "Portfolio 2" }
            };

            _mockPortfolioQueryService.Setup(x => x.GetAllPortfoliosAsync())
                .ReturnsAsync(portfolios);

            // Act
            var result = await _controller.GetAllPortfolios();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(portfolios);
            _mockPortfolioQueryService.Verify(x => x.GetAllPortfoliosAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllPortfolios_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            _mockPortfolioQueryService.Setup(x => x.GetAllPortfoliosAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetAllPortfolios();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Internal server error");
        }

        #endregion

        #region GetPortfolioById Tests

        [Fact]
        public async Task GetPortfolioById_ShouldReturnOkResult_WhenPortfolioExists()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var portfolio = new PortfolioDetailResponse
            {
                Id = portfolioId,
                Title = "Test Portfolio",
                UserId = Guid.NewGuid()
            };

            _mockPortfolioQueryService.Setup(x => x.GetPortfolioByIdAsync(portfolioId))
                .ReturnsAsync(portfolio);

            // Act
            var result = await _controller.GetPortfolioById(portfolioId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(portfolio);
            _mockPortfolioQueryService.Verify(x => x.GetPortfolioByIdAsync(portfolioId), Times.Once);
        }

        [Fact]
        public async Task GetPortfolioById_ShouldReturnNotFound_WhenPortfolioDoesNotExist()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            _mockPortfolioQueryService.Setup(x => x.GetPortfolioByIdAsync(portfolioId))
                .ReturnsAsync((PortfolioDetailResponse?)null);

            // Act
            var result = await _controller.GetPortfolioById(portfolioId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().Be($"Portfolio with ID {portfolioId} not found.");
        }

        [Fact]
        public async Task GetPortfolioById_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            _mockPortfolioQueryService.Setup(x => x.GetPortfolioByIdAsync(portfolioId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetPortfolioById(portfolioId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Internal server error");
        }

        #endregion

        #region GetPortfoliosByUserId Tests

        [Fact]
        public async Task GetPortfoliosByUserId_ShouldReturnOkResult_WhenUserHasPortfolios()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolios = new List<PortfolioSummaryResponse>
            {
                new PortfolioSummaryResponse { Id = Guid.NewGuid(), Title = "Portfolio 1" },
                new PortfolioSummaryResponse { Id = Guid.NewGuid(), Title = "Portfolio 2" }
            };

            _mockPortfolioQueryService.Setup(x => x.GetPortfoliosByUserIdAsync(userId))
                .ReturnsAsync(portfolios);

            // Act
            var result = await _controller.GetPortfoliosByUserId(userId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(portfolios);
            _mockPortfolioQueryService.Verify(x => x.GetPortfoliosByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetPortfoliosByUserId_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockPortfolioQueryService.Setup(x => x.GetPortfoliosByUserIdAsync(userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetPortfoliosByUserId(userId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Internal server error");
        }

        #endregion
    }
} 