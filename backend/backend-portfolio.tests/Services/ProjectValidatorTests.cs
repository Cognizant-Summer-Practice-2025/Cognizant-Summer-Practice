using Xunit;
using FluentAssertions;
using backend_portfolio.Services.Validators;
using backend_portfolio.DTO.Project.Request;
using backend_portfolio.Services.Abstractions;

namespace backend_portfolio.tests.Services
{
    public class ProjectValidatorTests
    {
        private readonly ProjectValidator _validator;

        public ProjectValidatorTests()
        {
            _validator = new ProjectValidator();
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            // Act
            var validator = new ProjectValidator();

            // Assert
            validator.Should().NotBeNull();
        }

        #endregion

        #region Validate Tests

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenValidProjectCreateRequestProvided()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Valid Project",
                Description = "Valid Description",
                Technologies = new string[] { "React", "Node.js", "MongoDB" },
                GithubUrl = "https://github.com/user/project",
                DemoUrl = "https://project.com",
                ImageUrl = "https://example.com/project-image.jpg",
                Featured = true
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenValidProjectCreateRequestWithNullOptionalFieldsProvided()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Valid Project",
                Description = null,
                Technologies = null,
                GithubUrl = null,
                DemoUrl = null,
                ImageUrl = null,
                Featured = false
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenValidProjectCreateRequestWithEmptyOptionalFieldsProvided()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Valid Project",
                Description = "",
                Technologies = new string[] { },
                GithubUrl = "",
                DemoUrl = "",
                ImageUrl = "",
                Featured = false
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldReturnFailure_WhenRequestIsNull()
        {
            // Act
            var result = _validator.Validate(null!);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("Project request cannot be null.");
        }

        [Fact]
        public void Validate_ShouldReturnFailure_WhenPortfolioIdIsEmpty()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.Empty,
                Title = "Valid Project"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("Portfolio ID is required.");
        }

        [Fact]
        public void Validate_ShouldReturnFailure_WhenTitleIsNull()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = null!
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("Project title is required.");
        }

        [Fact]
        public void Validate_ShouldReturnFailure_WhenTitleIsEmpty()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = ""
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("Project title is required.");
        }

        [Fact]
        public void Validate_ShouldReturnFailure_WhenTitleIsWhitespace()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "   "
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("Project title is required.");
        }

        [Fact]
        public void Validate_ShouldReturnFailure_WhenTitleExceeds255Characters()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = new string('A', 256)
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("Project title cannot exceed 255 characters.");
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenTitleIsExactly255Characters()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = new string('A', 255)
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldReturnFailure_WhenDemoUrlIsInvalid()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Valid Project",
                DemoUrl = "invalid-url"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("Demo URL must be a valid URL.");
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenDemoUrlIsValid()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Valid Project",
                DemoUrl = "https://project.com"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldReturnFailure_WhenGithubUrlIsInvalid()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Valid Project",
                GithubUrl = "invalid-url"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("GitHub URL must be a valid URL.");
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenGithubUrlIsValid()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Valid Project",
                GithubUrl = "https://github.com/user/project"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldReturnFailure_WhenImageUrlIsInvalid()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Valid Project",
                ImageUrl = "ht@tp://invalid<>url"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("Image URL must be a valid URL or server path.");
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenImageUrlIsValidHttpUrl()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Valid Project",
                ImageUrl = "https://example.com/image.jpg"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenImageUrlIsValidRelativeUrl()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Valid Project",
                ImageUrl = "/images/project.jpg"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenImageUrlIsServerPath()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Valid Project",
                ImageUrl = "server/portfolio/projects/image.jpg"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldReturnMultipleErrors_WhenMultipleValidationsFail()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.Empty,
                Title = "",
                DemoUrl = "invalid-url",
                GithubUrl = "invalid-url",
                ImageUrl = "invalid-url"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(4);
            result.Errors.Should().Contain("Portfolio ID is required.");
            result.Errors.Should().Contain("Project title is required.");
            result.Errors.Should().Contain("Demo URL must be a valid URL.");
            result.Errors.Should().Contain("GitHub URL must be a valid URL.");
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenTitleHasSpecialCharacters()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Project with special chars: áéíóú ñ & < > \" '"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenTitleHasEmojis()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Project with emojis: 🚀 💻 📱"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenDescriptionHasSpecialCharacters()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Valid Project",
                Description = "Description with special chars: áéíóú ñ & < > \" '"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenTechnologiesHasSpecialCharacters()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Valid Project",
                Technologies = new string[] { "React", "Node.js", "MongoDB", "C#", ".NET" }
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenUrlsAreValid()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Valid Project",
                GithubUrl = "https://github.com/user/project-with-special-chars",
                DemoUrl = "https://project-with-special-chars.com",
                ImageUrl = "https://example.com/project-image.jpg?size=large"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        #endregion

        #region ValidateAsync Tests

        [Fact]
        public async Task ValidateAsync_ShouldReturnSuccess_WhenValidProjectCreateRequestProvided()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Valid Project",
                Description = "Valid Description",
                Technologies = new string[] { "React", "Node.js", "MongoDB" },
                GithubUrl = "https://github.com/user/project",
                DemoUrl = "https://project.com",
                ImageUrl = "https://example.com/project-image.jpg",
                Featured = true
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public async Task ValidateAsync_ShouldReturnFailure_WhenRequestIsNull()
        {
            // Act
            var result = await _validator.ValidateAsync(null!);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("Project request cannot be null.");
        }

        [Fact]
        public async Task ValidateAsync_ShouldReturnFailure_WhenMultipleValidationsFail()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.Empty,
                Title = "",
                DemoUrl = "invalid-url",
                GithubUrl = "invalid-url"
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(4);
        }

        #endregion

        #region Edge Cases and Performance Tests

        [Fact]
        public void Validate_ShouldHandleVeryLongTitle()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = new string('A', 1000)
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("Project title cannot exceed 255 characters.");
        }

        [Fact]
        public void Validate_ShouldHandleVeryLongDescription()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Valid Project",
                Description = new string('A', 2000)
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldHandleVeryLongUrls()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Valid Project",
                GithubUrl = new string('A', 1000),
                DemoUrl = new string('A', 1000),
                ImageUrl = new string('A', 1000)
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(2);
            result.Errors.Should().Contain("GitHub URL must be a valid URL.");
            result.Errors.Should().Contain("Demo URL must be a valid URL.");
        }

        [Fact]
        public void Validate_ShouldHandleUnicodeCharacters()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "项目标题：React 项目",
                Description = "项目描述：这是一个使用 React 和 Node.js 构建的现代化 Web 应用程序",
                Technologies = new string[] { "React", "Node.js", "MongoDB", "中文支持" }
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldHandleMixedCaseCharacters()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Project Title with Mixed Case: UPPER lower MiXeD",
                Description = "Description with Mixed Case: UPPER lower MiXeD",
                Technologies = new string[] { "React", "Node.js", "MongoDB" }
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldHandleNumbersInFields()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Project 2023 - Version 2.1",
                Description = "Description with numbers: Version 1.0, API v2, etc.",
                Technologies = new string[] { "React 18", "Node.js 16", "MongoDB 5.0" }
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldHandlePunctuationInFields()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Project: A Developer's Journey (2023)",
                Description = "Description with punctuation: This is a project about...",
                Technologies = new string[] { "React", "Node.js", "MongoDB", "etc." }
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldHandleConcurrentValidation()
        {
            // Arrange
            var requests = Enumerable.Range(0, 100)
                .Select(i => new ProjectCreateRequest
                {
                    PortfolioId = Guid.NewGuid(),
                    Title = $"Project {i}",
                    Description = $"Description for project {i}",
                    Technologies = new string[] { $"Technology {i}" }
                })
                .ToList();

            // Act
            var results = requests.Select(r => _validator.Validate(r)).ToList();

            // Assert
            results.Should().HaveCount(100);
            results.Should().AllSatisfy(result => result.IsValid.Should().BeTrue());
        }

        [Fact]
        public async Task ValidateAsync_ShouldHandleConcurrentValidation()
        {
            // Arrange
            var requests = Enumerable.Range(0, 100)
                .Select(i => new ProjectCreateRequest
                {
                    PortfolioId = Guid.NewGuid(),
                    Title = $"Project {i}",
                    Description = $"Description for project {i}",
                    Technologies = new string[] { $"Technology {i}" }
                })
                .ToList();

            // Act
            var tasks = requests.Select(r => _validator.ValidateAsync(r));
            var results = await Task.WhenAll(tasks);

            // Assert
            results.Should().HaveCount(100);
            results.Should().AllSatisfy(result => result.IsValid.Should().BeTrue());
        }

        [Fact]
        public void Validate_ShouldHandlePerformanceWithLargeInput()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = new string('A', 255),
                Description = new string('B', 1000),
                Technologies = new string[] { "React", "Node.js", "MongoDB" },
                GithubUrl = "https://github.com/user/project",
                DemoUrl = "https://project.com",
                ImageUrl = "https://example.com/project-image.jpg"
            };

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = _validator.Validate(request);
            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Should complete within 100ms
        }

        [Fact]
        public async Task ValidateAsync_ShouldHandlePerformanceWithLargeInput()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = new string('A', 255),
                Description = new string('B', 1000),
                Technologies = new string[] { "React", "Node.js", "MongoDB" },
                GithubUrl = "https://github.com/user/project",
                DemoUrl = "https://project.com",
                ImageUrl = "https://example.com/project-image.jpg"
            };

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = await _validator.ValidateAsync(request);
            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Should complete within 100ms
        }

        #endregion
    }
} 