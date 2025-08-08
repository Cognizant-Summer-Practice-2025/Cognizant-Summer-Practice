using Xunit;
using FluentAssertions;
using backend_portfolio.Services.Validators;
using backend_portfolio.DTO.Portfolio.Request;
using backend_portfolio.Services.Abstractions;

namespace backend_portfolio.tests.Services
{
    public class PortfolioValidatorTests
    {
        private readonly PortfolioValidator _validator;
        private readonly PortfolioUpdateValidator _updateValidator;

        public PortfolioValidatorTests()
        {
            _validator = new PortfolioValidator();
            _updateValidator = new PortfolioUpdateValidator();
        }

        #region Constructor Tests

        [Fact]
        public void PortfolioValidator_Constructor_ShouldCreateInstance()
        {
            // Act
            var validator = new PortfolioValidator();

            // Assert
            validator.Should().NotBeNull();
        }

        [Fact]
        public void PortfolioUpdateValidator_Constructor_ShouldCreateInstance()
        {
            // Act
            var validator = new PortfolioUpdateValidator();

            // Assert
            validator.Should().NotBeNull();
        }

        #endregion

        #region PortfolioValidator Validate Tests

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenValidPortfolioCreateRequestProvided()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "Valid Portfolio",
                TemplateName = "Default Template"
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
            result.Errors.Should().Contain("Portfolio request cannot be null.");
        }

        [Fact]
        public void Validate_ShouldReturnFailure_WhenUserIdIsEmpty()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.Empty,
                Title = "Valid Portfolio",
                TemplateName = "Default Template"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("User ID is required.");
        }

        [Fact]
        public void Validate_ShouldReturnFailure_WhenTitleIsNull()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = null!,
                TemplateName = "Default Template"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("Portfolio title is required.");
        }

        [Fact]
        public void Validate_ShouldReturnFailure_WhenTitleIsEmpty()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "",
                TemplateName = "Default Template"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("Portfolio title is required.");
        }

        [Fact]
        public void Validate_ShouldReturnFailure_WhenTitleIsWhitespace()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "   ",
                TemplateName = "Default Template"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("Portfolio title is required.");
        }

        [Fact]
        public void Validate_ShouldReturnFailure_WhenTemplateNameIsNull()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "Valid Portfolio",
                TemplateName = null!
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("Template name is required.");
        }

        [Fact]
        public void Validate_ShouldReturnFailure_WhenTemplateNameIsEmpty()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "Valid Portfolio",
                TemplateName = ""
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("Template name is required.");
        }

        [Fact]
        public void Validate_ShouldReturnFailure_WhenTemplateNameIsWhitespace()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "Valid Portfolio",
                TemplateName = "   "
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("Template name is required.");
        }

        [Fact]
        public void Validate_ShouldReturnFailure_WhenTitleExceeds255Characters()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = new string('A', 256),
                TemplateName = "Default Template"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("Portfolio title cannot exceed 255 characters.");
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenTitleIsExactly255Characters()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = new string('A', 255),
                TemplateName = "Default Template"
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
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.Empty,
                Title = "",
                TemplateName = ""
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(3);
            result.Errors.Should().Contain("User ID is required.");
            result.Errors.Should().Contain("Portfolio title is required.");
            result.Errors.Should().Contain("Template name is required.");
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenTitleHasSpecialCharacters()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "Portfolio with special chars: áéíóú ñ & < > \" '",
                TemplateName = "Default Template"
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
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "Portfolio with emojis: 🚀 💻 📱",
                TemplateName = "Default Template"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenTemplateNameHasSpecialCharacters()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "Valid Portfolio",
                TemplateName = "Template with special chars: áéíóú ñ & < > \" '"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        #endregion

        #region PortfolioValidator ValidateAsync Tests

        [Fact]
        public async Task ValidateAsync_ShouldReturnSuccess_WhenValidPortfolioCreateRequestProvided()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "Valid Portfolio",
                TemplateName = "Default Template"
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
            result.Errors.Should().Contain("Portfolio request cannot be null.");
        }

        [Fact]
        public async Task ValidateAsync_ShouldReturnFailure_WhenMultipleValidationsFail()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.Empty,
                Title = "",
                TemplateName = ""
            };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(3);
        }

        #endregion

        #region PortfolioUpdateValidator Validate Tests

        [Fact]
        public void UpdateValidator_Validate_ShouldReturnSuccess_WhenValidPortfolioUpdateRequestProvided()
        {
            // Arrange
            var request = new PortfolioUpdateRequest
            {
                Title = "Updated Portfolio"
            };

            // Act
            var result = _updateValidator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void UpdateValidator_Validate_ShouldReturnSuccess_WhenRequestHasNullValues()
        {
            // Arrange
            var request = new PortfolioUpdateRequest
            {
                Title = null,
                Bio = null,
                Visibility = null,
                IsPublished = null,
                Components = null
            };

            // Act
            var result = _updateValidator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void UpdateValidator_Validate_ShouldReturnFailure_WhenRequestIsNull()
        {
            // Act
            var result = _updateValidator.Validate(null!);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("Portfolio update request cannot be null.");
        }

        [Fact]
        public void UpdateValidator_Validate_ShouldReturnFailure_WhenTitleExceeds255Characters()
        {
            // Arrange
            var request = new PortfolioUpdateRequest
            {
                Title = new string('A', 256)
            };

            // Act
            var result = _updateValidator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("Portfolio title cannot exceed 255 characters.");
        }

        [Fact]
        public void UpdateValidator_Validate_ShouldReturnSuccess_WhenTitleIsExactly255Characters()
        {
            // Arrange
            var request = new PortfolioUpdateRequest
            {
                Title = new string('A', 255)
            };

            // Act
            var result = _updateValidator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void UpdateValidator_Validate_ShouldReturnSuccess_WhenTitleIsEmpty()
        {
            // Arrange
            var request = new PortfolioUpdateRequest
            {
                Title = ""
            };

            // Act
            var result = _updateValidator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void UpdateValidator_Validate_ShouldReturnSuccess_WhenTitleIsWhitespace()
        {
            // Arrange
            var request = new PortfolioUpdateRequest
            {
                Title = "   "
            };

            // Act
            var result = _updateValidator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void UpdateValidator_Validate_ShouldReturnSuccess_WhenTitleHasSpecialCharacters()
        {
            // Arrange
            var request = new PortfolioUpdateRequest
            {
                Title = "Updated Portfolio with special chars: áéíóú ñ & < > \" '"
            };

            // Act
            var result = _updateValidator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void UpdateValidator_Validate_ShouldReturnSuccess_WhenTitleHasEmojis()
        {
            // Arrange
            var request = new PortfolioUpdateRequest
            {
                Title = "Updated Portfolio with emojis: 🚀 💻 📱"
            };

            // Act
            var result = _updateValidator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        #endregion

        #region PortfolioUpdateValidator ValidateAsync Tests

        [Fact]
        public async Task UpdateValidator_ValidateAsync_ShouldReturnSuccess_WhenValidPortfolioUpdateRequestProvided()
        {
            // Arrange
            var request = new PortfolioUpdateRequest
            {
                Title = "Updated Portfolio"
            };

            // Act
            var result = await _updateValidator.ValidateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public async Task UpdateValidator_ValidateAsync_ShouldReturnFailure_WhenRequestIsNull()
        {
            // Act
            var result = await _updateValidator.ValidateAsync(null!);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("Portfolio update request cannot be null.");
        }

        [Fact]
        public async Task UpdateValidator_ValidateAsync_ShouldReturnFailure_WhenTitleExceeds255Characters()
        {
            // Arrange
            var request = new PortfolioUpdateRequest
            {
                Title = new string('A', 256)
            };

            // Act
            var result = await _updateValidator.ValidateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("Portfolio title cannot exceed 255 characters.");
        }

        #endregion

        #region Edge Cases and Performance Tests

        [Fact]
        public void Validate_ShouldHandleVeryLongTitle()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = new string('A', 1000),
                TemplateName = "Default Template"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain("Portfolio title cannot exceed 255 characters.");
        }

        [Fact]
        public void Validate_ShouldHandleVeryLongTemplateName()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "Valid Portfolio",
                TemplateName = new string('A', 1000)
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldHandleUnicodeCharacters()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "作品集标题：React 项目",
                TemplateName = "模板名称：默认模板"
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
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "Portfolio Title with Mixed Case: UPPER lower MiXeD",
                TemplateName = "Template Name with Mixed Case: UPPER lower MiXeD"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldHandleNumbersInTitle()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "Portfolio 2023 - Version 2.1",
                TemplateName = "Template 2023"
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldHandlePunctuationInTitle()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "Portfolio: A Developer's Journey (2023)",
                TemplateName = "Template: Professional"
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
                .Select(i => new PortfolioCreateRequest
                {
                    UserId = Guid.NewGuid(),
                    Title = $"Portfolio {i}",
                    TemplateName = "Default Template"
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
                .Select(i => new PortfolioCreateRequest
                {
                    UserId = Guid.NewGuid(),
                    Title = $"Portfolio {i}",
                    TemplateName = "Default Template"
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
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = new string('A', 255),
                TemplateName = new string('B', 1000)
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
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = new string('A', 255),
                TemplateName = new string('B', 1000)
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