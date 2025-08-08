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
using backend_portfolio.DTO.Project.Response;
using backend_portfolio.DTO.Experience.Response;
using backend_portfolio.DTO.Skill.Response;
using backend_portfolio.DTO.BlogPost.Response;
using backend_portfolio.DTO.Bookmark.Response;
using backend_portfolio.DTO.PortfolioTemplate.Response;
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
            _mockPortfolioMapper.Verify(x => x.MapToSummaryDtos(portfolios), Times.Once);
        }

        [Fact]
        public async Task GetAllPortfoliosAsync_ShouldLogErrorAndRethrow_WhenExceptionOccurs()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Database connection failed");

            _mockPortfolioRepository.Setup(x => x.GetAllPortfoliosAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.GetAllPortfoliosAsync());

            exception.Should().Be(expectedException);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while getting all portfolios")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
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
            var expectedException = new InvalidOperationException("Database connection failed");

            _mockPortfolioRepository.Setup(x => x.GetPortfolioByIdAsync(portfolioId))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.GetPortfolioByIdAsync(portfolioId));

            exception.Should().Be(expectedException);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while getting portfolio by ID")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
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
            _mockPortfolioMapper.Verify(x => x.MapToSummaryDtos(portfolios), Times.Once);
        }

        [Fact]
        public async Task GetPortfoliosByUserIdAsync_ShouldLogErrorAndRethrow_WhenExceptionOccurs()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedException = new InvalidOperationException("Database connection failed");

            _mockPortfolioRepository.Setup(x => x.GetPortfoliosByUserIdAsync(userId))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.GetPortfoliosByUserIdAsync(userId));

            exception.Should().Be(expectedException);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while getting portfolios for user")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
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
            _mockPortfolioMapper.Verify(x => x.MapToSummaryDtos(portfolios), Times.Once);
        }

        [Fact]
        public async Task GetPublishedPortfoliosAsync_ShouldLogErrorAndRethrow_WhenExceptionOccurs()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Database connection failed");

            _mockPortfolioRepository.Setup(x => x.GetPublishedPortfoliosAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.GetPublishedPortfoliosAsync());

            exception.Should().Be(expectedException);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while getting published portfolios")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        #endregion

        #region GetPortfoliosForHomePageAsync Tests

        [Fact]
        public async Task GetPortfoliosForHomePageAsync_ShouldReturnPortfolioCards_WhenPublishedPortfoliosExist()
        {
            // Arrange
            var portfolios = new List<Portfolio>
            {
                TestDataFactory.CreatePortfolio(),
                TestDataFactory.CreatePortfolio()
            };

            var skills = new List<Skill>
            {
                TestDataFactory.CreateSkill(),
                TestDataFactory.CreateSkill()
            };

            var bookmarks = new List<Bookmark>
            {
                TestDataFactory.CreateBookmark(),
                TestDataFactory.CreateBookmark()
            };

            var userInfo = new UserInformation(
                "John Doe",
                "Software Engineer", 
                "San Francisco",
                "/avatar.jpg"
            );

            _mockPortfolioRepository.Setup(x => x.GetPublishedPortfoliosAsync())
                .ReturnsAsync(portfolios);

            _mockSkillRepository.Setup(x => x.GetSkillsByPortfolioIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(skills);

            _mockBookmarkRepository.Setup(x => x.GetBookmarksByPortfolioIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(bookmarks);

            _mockExternalUserService.Setup(x => x.GetUserInformationAsync(It.IsAny<Guid>()))
                .ReturnsAsync(userInfo);

            // Act
            var result = await _service.GetPortfoliosForHomePageAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            _mockPortfolioRepository.Verify(x => x.GetPublishedPortfoliosAsync(), Times.Once);
            _mockSkillRepository.Verify(x => x.GetSkillsByPortfolioIdAsync(It.IsAny<Guid>()), Times.Exactly(2));
            _mockBookmarkRepository.Verify(x => x.GetBookmarksByPortfolioIdAsync(It.IsAny<Guid>()), Times.Exactly(2));
            _mockExternalUserService.Verify(x => x.GetUserInformationAsync(It.IsAny<Guid>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetPortfoliosForHomePageAsync_ShouldReturnEmptyCollection_WhenNoPublishedPortfoliosExist()
        {
            // Arrange
            var portfolios = new List<Portfolio>();

            _mockPortfolioRepository.Setup(x => x.GetPublishedPortfoliosAsync())
                .ReturnsAsync(portfolios);

            // Act
            var result = await _service.GetPortfoliosForHomePageAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockPortfolioRepository.Verify(x => x.GetPublishedPortfoliosAsync(), Times.Once);
            _mockSkillRepository.Verify(x => x.GetSkillsByPortfolioIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockBookmarkRepository.Verify(x => x.GetBookmarksByPortfolioIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockExternalUserService.Verify(x => x.GetUserInformationAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetPortfoliosForHomePageAsync_ShouldUseDefaultValues_WhenUserInfoIsNull()
        {
            // Arrange
            var portfolios = new List<Portfolio>
            {
                TestDataFactory.CreatePortfolio()
            };

            var skills = new List<Skill>();
            var bookmarks = new List<Bookmark>();

            _mockPortfolioRepository.Setup(x => x.GetPublishedPortfoliosAsync())
                .ReturnsAsync(portfolios);

            _mockSkillRepository.Setup(x => x.GetSkillsByPortfolioIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(skills);

            _mockBookmarkRepository.Setup(x => x.GetBookmarksByPortfolioIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(bookmarks);

            _mockExternalUserService.Setup(x => x.GetUserInformationAsync(It.IsAny<Guid>()))
                .ReturnsAsync((UserInformation?)null);

            // Act
            var result = await _service.GetPortfoliosForHomePageAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            var portfolioCard = result.First();
            portfolioCard.Name.Should().Be("Anonymous User");
            portfolioCard.Role.Should().Be("Professional");
            portfolioCard.Location.Should().Be("Remote");
            portfolioCard.Avatar.Should().Be("/default-avatar.png");
        }

        [Fact]
        public async Task GetPortfoliosForHomePageAsync_ShouldLogErrorAndRethrow_WhenExceptionOccurs()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Database connection failed");

            _mockPortfolioRepository.Setup(x => x.GetPublishedPortfoliosAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.GetPortfoliosForHomePageAsync());

            exception.Should().Be(expectedException);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while getting portfolios for home page")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        #endregion

        #region GetPortfoliosForHomePagePaginatedAsync Tests

        [Fact]
        public async Task GetPortfoliosForHomePagePaginatedAsync_ShouldReturnCachedResult_WhenCacheExists()
        {
            // Arrange
            var request = new PaginationRequest
            {
                Page = 1,
                PageSize = 10,
                SortBy = "most-recent",
                SortDirection = "desc"
            };

            var cachedResponse = new PaginatedResponse<PortfolioCardResponse>
            {
                Data = new List<PortfolioCardResponse>
                {
                    new PortfolioCardResponse { Id = Guid.NewGuid(), Name = "John Doe" }
                },
                Pagination = new PaginationMetadata
                {
                    CurrentPage = 1,
                    PageSize = 10,
                    TotalCount = 1,
                    TotalPages = 1
                }
            };

            _mockCacheService.Setup(x => x.GenerateKey(It.IsAny<string>(), It.IsAny<object[]>()))
                .Returns("test-cache-key");

            _mockCacheService.Setup(x => x.GetAsync<PaginatedResponse<PortfolioCardResponse>>("test-cache-key"))
                .ReturnsAsync(cachedResponse);

            // Act
            var result = await _service.GetPortfoliosForHomePagePaginatedAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(cachedResponse);
            _mockCacheService.Verify(x => x.GetAsync<PaginatedResponse<PortfolioCardResponse>>("test-cache-key"), Times.Once);
            _mockPortfolioRepository.Verify(x => x.GetPublishedPortfoliosAsync(), Times.Never);
        }

        [Fact]
        public async Task GetPortfoliosForHomePagePaginatedAsync_ShouldGenerateNewResult_WhenCacheDoesNotExist()
        {
            // Arrange
            var request = new PaginationRequest
            {
                Page = 1,
                PageSize = 10,
                SortBy = "most-recent",
                SortDirection = "desc"
            };

            var portfolios = new List<Portfolio>
            {
                TestDataFactory.CreatePortfolio(),
                TestDataFactory.CreatePortfolio()
            };

            var skills = new List<Skill>();
            var bookmarks = new List<Bookmark>();
            var userInfo = new UserInformation(
                "John Doe",
                "Software Engineer", 
                "San Francisco",
                "/default-avatar.png"
            );

            _mockCacheService.Setup(x => x.GenerateKey(It.IsAny<string>(), It.IsAny<object[]>()))
                .Returns("test-cache-key");

            _mockCacheService.Setup(x => x.GetAsync<PaginatedResponse<PortfolioCardResponse>>("test-cache-key"))
                .ReturnsAsync((PaginatedResponse<PortfolioCardResponse>?)null);

            _mockPortfolioRepository.Setup(x => x.GetPublishedPortfoliosAsync())
                .ReturnsAsync(portfolios);

            _mockSkillRepository.Setup(x => x.GetSkillsByPortfolioIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(skills);

            _mockBookmarkRepository.Setup(x => x.GetBookmarksByPortfolioIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(bookmarks);

            _mockExternalUserService.Setup(x => x.GetUserInformationAsync(It.IsAny<Guid>()))
                .ReturnsAsync(userInfo);

            _mockCacheService.Setup(x => x.SetAsync("test-cache-key", It.IsAny<PaginatedResponse<PortfolioCardResponse>>(), It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.GetPortfoliosForHomePagePaginatedAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Pagination.CurrentPage.Should().Be(1);
            result.Pagination.PageSize.Should().Be(10);
            result.Pagination.TotalCount.Should().Be(2);
            result.CacheKey.Should().Be("test-cache-key");

            _mockCacheService.Verify(x => x.GetAsync<PaginatedResponse<PortfolioCardResponse>>("test-cache-key"), Times.Once);
            _mockPortfolioRepository.Verify(x => x.GetPublishedPortfoliosAsync(), Times.Once);
            _mockCacheService.Verify(x => x.SetAsync("test-cache-key", It.IsAny<PaginatedResponse<PortfolioCardResponse>>(), It.IsAny<TimeSpan>()), Times.Once);
        }

        [Fact]
        public async Task GetPortfoliosForHomePagePaginatedAsync_ShouldApplyFilters_WhenFilterParametersProvided()
        {
            // Arrange
            var request = new PaginationRequest
            {
                Page = 1,
                PageSize = 10,
                SearchTerm = "developer",
                Skills = new List<string> { "C#", "JavaScript" },
                Roles = new List<string> { "Software Engineer" },
                Featured = true,
                DateFrom = DateTime.UtcNow.AddDays(-30),
                DateTo = DateTime.UtcNow
            };

            var portfolios = new List<Portfolio>
            {
                TestDataFactory.CreatePortfolio(),
                TestDataFactory.CreatePortfolio()
            };

            var skills = new List<Skill>
            {
                new Skill { Name = "C#" },
                new Skill { Name = "JavaScript" }
            };

            var bookmarks = new List<Bookmark>();
            var userInfo = new UserInformation(
                "John Doe",
                "Software Engineer", 
                "San Francisco",
                "/default-avatar.png"
            );

            _mockCacheService.Setup(x => x.GenerateKey(It.IsAny<string>(), It.IsAny<object[]>()))
                .Returns("test-cache-key");

            _mockCacheService.Setup(x => x.GetAsync<PaginatedResponse<PortfolioCardResponse>>("test-cache-key"))
                .ReturnsAsync((PaginatedResponse<PortfolioCardResponse>?)null);

            _mockPortfolioRepository.Setup(x => x.GetPublishedPortfoliosAsync())
                .ReturnsAsync(portfolios);

            _mockSkillRepository.Setup(x => x.GetSkillsByPortfolioIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(skills);

            _mockBookmarkRepository.Setup(x => x.GetBookmarksByPortfolioIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(bookmarks);

            _mockExternalUserService.Setup(x => x.GetUserInformationAsync(It.IsAny<Guid>()))
                .ReturnsAsync(userInfo);

            _mockCacheService.Setup(x => x.SetAsync("test-cache-key", It.IsAny<PaginatedResponse<PortfolioCardResponse>>(), It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.GetPortfoliosForHomePagePaginatedAsync(request);

            // Assert
            result.Should().NotBeNull();
            _mockPortfolioRepository.Verify(x => x.GetPublishedPortfoliosAsync(), Times.Once);
        }

        [Fact]
        public async Task GetPortfoliosForHomePagePaginatedAsync_ShouldLogErrorAndRethrow_WhenExceptionOccurs()
        {
            // Arrange
            var request = new PaginationRequest
            {
                Page = 1,
                PageSize = 10
            };

            var expectedException = new InvalidOperationException("Database connection failed");

            _mockCacheService.Setup(x => x.GenerateKey(It.IsAny<string>(), It.IsAny<object[]>()))
                .Returns("test-cache-key");

            _mockCacheService.Setup(x => x.GetAsync<PaginatedResponse<PortfolioCardResponse>>("test-cache-key"))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.GetPortfoliosForHomePagePaginatedAsync(request));

            exception.Should().Be(expectedException);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while getting paginated portfolios for home page")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        #endregion

        #region GetUserPortfolioComprehensiveAsync Tests

        [Fact]
        public async Task GetUserPortfolioComprehensiveAsync_ShouldReturnComprehensiveData_WhenUserHasPortfolios()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolios = new List<Portfolio>
            {
                TestDataFactory.CreatePortfolio(),
                TestDataFactory.CreatePortfolio()
            };

            var projects = new List<Project>
            {
                TestDataFactory.CreateProject(),
                TestDataFactory.CreateProject()
            };

            var experience = new List<Experience>
            {
                TestDataFactory.CreateExperience(),
                TestDataFactory.CreateExperience()
            };

            var skills = new List<Skill>
            {
                TestDataFactory.CreateSkill(),
                TestDataFactory.CreateSkill()
            };

            var blogPosts = new List<BlogPost>
            {
                TestDataFactory.CreateBlogPost(),
                TestDataFactory.CreateBlogPost()
            };

            var bookmarks = new List<Bookmark>
            {
                TestDataFactory.CreateBookmark(),
                TestDataFactory.CreateBookmark()
            };

            var templates = new List<PortfolioTemplate>
            {
                TestDataFactory.CreatePortfolioTemplate(),
                TestDataFactory.CreatePortfolioTemplate()
            };

            var portfolioIds = portfolios.Select(p => p.Id).ToList();

            _mockPortfolioRepository.Setup(x => x.GetPortfoliosByUserIdAsync(userId))
                .ReturnsAsync(portfolios);

            _mockProjectRepository.Setup(x => x.GetProjectsByPortfolioIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(projects);

            _mockExperienceRepository.Setup(x => x.GetExperienceByPortfolioIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(experience);

            _mockSkillRepository.Setup(x => x.GetSkillsByPortfolioIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(skills);

            _mockBlogPostRepository.Setup(x => x.GetBlogPostsByPortfolioIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(blogPosts);

            _mockBookmarkRepository.Setup(x => x.GetBookmarksByPortfolioIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(bookmarks);

            _mockTemplateRepository.Setup(x => x.GetTemplateByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(templates.First());

            _mockPortfolioMapper.Setup(x => x.MapToSummaryDtos(portfolios))
                .Returns(new List<PortfolioSummaryResponse>());

            // Act
            var result = await _service.GetUserPortfolioComprehensiveAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.Portfolios.Should().NotBeNull();
            result.Projects.Should().NotBeNull();
            result.Experience.Should().NotBeNull();
            result.Skills.Should().NotBeNull();
            result.BlogPosts.Should().NotBeNull();
            result.Bookmarks.Should().NotBeNull();
            result.Templates.Should().NotBeNull();

            _mockPortfolioRepository.Verify(x => x.GetPortfoliosByUserIdAsync(userId), Times.Once);
            _mockProjectRepository.Verify(x => x.GetProjectsByPortfolioIdAsync(It.IsAny<Guid>()), Times.Exactly(2));
            _mockExperienceRepository.Verify(x => x.GetExperienceByPortfolioIdAsync(It.IsAny<Guid>()), Times.Exactly(2));
            _mockSkillRepository.Verify(x => x.GetSkillsByPortfolioIdAsync(It.IsAny<Guid>()), Times.Exactly(2));
            _mockBlogPostRepository.Verify(x => x.GetBlogPostsByPortfolioIdAsync(It.IsAny<Guid>()), Times.Exactly(2));
            _mockBookmarkRepository.Verify(x => x.GetBookmarksByPortfolioIdAsync(It.IsAny<Guid>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetUserPortfolioComprehensiveAsync_ShouldReturnEmptyComprehensiveData_WhenUserHasNoPortfolios()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolios = new List<Portfolio>();

            _mockPortfolioRepository.Setup(x => x.GetPortfoliosByUserIdAsync(userId))
                .ReturnsAsync(portfolios);

            // Act
            var result = await _service.GetUserPortfolioComprehensiveAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.Portfolios.Should().BeEmpty();
            result.Projects.Should().BeEmpty();
            result.Experience.Should().BeEmpty();
            result.Skills.Should().BeEmpty();
            result.BlogPosts.Should().BeEmpty();
            result.Bookmarks.Should().BeEmpty();
            result.Templates.Should().BeEmpty();

            _mockPortfolioRepository.Verify(x => x.GetPortfoliosByUserIdAsync(userId), Times.Once);
            _mockProjectRepository.Verify(x => x.GetProjectsByPortfolioIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockExperienceRepository.Verify(x => x.GetExperienceByPortfolioIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockSkillRepository.Verify(x => x.GetSkillsByPortfolioIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockBlogPostRepository.Verify(x => x.GetBlogPostsByPortfolioIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockBookmarkRepository.Verify(x => x.GetBookmarksByPortfolioIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetUserPortfolioComprehensiveAsync_ShouldLogErrorAndRethrow_WhenExceptionOccurs()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedException = new InvalidOperationException("Database connection failed");

            _mockPortfolioRepository.Setup(x => x.GetPortfoliosByUserIdAsync(userId))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.GetUserPortfolioComprehensiveAsync(userId));

            exception.Should().Be(expectedException);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while getting comprehensive portfolio data for user")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        #endregion


    }
} 