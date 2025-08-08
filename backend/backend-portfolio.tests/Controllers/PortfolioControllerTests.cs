using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using backend_portfolio.Controllers;
using backend_portfolio.Services.Abstractions;
using backend_portfolio.DTO.Portfolio.Response;
using backend_portfolio.DTO.Portfolio.Request;
using backend_portfolio.DTO.Pagination;
using backend_portfolio.DTO.Project.Response;
using backend_portfolio.DTO.Experience.Response;
using backend_portfolio.DTO.Skill.Response;
using backend_portfolio.DTO.BlogPost.Response;
using backend_portfolio.DTO.Bookmark.Response;
using backend_portfolio.DTO.PortfolioTemplate.Response;
using AutoFixture;
using AutoFixture.Kernel;

namespace backend_portfolio.tests.Controllers
{
    public class PortfolioControllerTests
    {
        private readonly Mock<IPortfolioQueryService> _mockPortfolioQueryService;
        private readonly Mock<IPortfolioCommandService> _mockPortfolioCommandService;
        private readonly Mock<ILogger<PortfolioController>> _mockLogger;
        private readonly PortfolioController _controller;
        private readonly Fixture _fixture;

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

            _fixture = CreateFixtureWithDateOnlySupport();
        }

        private static Fixture CreateFixtureWithDateOnlySupport()
        {
            var fixture = new Fixture();
            
            // Add custom DateOnly generator
            fixture.Customizations.Add(new DateOnlyGenerator());
            
            return fixture;
        }

        // Custom DateOnly generator for AutoFixture
        private class DateOnlyGenerator : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                if (request is Type type && type == typeof(DateOnly))
                {
                    // Generate a valid date between 1900 and 2100
                    var random = new Random();
                    var year = random.Next(1900, 2100);
                    var month = random.Next(1, 13);
                    var day = random.Next(1, DateTime.DaysInMonth(year, month) + 1);
                    return new DateOnly(year, month, day);
                }
                return new NoSpecimen();
            }
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

        #region CreatePortfolio Tests

        [Fact]
        public async Task CreatePortfolio_ShouldReturnCreatedAtAction_WhenPortfolioCreatedSuccessfully()
        {
            // Arrange
            var request = _fixture.Create<PortfolioCreateRequest>();
            var createdPortfolio = new PortfolioResponse
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                UserId = request.UserId
            };

            _mockPortfolioCommandService.Setup(x => x.CreatePortfolioAsync(request))
                .ReturnsAsync(createdPortfolio);

            // Act
            var result = await _controller.CreatePortfolio(request);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdAtResult = result as CreatedAtActionResult;
            createdAtResult!.ActionName.Should().Be(nameof(PortfolioController.GetPortfolioById));
            createdAtResult.Value.Should().BeEquivalentTo(createdPortfolio);
            _mockPortfolioCommandService.Verify(x => x.CreatePortfolioAsync(request), Times.Once);
        }

        [Fact]
        public async Task CreatePortfolio_ShouldReturnBadRequest_WhenArgumentExceptionThrown()
        {
            // Arrange
            var request = _fixture.Create<PortfolioCreateRequest>();
            var errorMessage = "Invalid portfolio data";

            _mockPortfolioCommandService.Setup(x => x.CreatePortfolioAsync(request))
                .ThrowsAsync(new ArgumentException(errorMessage));

            // Act
            var result = await _controller.CreatePortfolio(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeEquivalentTo(new { message = errorMessage });
        }

        [Fact]
        public async Task CreatePortfolio_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var request = _fixture.Create<PortfolioCreateRequest>();

            _mockPortfolioCommandService.Setup(x => x.CreatePortfolioAsync(request))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreatePortfolio(request);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Internal server error");
        }

        #endregion

        #region CreatePortfolioAndGetId Tests

        [Fact]
        public async Task CreatePortfolioAndGetId_ShouldReturnOkResult_WhenPortfolioCreatedSuccessfully()
        {
            // Arrange
            var request = _fixture.Create<PortfolioCreateRequest>();
            var createdPortfolio = new PortfolioResponse
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                UserId = request.UserId
            };

            _mockPortfolioCommandService.Setup(x => x.CreatePortfolioAndGetIdAsync(request))
                .ReturnsAsync(createdPortfolio);

            // Act
            var result = await _controller.CreatePortfolioAndGetId(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(new { portfolioId = createdPortfolio.Id });
            _mockPortfolioCommandService.Verify(x => x.CreatePortfolioAndGetIdAsync(request), Times.Once);
        }

        [Fact]
        public async Task CreatePortfolioAndGetId_ShouldReturnBadRequest_WhenArgumentExceptionThrown()
        {
            // Arrange
            var request = _fixture.Create<PortfolioCreateRequest>();
            var errorMessage = "Invalid portfolio data";

            _mockPortfolioCommandService.Setup(x => x.CreatePortfolioAndGetIdAsync(request))
                .ThrowsAsync(new ArgumentException(errorMessage));

            // Act
            var result = await _controller.CreatePortfolioAndGetId(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeEquivalentTo(new { message = errorMessage });
        }

        [Fact]
        public async Task CreatePortfolioAndGetId_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var request = _fixture.Create<PortfolioCreateRequest>();

            _mockPortfolioCommandService.Setup(x => x.CreatePortfolioAndGetIdAsync(request))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreatePortfolioAndGetId(request);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Internal server error");
        }

        #endregion

        #region SavePortfolioContent Tests

        [Fact]
        public async Task SavePortfolioContent_ShouldReturnOkResult_WhenContentSavedSuccessfully()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var request = _fixture.Create<BulkPortfolioContentRequest>();
            var response = new BulkPortfolioContentResponse
            {
                Message = "Content saved successfully",
                ProjectsCreated = 2,
                ExperienceCreated = 1,
                SkillsCreated = 3,
                BlogPostsCreated = 1,
                PortfolioPublished = true
            };

            _mockPortfolioCommandService.Setup(x => x.SavePortfolioContentAsync(portfolioId, request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.SavePortfolioContent(portfolioId, request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(response);
            _mockPortfolioCommandService.Verify(x => x.SavePortfolioContentAsync(portfolioId, request), Times.Once);
        }

        [Fact]
        public async Task SavePortfolioContent_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var request = _fixture.Create<BulkPortfolioContentRequest>();

            _mockPortfolioCommandService.Setup(x => x.SavePortfolioContentAsync(portfolioId, request))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.SavePortfolioContent(portfolioId, request);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Internal server error");
        }

        #endregion

        #region UpdatePortfolio Tests

        [Fact]
        public async Task UpdatePortfolio_ShouldReturnOkResult_WhenPortfolioUpdatedSuccessfully()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var request = _fixture.Create<PortfolioUpdateRequest>();
            var updatedPortfolio = new PortfolioResponse
            {
                Id = portfolioId,
                Title = request.Title ?? "Updated Title"
            };

            _mockPortfolioCommandService.Setup(x => x.UpdatePortfolioAsync(portfolioId, request))
                .ReturnsAsync(updatedPortfolio);

            // Act
            var result = await _controller.UpdatePortfolio(portfolioId, request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(updatedPortfolio);
            _mockPortfolioCommandService.Verify(x => x.UpdatePortfolioAsync(portfolioId, request), Times.Once);
        }

        [Fact]
        public async Task UpdatePortfolio_ShouldReturnNotFound_WhenPortfolioDoesNotExist()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var request = _fixture.Create<PortfolioUpdateRequest>();

            _mockPortfolioCommandService.Setup(x => x.UpdatePortfolioAsync(portfolioId, request))
                .ReturnsAsync((PortfolioResponse?)null);

            // Act
            var result = await _controller.UpdatePortfolio(portfolioId, request);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().Be($"Portfolio with ID {portfolioId} not found.");
        }

        [Fact]
        public async Task UpdatePortfolio_ShouldReturnBadRequest_WhenArgumentExceptionThrown()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var request = _fixture.Create<PortfolioUpdateRequest>();
            var errorMessage = "Invalid update data";

            _mockPortfolioCommandService.Setup(x => x.UpdatePortfolioAsync(portfolioId, request))
                .ThrowsAsync(new ArgumentException(errorMessage));

            // Act
            var result = await _controller.UpdatePortfolio(portfolioId, request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeEquivalentTo(new { message = errorMessage });
        }

        [Fact]
        public async Task UpdatePortfolio_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var request = _fixture.Create<PortfolioUpdateRequest>();

            _mockPortfolioCommandService.Setup(x => x.UpdatePortfolioAsync(portfolioId, request))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.UpdatePortfolio(portfolioId, request);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Internal server error");
        }

        #endregion

        #region DeletePortfolio Tests

        [Fact]
        public async Task DeletePortfolio_ShouldReturnNoContent_WhenPortfolioDeletedSuccessfully()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();

            _mockPortfolioCommandService.Setup(x => x.DeletePortfolioAsync(portfolioId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeletePortfolio(portfolioId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockPortfolioCommandService.Verify(x => x.DeletePortfolioAsync(portfolioId), Times.Once);
        }

        [Fact]
        public async Task DeletePortfolio_ShouldReturnNotFound_WhenPortfolioDoesNotExist()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();

            _mockPortfolioCommandService.Setup(x => x.DeletePortfolioAsync(portfolioId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeletePortfolio(portfolioId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().Be($"Portfolio with ID {portfolioId} not found.");
        }

        [Fact]
        public async Task DeletePortfolio_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();

            _mockPortfolioCommandService.Setup(x => x.DeletePortfolioAsync(portfolioId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.DeletePortfolio(portfolioId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Internal server error");
        }

        #endregion

        #region GetPublishedPortfolios Tests

        [Fact]
        public async Task GetPublishedPortfolios_ShouldReturnOkResult_WhenPublishedPortfoliosExist()
        {
            // Arrange
            var portfolios = new List<PortfolioSummaryResponse>
            {
                new PortfolioSummaryResponse { Id = Guid.NewGuid(), Title = "Published Portfolio 1" },
                new PortfolioSummaryResponse { Id = Guid.NewGuid(), Title = "Published Portfolio 2" }
            };

            _mockPortfolioQueryService.Setup(x => x.GetPublishedPortfoliosAsync())
                .ReturnsAsync(portfolios);

            // Act
            var result = await _controller.GetPublishedPortfolios();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(portfolios);
            _mockPortfolioQueryService.Verify(x => x.GetPublishedPortfoliosAsync(), Times.Once);
        }

        [Fact]
        public async Task GetPublishedPortfolios_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            _mockPortfolioQueryService.Setup(x => x.GetPublishedPortfoliosAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetPublishedPortfolios();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Internal server error");
        }

        #endregion

        #region GetPortfoliosForHomePage Tests

        [Fact]
        public async Task GetPortfoliosForHomePage_ShouldReturnOkResult_WhenPortfoliosExist()
        {
            // Arrange
            var portfolioCards = new List<PortfolioCardResponse>
            {
                new PortfolioCardResponse { Id = Guid.NewGuid(), Name = "Home Portfolio 1" },
                new PortfolioCardResponse { Id = Guid.NewGuid(), Name = "Home Portfolio 2" }
            };

            _mockPortfolioQueryService.Setup(x => x.GetPortfoliosForHomePageAsync())
                .ReturnsAsync(portfolioCards);

            // Act
            var result = await _controller.GetPortfoliosForHomePage();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(portfolioCards);
            _mockPortfolioQueryService.Verify(x => x.GetPortfoliosForHomePageAsync(), Times.Once);
        }

        [Fact]
        public async Task GetPortfoliosForHomePage_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            _mockPortfolioQueryService.Setup(x => x.GetPortfoliosForHomePageAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetPortfoliosForHomePage();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Internal server error");
        }

        #endregion

        #region GetPortfoliosForHomePagePaginated Tests

        [Fact]
        public async Task GetPortfoliosForHomePagePaginated_ShouldReturnOkResult_WhenPortfoliosExist()
        {
            // Arrange
            var request = new PaginationRequest { Page = 1, PageSize = 10 };
            var portfolioCards = new PaginatedResponse<PortfolioCardResponse>
            {
                Data = new List<PortfolioCardResponse>
                {
                    new PortfolioCardResponse { Id = Guid.NewGuid(), Name = "Paginated Portfolio 1" },
                    new PortfolioCardResponse { Id = Guid.NewGuid(), Name = "Paginated Portfolio 2" }
                },
                Pagination = new PaginationMetadata
                {
                    CurrentPage = 1,
                    PageSize = 10,
                    TotalCount = 2,
                    TotalPages = 1,
                    HasNext = false,
                    HasPrevious = false
                }
            };

            _mockPortfolioQueryService.Setup(x => x.GetPortfoliosForHomePagePaginatedAsync(request))
                .ReturnsAsync(portfolioCards);

            // Act
            var result = await _controller.GetPortfoliosForHomePagePaginated(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(portfolioCards);
            _mockPortfolioQueryService.Verify(x => x.GetPortfoliosForHomePagePaginatedAsync(request), Times.Once);
        }

        [Fact]
        public async Task GetPortfoliosForHomePagePaginated_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var request = new PaginationRequest { Page = 1, PageSize = 10 };

            _mockPortfolioQueryService.Setup(x => x.GetPortfoliosForHomePagePaginatedAsync(request))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetPortfoliosForHomePagePaginated(request);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Internal server error");
        }

        #endregion

        #region IncrementViewCount Tests

        [Fact]
        public async Task IncrementViewCount_ShouldReturnOkResult_WhenViewCountIncrementedSuccessfully()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();

            _mockPortfolioCommandService.Setup(x => x.IncrementViewCountAsync(portfolioId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.IncrementViewCount(portfolioId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(new { message = "View count incremented successfully" });
            _mockPortfolioCommandService.Verify(x => x.IncrementViewCountAsync(portfolioId), Times.Once);
        }

        [Fact]
        public async Task IncrementViewCount_ShouldReturnNotFound_WhenPortfolioDoesNotExist()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();

            _mockPortfolioCommandService.Setup(x => x.IncrementViewCountAsync(portfolioId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.IncrementViewCount(portfolioId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().Be($"Portfolio with ID {portfolioId} not found.");
        }

        [Fact]
        public async Task IncrementViewCount_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();

            _mockPortfolioCommandService.Setup(x => x.IncrementViewCountAsync(portfolioId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.IncrementViewCount(portfolioId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Internal server error");
        }

        #endregion

        #region IncrementLikeCount Tests

        [Fact]
        public async Task IncrementLikeCount_ShouldReturnOkResult_WhenLikeCountIncrementedSuccessfully()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();

            _mockPortfolioCommandService.Setup(x => x.IncrementLikeCountAsync(portfolioId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.IncrementLikeCount(portfolioId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(new { message = "Like count incremented successfully" });
            _mockPortfolioCommandService.Verify(x => x.IncrementLikeCountAsync(portfolioId), Times.Once);
        }

        [Fact]
        public async Task IncrementLikeCount_ShouldReturnNotFound_WhenPortfolioDoesNotExist()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();

            _mockPortfolioCommandService.Setup(x => x.IncrementLikeCountAsync(portfolioId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.IncrementLikeCount(portfolioId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().Be($"Portfolio with ID {portfolioId} not found.");
        }

        [Fact]
        public async Task IncrementLikeCount_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();

            _mockPortfolioCommandService.Setup(x => x.IncrementLikeCountAsync(portfolioId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.IncrementLikeCount(portfolioId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Internal server error");
        }

        #endregion

        #region DecrementLikeCount Tests

        [Fact]
        public async Task DecrementLikeCount_ShouldReturnOkResult_WhenLikeCountDecrementedSuccessfully()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();

            _mockPortfolioCommandService.Setup(x => x.DecrementLikeCountAsync(portfolioId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DecrementLikeCount(portfolioId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(new { message = "Like count decremented successfully" });
            _mockPortfolioCommandService.Verify(x => x.DecrementLikeCountAsync(portfolioId), Times.Once);
        }

        [Fact]
        public async Task DecrementLikeCount_ShouldReturnNotFound_WhenPortfolioDoesNotExist()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();

            _mockPortfolioCommandService.Setup(x => x.DecrementLikeCountAsync(portfolioId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DecrementLikeCount(portfolioId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().Be($"Portfolio with ID {portfolioId} not found.");
        }

        [Fact]
        public async Task DecrementLikeCount_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();

            _mockPortfolioCommandService.Setup(x => x.DecrementLikeCountAsync(portfolioId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.DecrementLikeCount(portfolioId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Internal server error");
        }

        #endregion

        #region GetUserPortfolioComprehensive Tests

        [Fact]
        public async Task GetUserPortfolioComprehensive_ShouldReturnOkResult_WhenUserPortfolioExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var comprehensiveData = new UserPortfolioComprehensiveResponse
            {
                UserId = userId,
                Portfolios = new List<PortfolioSummaryResponse>(),
                Projects = new List<ProjectResponse>(),
                Experience = new List<ExperienceResponse>(),
                Skills = new List<SkillResponse>(),
                BlogPosts = new List<BlogPostResponse>(),
                Bookmarks = new List<BookmarkResponse>(),
                Templates = new List<PortfolioTemplateSummaryResponse>()
            };

            _mockPortfolioQueryService.Setup(x => x.GetUserPortfolioComprehensiveAsync(userId))
                .ReturnsAsync(comprehensiveData);

            // Act
            var result = await _controller.GetUserPortfolioComprehensive(userId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(comprehensiveData);
            _mockPortfolioQueryService.Verify(x => x.GetUserPortfolioComprehensiveAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserPortfolioComprehensive_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockPortfolioQueryService.Setup(x => x.GetUserPortfolioComprehensiveAsync(userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetUserPortfolioComprehensive(userId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Internal server error");
        }

        #endregion
    }
} 