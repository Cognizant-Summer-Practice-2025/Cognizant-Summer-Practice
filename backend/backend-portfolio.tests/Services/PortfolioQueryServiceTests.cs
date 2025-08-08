using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using backend_portfolio.Services;
using backend_portfolio.Services.Abstractions;
using backend_portfolio.Services.Mappers;
using backend_portfolio.Repositories;
using backend_portfolio.Models;
using backend_portfolio.DTO.Portfolio.Response;
using backend_portfolio.DTO.Pagination;
using backend_portfolio.tests.Helpers;

namespace backend_portfolio.tests.Services
{
    public class PortfolioQueryServiceTests
    {
        private readonly Mock<IPortfolioRepository> _mockPortfolioRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IExperienceRepository> _mockExperienceRepository;
        private readonly Mock<ISkillRepository> _mockSkillRepository;
        private readonly Mock<IBlogPostRepository> _mockBlogPostRepository;
        private readonly Mock<IBookmarkRepository> _mockBookmarkRepository;
        private readonly Mock<IPortfolioTemplateRepository> _mockTemplateRepository;
        private readonly Mock<IExternalUserService> _mockExternalUserService;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<IPortfolioMapper> _mockPortfolioMapper;
        private readonly Mock<ILogger<PortfolioQueryService>> _mockLogger;
        private readonly PortfolioQueryService _service;

        public PortfolioQueryServiceTests()
        {
            _mockPortfolioRepository = new Mock<IPortfolioRepository>();
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockExperienceRepository = new Mock<IExperienceRepository>();
            _mockSkillRepository = new Mock<ISkillRepository>();
            _mockBlogPostRepository = new Mock<IBlogPostRepository>();
            _mockBookmarkRepository = new Mock<IBookmarkRepository>();
            _mockTemplateRepository = new Mock<IPortfolioTemplateRepository>();
            _mockExternalUserService = new Mock<IExternalUserService>();
            _mockCacheService = new Mock<ICacheService>();
            _mockPortfolioMapper = new Mock<IPortfolioMapper>();
            _mockLogger = new Mock<ILogger<PortfolioQueryService>>();

            _service = new PortfolioQueryService(
                _mockPortfolioRepository.Object,
                _mockProjectRepository.Object,
                _mockExperienceRepository.Object,
                _mockSkillRepository.Object,
                _mockBlogPostRepository.Object,
                _mockBookmarkRepository.Object,
                _mockTemplateRepository.Object,
                _mockExternalUserService.Object,
                _mockCacheService.Object,
                _mockPortfolioMapper.Object,
                _mockLogger.Object
            );
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenAllParametersAreValid()
        {
            // Act
            var service = new PortfolioQueryService(
                _mockPortfolioRepository.Object,
                _mockProjectRepository.Object,
                _mockExperienceRepository.Object,
                _mockSkillRepository.Object,
                _mockBlogPostRepository.Object,
                _mockBookmarkRepository.Object,
                _mockTemplateRepository.Object,
                _mockExternalUserService.Object,
                _mockCacheService.Object,
                _mockPortfolioMapper.Object,
                _mockLogger.Object
            );

            // Assert
            service.Should().NotBeNull();
        }

        #endregion

        #region GetAllPortfoliosAsync Tests

        [Fact]
        public async Task GetAllPortfoliosAsync_ShouldReturnPortfolioSummaries_WhenPortfoliosExist()
        {
            // Arrange
            var portfolios = new List<Portfolio>
            {
                TestDataFactory.CreatePortfolio(),
                TestDataFactory.CreatePortfolio()
            };

            var expectedResponses = new List<PortfolioSummaryResponse>
            {
                new PortfolioSummaryResponse { Id = portfolios[0].Id, Title = portfolios[0].Title },
                new PortfolioSummaryResponse { Id = portfolios[1].Id, Title = portfolios[1].Title }
            };

            _mockPortfolioRepository.Setup(x => x.GetAllPortfoliosAsync())
                .ReturnsAsync(portfolios);
            _mockPortfolioMapper.Setup(x => x.MapToSummaryDtos(portfolios))
                .Returns(expectedResponses);

            // Act
            var result = await _service.GetAllPortfoliosAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedResponses);
            _mockPortfolioRepository.Verify(x => x.GetAllPortfoliosAsync(), Times.Once);
            _mockPortfolioMapper.Verify(x => x.MapToSummaryDtos(portfolios), Times.Once);
        }

        [Fact]
        public async Task GetAllPortfoliosAsync_ShouldReturnEmptyCollection_WhenNoPortfoliosExist()
        {
            // Arrange
            var portfolios = new List<Portfolio>();
            var expectedResponses = new List<PortfolioSummaryResponse>();

            _mockPortfolioRepository.Setup(x => x.GetAllPortfoliosAsync())
                .ReturnsAsync(portfolios);
            _mockPortfolioMapper.Setup(x => x.MapToSummaryDtos(portfolios))
                .Returns(expectedResponses);

            // Act
            var result = await _service.GetAllPortfoliosAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockPortfolioRepository.Verify(x => x.GetAllPortfoliosAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllPortfoliosAsync_ShouldLogErrorAndRethrow_WhenExceptionOccurs()
        {
            // Arrange
            var exception = new Exception("Database error");
            _mockPortfolioRepository.Setup(x => x.GetAllPortfoliosAsync())
                .ThrowsAsync(exception);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<Exception>(
                () => _service.GetAllPortfoliosAsync());

            thrownException.Should().Be(exception);
            _mockPortfolioRepository.Verify(x => x.GetAllPortfoliosAsync(), Times.Once);
        }

        #endregion

        #region GetPortfolioByIdAsync Tests

        [Fact]
        public async Task GetPortfolioByIdAsync_ShouldReturnPortfolioDetail_WhenPortfolioExists()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var portfolio = TestDataFactory.CreatePortfolio();
            portfolio.Id = portfolioId;

            var expectedResponse = new PortfolioDetailResponse
            {
                Id = portfolio.Id,
                Title = portfolio.Title,
                UserId = portfolio.UserId
            };

            _mockPortfolioRepository.Setup(x => x.GetPortfolioByIdAsync(portfolioId))
                .ReturnsAsync(portfolio);
            _mockPortfolioMapper.Setup(x => x.MapToDetailDto(portfolio))
                .Returns(expectedResponse);

            // Act
            var result = await _service.GetPortfolioByIdAsync(portfolioId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResponse);
            _mockPortfolioRepository.Verify(x => x.GetPortfolioByIdAsync(portfolioId), Times.Once);
            _mockPortfolioMapper.Verify(x => x.MapToDetailDto(portfolio), Times.Once);
        }

        [Fact]
        public async Task GetPortfolioByIdAsync_ShouldReturnNull_WhenPortfolioDoesNotExist()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            _mockPortfolioRepository.Setup(x => x.GetPortfolioByIdAsync(portfolioId))
                .ReturnsAsync((Portfolio?)null);

            // Act
            var result = await _service.GetPortfolioByIdAsync(portfolioId);

            // Assert
            result.Should().BeNull();
            _mockPortfolioRepository.Verify(x => x.GetPortfolioByIdAsync(portfolioId), Times.Once);
            _mockPortfolioMapper.Verify(x => x.MapToDetailDto(It.IsAny<Portfolio>()), Times.Never);
        }

        [Fact]
        public async Task GetPortfolioByIdAsync_ShouldLogErrorAndRethrow_WhenExceptionOccurs()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var exception = new Exception("Database error");
            _mockPortfolioRepository.Setup(x => x.GetPortfolioByIdAsync(portfolioId))
                .ThrowsAsync(exception);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<Exception>(
                () => _service.GetPortfolioByIdAsync(portfolioId));

            thrownException.Should().Be(exception);
            _mockPortfolioRepository.Verify(x => x.GetPortfolioByIdAsync(portfolioId), Times.Once);
        }

        #endregion

        #region GetPortfoliosByUserIdAsync Tests

        [Fact]
        public async Task GetPortfoliosByUserIdAsync_ShouldReturnPortfolioSummaries_WhenUserHasPortfolios()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolios = new List<Portfolio>
            {
                TestDataFactory.CreatePortfolio(),
                TestDataFactory.CreatePortfolio()
            };

            var expectedResponses = new List<PortfolioSummaryResponse>
            {
                new PortfolioSummaryResponse { Id = portfolios[0].Id, Title = portfolios[0].Title },
                new PortfolioSummaryResponse { Id = portfolios[1].Id, Title = portfolios[1].Title }
            };

            _mockPortfolioRepository.Setup(x => x.GetPortfoliosByUserIdAsync(userId))
                .ReturnsAsync(portfolios);
            _mockPortfolioMapper.Setup(x => x.MapToSummaryDtos(portfolios))
                .Returns(expectedResponses);

            // Act
            var result = await _service.GetPortfoliosByUserIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedResponses);
            _mockPortfolioRepository.Verify(x => x.GetPortfoliosByUserIdAsync(userId), Times.Once);
            _mockPortfolioMapper.Verify(x => x.MapToSummaryDtos(portfolios), Times.Once);
        }

        [Fact]
        public async Task GetPortfoliosByUserIdAsync_ShouldReturnEmptyCollection_WhenUserHasNoPortfolios()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolios = new List<Portfolio>();
            var expectedResponses = new List<PortfolioSummaryResponse>();

            _mockPortfolioRepository.Setup(x => x.GetPortfoliosByUserIdAsync(userId))
                .ReturnsAsync(portfolios);
            _mockPortfolioMapper.Setup(x => x.MapToSummaryDtos(portfolios))
                .Returns(expectedResponses);

            // Act
            var result = await _service.GetPortfoliosByUserIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockPortfolioRepository.Verify(x => x.GetPortfoliosByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetPortfoliosByUserIdAsync_ShouldLogErrorAndRethrow_WhenExceptionOccurs()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var exception = new Exception("Database error");
            _mockPortfolioRepository.Setup(x => x.GetPortfoliosByUserIdAsync(userId))
                .ThrowsAsync(exception);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<Exception>(
                () => _service.GetPortfoliosByUserIdAsync(userId));

            thrownException.Should().Be(exception);
            _mockPortfolioRepository.Verify(x => x.GetPortfoliosByUserIdAsync(userId), Times.Once);
        }

        #endregion

        #region GetPublishedPortfoliosAsync Tests

        [Fact]
        public async Task GetPublishedPortfoliosAsync_ShouldReturnPublishedPortfolios_WhenPublishedPortfoliosExist()
        {
            // Arrange
            var portfolios = new List<Portfolio>
            {
                TestDataFactory.CreatePortfolio(),
                TestDataFactory.CreatePortfolio()
            };

            var expectedResponses = new List<PortfolioSummaryResponse>
            {
                new PortfolioSummaryResponse { Id = portfolios[0].Id, Title = portfolios[0].Title },
                new PortfolioSummaryResponse { Id = portfolios[1].Id, Title = portfolios[1].Title }
            };

            _mockPortfolioRepository.Setup(x => x.GetPublishedPortfoliosAsync())
                .ReturnsAsync(portfolios);
            _mockPortfolioMapper.Setup(x => x.MapToSummaryDtos(portfolios))
                .Returns(expectedResponses);

            // Act
            var result = await _service.GetPublishedPortfoliosAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedResponses);
            _mockPortfolioRepository.Verify(x => x.GetPublishedPortfoliosAsync(), Times.Once);
            _mockPortfolioMapper.Verify(x => x.MapToSummaryDtos(portfolios), Times.Once);
        }

        [Fact]
        public async Task GetPublishedPortfoliosAsync_ShouldReturnEmptyCollection_WhenNoPublishedPortfoliosExist()
        {
            // Arrange
            var portfolios = new List<Portfolio>();
            var expectedResponses = new List<PortfolioSummaryResponse>();

            _mockPortfolioRepository.Setup(x => x.GetPublishedPortfoliosAsync())
                .ReturnsAsync(portfolios);
            _mockPortfolioMapper.Setup(x => x.MapToSummaryDtos(portfolios))
                .Returns(expectedResponses);

            // Act
            var result = await _service.GetPublishedPortfoliosAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockPortfolioRepository.Verify(x => x.GetPublishedPortfoliosAsync(), Times.Once);
        }

        #endregion
    }
} 