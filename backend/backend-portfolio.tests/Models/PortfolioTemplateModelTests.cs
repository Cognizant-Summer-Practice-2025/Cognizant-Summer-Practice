using Xunit;
using FluentAssertions;
using backend_portfolio.Models;
using System.ComponentModel.DataAnnotations;

namespace backend_portfolio.tests.Models
{
    public class PortfolioTemplateModelTests
    {
        [Fact]
        public void PortfolioTemplate_ShouldCreateInstance_WithDefaultValues()
        {
            // Arrange & Act
            var template = new PortfolioTemplate();

            // Assert
            template.Should().NotBeNull();
            template.Id.Should().NotBe(Guid.Empty);
            template.Name.Should().Be(string.Empty);
            template.IsActive.Should().BeTrue();
            template.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            template.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            template.Portfolios.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void PortfolioTemplate_ShouldSetProperties_WhenValidDataProvided()
        {
            // Arrange
            var name = "Modern Template";
            var description = "A modern portfolio template with responsive design";
            var previewImageUrl = "https://example.com/preview.jpg";
            var isActive = false;

            // Act
            var template = new PortfolioTemplate
            {
                Name = name,
                Description = description,
                PreviewImageUrl = previewImageUrl,
                IsActive = isActive
            };

            // Assert
            template.Name.Should().Be(name);
            template.Description.Should().Be(description);
            template.PreviewImageUrl.Should().Be(previewImageUrl);
            template.IsActive.Should().Be(isActive);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void PortfolioTemplate_ShouldFailValidation_WhenNameIsInvalid(string invalidName)
        {
            // Arrange
            var template = new PortfolioTemplate
            {
                Name = invalidName
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(template);
            var isValid = Validator.TryValidateObject(template, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("Name"));
        }

        [Fact]
        public void PortfolioTemplate_ShouldFailValidation_WhenNameExceedsMaxLength()
        {
            // Arrange
            var longName = new string('a', 101); // Exceeds 100 character limit
            var template = new PortfolioTemplate
            {
                Name = longName
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(template);
            var isValid = Validator.TryValidateObject(template, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("Name"));
        }

        [Fact]
        public void PortfolioTemplate_ShouldPassValidation_WhenRequiredFieldsAreProvided()
        {
            // Arrange
            var template = new PortfolioTemplate
            {
                Name = "Valid Template Name"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(template);
            var isValid = Validator.TryValidateObject(template, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void PortfolioTemplate_ShouldAllowNullOptionalFields()
        {
            // Arrange & Act
            var template = new PortfolioTemplate
            {
                Name = "Basic Template",
                Description = null,
                PreviewImageUrl = null
            };

            // Assert
            template.Description.Should().BeNull();
            template.PreviewImageUrl.Should().BeNull();
        }

        [Fact]
        public void PortfolioTemplate_ShouldSetActiveStatus_WhenSpecified()
        {
            // Arrange & Act
            var activeTemplate = new PortfolioTemplate
            {
                Name = "Active Template",
                IsActive = true
            };

            var inactiveTemplate = new PortfolioTemplate
            {
                Name = "Inactive Template",
                IsActive = false
            };

            // Assert
            activeTemplate.IsActive.Should().BeTrue();
            inactiveTemplate.IsActive.Should().BeFalse();
        }

        [Fact]
        public void PortfolioTemplate_ShouldInitializePortfoliosCollection()
        {
            // Arrange & Act
            var template = new PortfolioTemplate();

            // Assert
            template.Portfolios.Should().NotBeNull();
            template.Portfolios.Should().BeOfType<List<Portfolio>>();
            template.Portfolios.Should().BeEmpty();
        }

        [Fact]
        public void PortfolioTemplate_ShouldAllowPortfoliosToBeAdded()
        {
            // Arrange
            var template = new PortfolioTemplate
            {
                Name = "Test Template"
            };

            var portfolio = new Portfolio
            {
                Title = "Test Portfolio",
                UserId = Guid.NewGuid(),
                TemplateId = template.Id
            };

            // Act
            template.Portfolios.Add(portfolio);

            // Assert
            template.Portfolios.Should().HaveCount(1);
            template.Portfolios.Should().Contain(portfolio);
        }

        [Theory]
        [InlineData("Classic")]
        [InlineData("Modern Portfolio")]
        [InlineData("Creative Design Template")]
        public void PortfolioTemplate_ShouldAcceptValidNames(string validName)
        {
            // Arrange & Act
            var template = new PortfolioTemplate
            {
                Name = validName
            };

            // Assert
            template.Name.Should().Be(validName);
        }

        [Fact]
        public void PortfolioTemplate_ShouldSetTimestamps_WhenCreated()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow;

            // Act
            var template = new PortfolioTemplate
            {
                Name = "Timestamp Test"
            };

            var afterCreation = DateTime.UtcNow;

            // Assert
            template.CreatedAt.Should().BeOnOrAfter(beforeCreation).And.BeOnOrBefore(afterCreation);
            template.UpdatedAt.Should().BeOnOrAfter(beforeCreation).And.BeOnOrBefore(afterCreation);
        }
    }
} 