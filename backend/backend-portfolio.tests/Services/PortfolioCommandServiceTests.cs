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
using backend_portfolio.DTO.Project.Request;
using backend_portfolio.DTO.Experience.Request;
using backend_portfolio.DTO.Skill.Request;
using backend_portfolio.DTO.BlogPost.Request;
using backend_portfolio.DTO.PortfolioTemplate.Request;
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
                Title = "Test Portfolio",
                Bio = "Test Bio",
                IsPublished = true
            };

            var portfolio = TestDataFactory.CreatePortfolio();
            var expectedResponse = new PortfolioResponse
            {
                Id = portfolio.Id,
                Title = portfolio.Title,
                UserId = portfolio.UserId,
                Bio = portfolio.Bio,
                IsPublished = portfolio.IsPublished
            };

            _mockPortfolioValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(new ValidationResult { IsValid = true });

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
                Title = "Test Portfolio"
            };

            var validationErrors = new List<string> { "Title is required", "Bio is required" };
            _mockPortfolioValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(new ValidationResult { IsValid = false, Errors = validationErrors });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreatePortfolioAsync(request));

            exception.Message.Should().Contain("Validation failed");
            exception.Message.Should().Contain("Title is required");
            exception.Message.Should().Contain("Bio is required");

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
                Title = "Test Portfolio",
                IsPublished = true
            };

            var portfolio = TestDataFactory.CreatePortfolio();
            portfolio.IsPublished = true;

            var expectedResponse = new PortfolioResponse
            {
                Id = portfolio.Id,
                Title = portfolio.Title,
                UserId = portfolio.UserId,
                IsPublished = true
            };

            _mockPortfolioValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(new ValidationResult { IsValid = true });

            _mockPortfolioRepository.Setup(x => x.CreatePortfolioAsync(request))
                .ReturnsAsync(portfolio);

            _mockPortfolioMapper.Setup(x => x.MapToResponseDto(portfolio))
                .Returns(expectedResponse);

            _mockCacheService.Setup(x => x.RemoveByPatternAsync("portfolios_paginated:.*"))
                .Returns(Task.CompletedTask);

            _mockCacheService.Setup(x => x.RemoveByPatternAsync("portfolios:.*"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreatePortfolioAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsPublished.Should().BeTrue();
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
                Title = "Test Portfolio",
                IsPublished = false
            };

            var portfolio = TestDataFactory.CreatePortfolio();
            portfolio.IsPublished = false;

            var expectedResponse = new PortfolioResponse
            {
                Id = portfolio.Id,
                Title = portfolio.Title,
                UserId = portfolio.UserId,
                IsPublished = false
            };

            _mockPortfolioValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(new ValidationResult { IsValid = true });

            _mockPortfolioRepository.Setup(x => x.CreatePortfolioAsync(request))
                .ReturnsAsync(portfolio);

            _mockPortfolioMapper.Setup(x => x.MapToResponseDto(portfolio))
                .Returns(expectedResponse);

            // Act
            var result = await _service.CreatePortfolioAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsPublished.Should().BeFalse();
            _mockCacheService.Verify(x => x.RemoveByPatternAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CreatePortfolioAsync_ShouldLogErrorAndRethrow_WhenExceptionOccurs()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "Test Portfolio"
            };

            var expectedException = new InvalidOperationException("Database connection failed");

            _mockPortfolioValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(new ValidationResult { IsValid = true });

            _mockPortfolioRepository.Setup(x => x.CreatePortfolioAsync(request))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.CreatePortfolioAsync(request));

            exception.Should().Be(expectedException);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while creating portfolio")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        #endregion

        #region CreatePortfolioAndGetIdAsync Tests

        [Fact]
        public async Task CreatePortfolioAndGetIdAsync_ShouldReturnPortfolioResponse_WhenValidRequestProvided()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                TemplateName = "Default Template",
                Title = "Test Portfolio",
                Bio = "Test Bio",
                IsPublished = true
            };

            var portfolio = TestDataFactory.CreatePortfolio();
            var expectedResponse = new PortfolioResponse
            {
                Id = portfolio.Id,
                Title = portfolio.Title,
                UserId = portfolio.UserId,
                Bio = portfolio.Bio,
                IsPublished = portfolio.IsPublished
            };

            _mockPortfolioValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(new ValidationResult { IsValid = true });

            _mockPortfolioRepository.Setup(x => x.CreatePortfolioAsync(request))
                .ReturnsAsync(portfolio);

            _mockPortfolioMapper.Setup(x => x.MapToResponseDto(portfolio))
                .Returns(expectedResponse);

            // Act
            var result = await _service.CreatePortfolioAndGetIdAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResponse);
            _mockPortfolioValidator.Verify(x => x.ValidateAsync(request), Times.Once);
            _mockPortfolioRepository.Verify(x => x.CreatePortfolioAsync(request), Times.Once);
            _mockPortfolioMapper.Verify(x => x.MapToResponseDto(portfolio), Times.Once);
        }

        [Fact]
        public async Task CreatePortfolioAndGetIdAsync_ShouldThrowArgumentException_WhenValidationFails()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "Test Portfolio"
            };

            var validationErrors = new List<string> { "Title is required", "Bio is required" };
            _mockPortfolioValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(new ValidationResult { IsValid = false, Errors = validationErrors });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreatePortfolioAndGetIdAsync(request));

            exception.Message.Should().Contain("Validation failed");
            exception.Message.Should().Contain("Title is required");
            exception.Message.Should().Contain("Bio is required");

            _mockPortfolioValidator.Verify(x => x.ValidateAsync(request), Times.Once);
            _mockPortfolioRepository.Verify(x => x.CreatePortfolioAsync(It.IsAny<PortfolioCreateRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreatePortfolioAndGetIdAsync_ShouldLogErrorAndRethrow_WhenExceptionOccurs()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "Test Portfolio"
            };

            var expectedException = new InvalidOperationException("Database connection failed");

            _mockPortfolioValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(new ValidationResult { IsValid = true });

            _mockPortfolioRepository.Setup(x => x.CreatePortfolioAsync(request))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.CreatePortfolioAndGetIdAsync(request));

            exception.Should().Be(expectedException);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while creating portfolio and getting ID")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        #endregion

        #region SavePortfolioContentAsync Tests

        [Fact]
        public async Task SavePortfolioContentAsync_ShouldSaveAllContent_WhenValidRequestProvided()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var request = new BulkPortfolioContentRequest
            {
                Projects = new List<ProjectCreateRequest>
                {
                    new ProjectCreateRequest { Title = "Project 1", PortfolioId = portfolioId },
                    new ProjectCreateRequest { Title = "Project 2", PortfolioId = portfolioId }
                },
                Experience = new List<ExperienceCreateRequest>
                {
                    new ExperienceCreateRequest { JobTitle = "Experience 1", PortfolioId = portfolioId }
                },
                Skills = new List<SkillCreateRequest>
                {
                    new SkillCreateRequest { Name = "Skill 1", PortfolioId = portfolioId }
                },
                BlogPosts = new List<BlogPostCreateRequest>
                {
                    new BlogPostCreateRequest { Title = "Blog Post 1", PortfolioId = portfolioId }
                },
                PublishPortfolio = true
            };

            var updatedPortfolio = TestDataFactory.CreatePortfolio();
            updatedPortfolio.Id = portfolioId;
            updatedPortfolio.IsPublished = true;

            _mockProjectRepository.Setup(x => x.CreateProjectAsync(It.IsAny<ProjectCreateRequest>()))
                .ReturnsAsync(TestDataFactory.CreateProject());

            _mockExperienceRepository.Setup(x => x.CreateExperienceAsync(It.IsAny<ExperienceCreateRequest>()))
                .ReturnsAsync(TestDataFactory.CreateExperience());

            _mockSkillRepository.Setup(x => x.CreateSkillAsync(It.IsAny<SkillCreateRequest>()))
                .ReturnsAsync(TestDataFactory.CreateSkill());

            _mockBlogPostRepository.Setup(x => x.CreateBlogPostAsync(It.IsAny<BlogPostCreateRequest>()))
                .ReturnsAsync(TestDataFactory.CreateBlogPost());

            _mockPortfolioRepository.Setup(x => x.UpdatePortfolioAsync(portfolioId, It.IsAny<PortfolioUpdateRequest>()))
                .ReturnsAsync(updatedPortfolio);

            _mockCacheService.Setup(x => x.RemoveByPatternAsync("portfolios_paginated:.*"))
                .Returns(Task.CompletedTask);

            _mockCacheService.Setup(x => x.RemoveByPatternAsync("portfolios:.*"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.SavePortfolioContentAsync(portfolioId, request);

            // Assert
            result.Should().NotBeNull();
            result.Message.Should().Be("Portfolio content saved successfully");
            result.ProjectsCreated.Should().Be(2);
            result.ExperienceCreated.Should().Be(1);
            result.SkillsCreated.Should().Be(1);
            result.BlogPostsCreated.Should().Be(1);
            result.PortfolioPublished.Should().BeTrue();

            _mockProjectRepository.Verify(x => x.CreateProjectAsync(It.IsAny<ProjectCreateRequest>()), Times.Exactly(2));
            _mockExperienceRepository.Verify(x => x.CreateExperienceAsync(It.IsAny<ExperienceCreateRequest>()), Times.Exactly(1));
            _mockSkillRepository.Verify(x => x.CreateSkillAsync(It.IsAny<SkillCreateRequest>()), Times.Exactly(1));
            _mockBlogPostRepository.Verify(x => x.CreateBlogPostAsync(It.IsAny<BlogPostCreateRequest>()), Times.Exactly(1));
            _mockPortfolioRepository.Verify(x => x.UpdatePortfolioAsync(portfolioId, It.Is<PortfolioUpdateRequest>(r => r.IsPublished == true)), Times.Once);
            _mockCacheService.Verify(x => x.RemoveByPatternAsync("portfolios_paginated:.*"), Times.Once);
            _mockCacheService.Verify(x => x.RemoveByPatternAsync("portfolios:.*"), Times.Once);
        }

        [Fact]
        public async Task SavePortfolioContentAsync_ShouldHandleEmptyContent_WhenNoContentProvided()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var request = new BulkPortfolioContentRequest
            {
                PublishPortfolio = false
            };

            // Act
            var result = await _service.SavePortfolioContentAsync(portfolioId, request);

            // Assert
            result.Should().NotBeNull();
            result.Message.Should().Be("Portfolio content saved successfully");
            result.ProjectsCreated.Should().Be(0);
            result.ExperienceCreated.Should().Be(0);
            result.SkillsCreated.Should().Be(0);
            result.BlogPostsCreated.Should().Be(0);
            result.PortfolioPublished.Should().BeFalse();

            _mockProjectRepository.Verify(x => x.CreateProjectAsync(It.IsAny<ProjectCreateRequest>()), Times.Never);
            _mockExperienceRepository.Verify(x => x.CreateExperienceAsync(It.IsAny<ExperienceCreateRequest>()), Times.Never);
            _mockSkillRepository.Verify(x => x.CreateSkillAsync(It.IsAny<SkillCreateRequest>()), Times.Never);
            _mockBlogPostRepository.Verify(x => x.CreateBlogPostAsync(It.IsAny<BlogPostCreateRequest>()), Times.Never);
            _mockPortfolioRepository.Verify(x => x.UpdatePortfolioAsync(It.IsAny<Guid>(), It.IsAny<PortfolioUpdateRequest>()), Times.Never);
        }

        [Fact]
        public async Task SavePortfolioContentAsync_ShouldNotPublishPortfolio_WhenPublishPortfolioIsFalse()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var request = new BulkPortfolioContentRequest
            {
                Projects = new List<ProjectCreateRequest>
                {
                    new ProjectCreateRequest { Title = "Project 1", PortfolioId = portfolioId }
                },
                PublishPortfolio = false
            };

            _mockProjectRepository.Setup(x => x.CreateProjectAsync(It.IsAny<ProjectCreateRequest>()))
                .ReturnsAsync(TestDataFactory.CreateProject());

            // Act
            var result = await _service.SavePortfolioContentAsync(portfolioId, request);

            // Assert
            result.Should().NotBeNull();
            result.PortfolioPublished.Should().BeFalse();

            _mockProjectRepository.Verify(x => x.CreateProjectAsync(It.IsAny<ProjectCreateRequest>()), Times.Once);
            _mockPortfolioRepository.Verify(x => x.UpdatePortfolioAsync(It.IsAny<Guid>(), It.IsAny<PortfolioUpdateRequest>()), Times.Never);
            _mockCacheService.Verify(x => x.RemoveByPatternAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SavePortfolioContentAsync_ShouldHandleNullCollections()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var request = new BulkPortfolioContentRequest
            {
                Projects = null,
                Experience = null,
                Skills = null,
                BlogPosts = null,
                PublishPortfolio = false
            };

            // Act
            var result = await _service.SavePortfolioContentAsync(portfolioId, request);

            // Assert
            result.Should().NotBeNull();
            result.ProjectsCreated.Should().Be(0);
            result.ExperienceCreated.Should().Be(0);
            result.SkillsCreated.Should().Be(0);
            result.BlogPostsCreated.Should().Be(0);
        }

        #endregion

        #region UpdatePortfolioAsync Tests

        [Fact]
        public async Task UpdatePortfolioAsync_ShouldReturnUpdatedPortfolio_WhenValidRequestProvided()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var request = new PortfolioUpdateRequest
            {
                Title = "Updated Portfolio",
                Bio = "Updated Bio",
                IsPublished = true
            };

            var updatedPortfolio = TestDataFactory.CreatePortfolio();
            updatedPortfolio.Id = portfolioId;
            updatedPortfolio.Title = "Updated Portfolio";
            updatedPortfolio.Bio = "Updated Bio";
            updatedPortfolio.IsPublished = true;

            var expectedResponse = new PortfolioResponse
            {
                Id = updatedPortfolio.Id,
                Title = updatedPortfolio.Title,
                Bio = updatedPortfolio.Bio,
                IsPublished = updatedPortfolio.IsPublished
            };

            _mockPortfolioUpdateValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(new ValidationResult { IsValid = true });

            _mockPortfolioRepository.Setup(x => x.UpdatePortfolioAsync(portfolioId, request))
                .ReturnsAsync(updatedPortfolio);

            _mockPortfolioMapper.Setup(x => x.MapToResponseDto(updatedPortfolio))
                .Returns(expectedResponse);

            _mockCacheService.Setup(x => x.RemoveByPatternAsync("portfolios_paginated:.*"))
                .Returns(Task.CompletedTask);

            _mockCacheService.Setup(x => x.RemoveByPatternAsync("portfolios:.*"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdatePortfolioAsync(portfolioId, request);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResponse);
            _mockPortfolioUpdateValidator.Verify(x => x.ValidateAsync(request), Times.Once);
            _mockPortfolioRepository.Verify(x => x.UpdatePortfolioAsync(portfolioId, request), Times.Once);
            _mockPortfolioMapper.Verify(x => x.MapToResponseDto(updatedPortfolio), Times.Once);
            _mockCacheService.Verify(x => x.RemoveByPatternAsync("portfolios_paginated:.*"), Times.Once);
            _mockCacheService.Verify(x => x.RemoveByPatternAsync("portfolios:.*"), Times.Once);
        }

        [Fact]
        public async Task UpdatePortfolioAsync_ShouldReturnNull_WhenPortfolioDoesNotExist()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var request = new PortfolioUpdateRequest
            {
                Title = "Updated Portfolio"
            };

            _mockPortfolioUpdateValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(new ValidationResult { IsValid = true });

            _mockPortfolioRepository.Setup(x => x.UpdatePortfolioAsync(portfolioId, request))
                .ReturnsAsync((Portfolio?)null);

            // Act
            var result = await _service.UpdatePortfolioAsync(portfolioId, request);

            // Assert
            result.Should().BeNull();
            _mockPortfolioUpdateValidator.Verify(x => x.ValidateAsync(request), Times.Once);
            _mockPortfolioRepository.Verify(x => x.UpdatePortfolioAsync(portfolioId, request), Times.Once);
            _mockPortfolioMapper.Verify(x => x.MapToResponseDto(It.IsAny<Portfolio>()), Times.Never);
        }

        [Fact]
        public async Task UpdatePortfolioAsync_ShouldThrowArgumentException_WhenValidationFails()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var request = new PortfolioUpdateRequest
            {
                Title = ""
            };

            var validationErrors = new List<string> { "Title cannot be empty" };
            _mockPortfolioUpdateValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(new ValidationResult { IsValid = false, Errors = validationErrors });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.UpdatePortfolioAsync(portfolioId, request));

            exception.Message.Should().Contain("Validation failed");
            exception.Message.Should().Contain("Title cannot be empty");

            _mockPortfolioUpdateValidator.Verify(x => x.ValidateAsync(request), Times.Once);
            _mockPortfolioRepository.Verify(x => x.UpdatePortfolioAsync(It.IsAny<Guid>(), It.IsAny<PortfolioUpdateRequest>()), Times.Never);
        }

        [Fact]
        public async Task UpdatePortfolioAsync_ShouldInvalidateCache_WhenBioIsUpdated()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var request = new PortfolioUpdateRequest
            {
                Bio = "Updated Bio" // This triggers cache invalidation
            };

            var updatedPortfolio = TestDataFactory.CreatePortfolio();
            updatedPortfolio.Id = portfolioId;

            var expectedResponse = new PortfolioResponse
            {
                Id = updatedPortfolio.Id,
                Title = updatedPortfolio.Title
            };

            _mockPortfolioUpdateValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(new ValidationResult { IsValid = true });

            _mockPortfolioRepository.Setup(x => x.UpdatePortfolioAsync(portfolioId, request))
                .ReturnsAsync(updatedPortfolio);

            _mockPortfolioMapper.Setup(x => x.MapToResponseDto(updatedPortfolio))
                .Returns(expectedResponse);

            _mockCacheService.Setup(x => x.RemoveByPatternAsync("portfolios_paginated:.*"))
                .Returns(Task.CompletedTask);

            _mockCacheService.Setup(x => x.RemoveByPatternAsync("portfolios:.*"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdatePortfolioAsync(portfolioId, request);

            // Assert
            result.Should().NotBeNull();
            _mockCacheService.Verify(x => x.RemoveByPatternAsync("portfolios_paginated:.*"), Times.Once);
            _mockCacheService.Verify(x => x.RemoveByPatternAsync("portfolios:.*"), Times.Once);
        }

        [Fact]
        public async Task UpdatePortfolioAsync_ShouldNotInvalidateCache_WhenOnlyComponentsIsUpdated()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var request = new PortfolioUpdateRequest
            {
                Components = "Updated Components" // This doesn't trigger cache invalidation
            };

            var updatedPortfolio = TestDataFactory.CreatePortfolio();
            updatedPortfolio.Id = portfolioId;

            var expectedResponse = new PortfolioResponse
            {
                Id = updatedPortfolio.Id,
                Title = updatedPortfolio.Title
            };

            _mockPortfolioUpdateValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(new ValidationResult { IsValid = true });

            _mockPortfolioRepository.Setup(x => x.UpdatePortfolioAsync(portfolioId, request))
                .ReturnsAsync(updatedPortfolio);

            _mockPortfolioMapper.Setup(x => x.MapToResponseDto(updatedPortfolio))
                .Returns(expectedResponse);

            // Act
            var result = await _service.UpdatePortfolioAsync(portfolioId, request);

            // Assert
            result.Should().NotBeNull();
            _mockCacheService.Verify(x => x.RemoveByPatternAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdatePortfolioAsync_ShouldLogErrorAndRethrow_WhenExceptionOccurs()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var request = new PortfolioUpdateRequest
            {
                Title = "Updated Portfolio"
            };

            var expectedException = new InvalidOperationException("Database connection failed");

            _mockPortfolioUpdateValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(new ValidationResult { IsValid = true });

            _mockPortfolioRepository.Setup(x => x.UpdatePortfolioAsync(portfolioId, request))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.UpdatePortfolioAsync(portfolioId, request));

            exception.Should().Be(expectedException);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while updating portfolio")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
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

            _mockCacheService.Setup(x => x.RemoveByPatternAsync("portfolios_paginated:.*"))
                .Returns(Task.CompletedTask);

            _mockCacheService.Setup(x => x.RemoveByPatternAsync("portfolios:.*"))
                .Returns(Task.CompletedTask);

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

        [Fact]
        public async Task DeletePortfolioAsync_ShouldLogErrorAndRethrow_WhenExceptionOccurs()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var expectedException = new InvalidOperationException("Database connection failed");

            _mockPortfolioRepository.Setup(x => x.DeletePortfolioAsync(portfolioId))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.DeletePortfolioAsync(portfolioId));

            exception.Should().Be(expectedException);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while deleting portfolio")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
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

        [Fact]
        public async Task IncrementViewCountAsync_ShouldLogErrorAndRethrow_WhenExceptionOccurs()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var expectedException = new InvalidOperationException("Database connection failed");

            _mockPortfolioRepository.Setup(x => x.IncrementViewCountAsync(portfolioId))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.IncrementViewCountAsync(portfolioId));

            exception.Should().Be(expectedException);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while incrementing view count")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
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

        [Fact]
        public async Task IncrementLikeCountAsync_ShouldLogErrorAndRethrow_WhenExceptionOccurs()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var expectedException = new InvalidOperationException("Database connection failed");

            _mockPortfolioRepository.Setup(x => x.IncrementLikeCountAsync(portfolioId))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.IncrementLikeCountAsync(portfolioId));

            exception.Should().Be(expectedException);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while incrementing like count")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
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

        [Fact]
        public async Task DecrementLikeCountAsync_ShouldLogErrorAndRethrow_WhenExceptionOccurs()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var expectedException = new InvalidOperationException("Database connection failed");

            _mockPortfolioRepository.Setup(x => x.DecrementLikeCountAsync(portfolioId))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.DecrementLikeCountAsync(portfolioId));

            exception.Should().Be(expectedException);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while decrementing like count")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        #endregion

        #region Cache Invalidation Tests

        [Fact]
        public async Task InvalidatePortfolioCacheAsync_ShouldLogSuccess_WhenCacheInvalidationSucceeds()
        {
            // Arrange
            _mockCacheService.Setup(x => x.RemoveByPatternAsync("portfolios_paginated:.*"))
                .Returns(Task.CompletedTask);

            _mockCacheService.Setup(x => x.RemoveByPatternAsync("portfolios:.*"))
                .Returns(Task.CompletedTask);

            // Act - Trigger cache invalidation by creating a published portfolio
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "Test Portfolio",
                IsPublished = true
            };

            var portfolio = TestDataFactory.CreatePortfolio();
            portfolio.IsPublished = true;

            var expectedResponse = new PortfolioResponse
            {
                Id = portfolio.Id,
                Title = portfolio.Title,
                UserId = portfolio.UserId,
                IsPublished = true
            };

            _mockPortfolioValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(new ValidationResult { IsValid = true });

            _mockPortfolioRepository.Setup(x => x.CreatePortfolioAsync(request))
                .ReturnsAsync(portfolio);

            _mockPortfolioMapper.Setup(x => x.MapToResponseDto(portfolio))
                .Returns(expectedResponse);

            await _service.CreatePortfolioAsync(request);

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Portfolio cache invalidated successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task InvalidatePortfolioCacheAsync_ShouldLogWarning_WhenCacheInvalidationFails()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Cache service unavailable");
            _mockCacheService.Setup(x => x.RemoveByPatternAsync("portfolios_paginated:.*"))
                .ThrowsAsync(expectedException);

            // Act - Trigger cache invalidation by creating a published portfolio
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "Test Portfolio",
                IsPublished = true
            };

            var portfolio = TestDataFactory.CreatePortfolio();
            portfolio.IsPublished = true;

            var expectedResponse = new PortfolioResponse
            {
                Id = portfolio.Id,
                Title = portfolio.Title,
                UserId = portfolio.UserId,
                IsPublished = true
            };

            _mockPortfolioValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(new ValidationResult { IsValid = true });

            _mockPortfolioRepository.Setup(x => x.CreatePortfolioAsync(request))
                .ReturnsAsync(portfolio);

            _mockPortfolioMapper.Setup(x => x.MapToResponseDto(portfolio))
                .Returns(expectedResponse);

            await _service.CreatePortfolioAsync(request);

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to invalidate portfolio cache")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        #endregion
    }
} 