using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using backend_portfolio.Services;
using backend_portfolio.Services.Abstractions;
using backend_portfolio.Services.Mappers;
using backend_portfolio.Repositories;
using backend_portfolio.Models;
using backend_portfolio.DTO.Portfolio.Request;
using backend_portfolio.DTO.Portfolio.Response;
using backend_portfolio.tests.Helpers;

namespace backend_portfolio.tests.Services
{
    public class PortfolioCommandServiceTests
    {
        private readonly Mock<IPortfolioRepository> _mockPortfolioRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IExperienceRepository> _mockExperienceRepository;
        private readonly Mock<ISkillRepository> _mockSkillRepository;
        private readonly Mock<IBlogPostRepository> _mockBlogPostRepository;
        private readonly Mock<IValidationService<PortfolioCreateRequest>> _mockPortfolioValidator;
        private readonly Mock<IValidationService<PortfolioUpdateRequest>> _mockPortfolioUpdateValidator;
        private readonly Mock<IPortfolioMapper> _mockPortfolioMapper;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ILogger<PortfolioCommandService>> _mockLogger;
        private readonly PortfolioCommandService _service;

        public PortfolioCommandServiceTests()
        {
            _mockPortfolioRepository = new Mock<IPortfolioRepository>();
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockExperienceRepository = new Mock<IExperienceRepository>();
            _mockSkillRepository = new Mock<ISkillRepository>();
            _mockBlogPostRepository = new Mock<IBlogPostRepository>();
            _mockPortfolioValidator = new Mock<IValidationService<PortfolioCreateRequest>>();
            _mockPortfolioUpdateValidator = new Mock<IValidationService<PortfolioUpdateRequest>>();
            _mockPortfolioMapper = new Mock<IPortfolioMapper>();
            _mockCacheService = new Mock<ICacheService>();
            _mockLogger = new Mock<ILogger<PortfolioCommandService>>();

            _service = new PortfolioCommandService(
                _mockPortfolioRepository.Object,
                _mockProjectRepository.Object,
                _mockExperienceRepository.Object,
                _mockSkillRepository.Object,
                _mockBlogPostRepository.Object,
                _mockPortfolioValidator.Object,
                _mockPortfolioUpdateValidator.Object,
                _mockPortfolioMapper.Object,
                _mockCacheService.Object,
                _mockLogger.Object
            );
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenAllParametersAreValid()
        {
            // Act
            var service = new PortfolioCommandService(
                _mockPortfolioRepository.Object,
                _mockProjectRepository.Object,
                _mockExperienceRepository.Object,
                _mockSkillRepository.Object,
                _mockBlogPostRepository.Object,
                _mockPortfolioValidator.Object,
                _mockPortfolioUpdateValidator.Object,
                _mockPortfolioMapper.Object,
                _mockCacheService.Object,
                _mockLogger.Object
            );

            // Assert
            service.Should().NotBeNull();
        }

        #endregion

        #region CreatePortfolioAsync Tests

        [Fact]
        public async Task CreatePortfolioAsync_ShouldReturnPortfolioResponse_WhenValidRequestProvided()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                TemplateName = "Default Template",
                Title = "Test Portfolio"
            };

            var portfolio = TestDataFactory.CreatePortfolio();
            var expectedResponse = new PortfolioResponse
            {
                Id = portfolio.Id,
                Title = portfolio.Title,
                UserId = portfolio.UserId
            };

            var validationResult = ValidationResult.Success();

            _mockPortfolioValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(validationResult);
            _mockPortfolioRepository.Setup(x => x.CreatePortfolioAsync(request))
                .ReturnsAsync(portfolio);
            _mockPortfolioMapper.Setup(x => x.MapToResponseDto(portfolio))
                .Returns(expectedResponse);

            // Act
            var result = await _service.CreatePortfolioAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResponse);
            _mockPortfolioValidator.Verify(x => x.ValidateAsync(request), Times.Once);
            _mockPortfolioRepository.Verify(x => x.CreatePortfolioAsync(request), Times.Once);
            _mockPortfolioMapper.Verify(x => x.MapToResponseDto(portfolio), Times.Once);
        }

        [Fact]
        public async Task CreatePortfolioAsync_ShouldThrowArgumentException_WhenValidationFails()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                TemplateName = "Default Template",
                Title = ""
            };

            var validationResult = ValidationResult.Failure("Title is required");

            _mockPortfolioValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(validationResult);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreatePortfolioAsync(request));

            exception.Message.Should().Contain("Validation failed");
            exception.Message.Should().Contain("Title is required");
            _mockPortfolioValidator.Verify(x => x.ValidateAsync(request), Times.Once);
            _mockPortfolioRepository.Verify(x => x.CreatePortfolioAsync(It.IsAny<PortfolioCreateRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreatePortfolioAsync_ShouldInvalidateCache_WhenPortfolioIsPublished()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                TemplateName = "Default Template",
                Title = "Test Portfolio"
            };

            var portfolio = TestDataFactory.CreatePortfolio();
            portfolio.IsPublished = true;

            var expectedResponse = new PortfolioResponse
            {
                Id = portfolio.Id,
                Title = portfolio.Title,
                UserId = portfolio.UserId
            };

            var validationResult = ValidationResult.Success();

            _mockPortfolioValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(validationResult);
            _mockPortfolioRepository.Setup(x => x.CreatePortfolioAsync(request))
                .ReturnsAsync(portfolio);
            _mockPortfolioMapper.Setup(x => x.MapToResponseDto(portfolio))
                .Returns(expectedResponse);

            // Act
            var result = await _service.CreatePortfolioAsync(request);

            // Assert
            result.Should().NotBeNull();
            _mockCacheService.Verify(x => x.RemoveByPatternAsync("portfolios_paginated:.*"), Times.Once);
            _mockCacheService.Verify(x => x.RemoveByPatternAsync("portfolios:.*"), Times.Once);
        }

        [Fact]
        public async Task CreatePortfolioAsync_ShouldNotInvalidateCache_WhenPortfolioIsNotPublished()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                TemplateName = "Default Template",
                Title = "Test Portfolio"
            };

            var portfolio = TestDataFactory.CreatePortfolio();
            portfolio.IsPublished = false;

            var expectedResponse = new PortfolioResponse
            {
                Id = portfolio.Id,
                Title = portfolio.Title,
                UserId = portfolio.UserId
            };

            var validationResult = ValidationResult.Success();

            _mockPortfolioValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(validationResult);
            _mockPortfolioRepository.Setup(x => x.CreatePortfolioAsync(request))
                .ReturnsAsync(portfolio);
            _mockPortfolioMapper.Setup(x => x.MapToResponseDto(portfolio))
                .Returns(expectedResponse);

            // Act
            var result = await _service.CreatePortfolioAsync(request);

            // Assert
            result.Should().NotBeNull();
            _mockCacheService.Verify(x => x.RemoveByPatternAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region DeletePortfolioAsync Tests

        [Fact]
        public async Task DeletePortfolioAsync_ShouldReturnTrue_WhenPortfolioExistsAndIsDeleted()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            _mockPortfolioRepository.Setup(x => x.DeletePortfolioAsync(portfolioId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeletePortfolioAsync(portfolioId);

            // Assert
            result.Should().BeTrue();
            _mockPortfolioRepository.Verify(x => x.DeletePortfolioAsync(portfolioId), Times.Once);
            _mockCacheService.Verify(x => x.RemoveByPatternAsync("portfolios_paginated:.*"), Times.Once);
            _mockCacheService.Verify(x => x.RemoveByPatternAsync("portfolios:.*"), Times.Once);
        }

        [Fact]
        public async Task DeletePortfolioAsync_ShouldReturnFalse_WhenPortfolioDoesNotExist()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            _mockPortfolioRepository.Setup(x => x.DeletePortfolioAsync(portfolioId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeletePortfolioAsync(portfolioId);

            // Assert
            result.Should().BeFalse();
            _mockPortfolioRepository.Verify(x => x.DeletePortfolioAsync(portfolioId), Times.Once);
            _mockCacheService.Verify(x => x.RemoveByPatternAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region IncrementViewCountAsync Tests

        [Fact]
        public async Task IncrementViewCountAsync_ShouldReturnTrue_WhenPortfolioExists()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            _mockPortfolioRepository.Setup(x => x.IncrementViewCountAsync(portfolioId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.IncrementViewCountAsync(portfolioId);

            // Assert
            result.Should().BeTrue();
            _mockPortfolioRepository.Verify(x => x.IncrementViewCountAsync(portfolioId), Times.Once);
        }

        [Fact]
        public async Task IncrementViewCountAsync_ShouldReturnFalse_WhenPortfolioDoesNotExist()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            _mockPortfolioRepository.Setup(x => x.IncrementViewCountAsync(portfolioId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.IncrementViewCountAsync(portfolioId);

            // Assert
            result.Should().BeFalse();
            _mockPortfolioRepository.Verify(x => x.IncrementViewCountAsync(portfolioId), Times.Once);
        }

        #endregion

        #region IncrementLikeCountAsync Tests

        [Fact]
        public async Task IncrementLikeCountAsync_ShouldReturnTrue_WhenPortfolioExists()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            _mockPortfolioRepository.Setup(x => x.IncrementLikeCountAsync(portfolioId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.IncrementLikeCountAsync(portfolioId);

            // Assert
            result.Should().BeTrue();
            _mockPortfolioRepository.Verify(x => x.IncrementLikeCountAsync(portfolioId), Times.Once);
        }

        [Fact]
        public async Task IncrementLikeCountAsync_ShouldReturnFalse_WhenPortfolioDoesNotExist()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            _mockPortfolioRepository.Setup(x => x.IncrementLikeCountAsync(portfolioId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.IncrementLikeCountAsync(portfolioId);

            // Assert
            result.Should().BeFalse();
            _mockPortfolioRepository.Verify(x => x.IncrementLikeCountAsync(portfolioId), Times.Once);
        }

        #endregion

        #region DecrementLikeCountAsync Tests

        [Fact]
        public async Task DecrementLikeCountAsync_ShouldReturnTrue_WhenPortfolioExists()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            _mockPortfolioRepository.Setup(x => x.DecrementLikeCountAsync(portfolioId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DecrementLikeCountAsync(portfolioId);

            // Assert
            result.Should().BeTrue();
            _mockPortfolioRepository.Verify(x => x.DecrementLikeCountAsync(portfolioId), Times.Once);
        }

        [Fact]
        public async Task DecrementLikeCountAsync_ShouldReturnFalse_WhenPortfolioDoesNotExist()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            _mockPortfolioRepository.Setup(x => x.DecrementLikeCountAsync(portfolioId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DecrementLikeCountAsync(portfolioId);

            // Assert
            result.Should().BeFalse();
            _mockPortfolioRepository.Verify(x => x.DecrementLikeCountAsync(portfolioId), Times.Once);
        }

        #endregion
    }
} 