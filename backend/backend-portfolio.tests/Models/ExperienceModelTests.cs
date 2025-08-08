using Xunit;
using FluentAssertions;
using backend_portfolio.Models;
using System.ComponentModel.DataAnnotations;

namespace backend_portfolio.tests.Models
{
    public class ExperienceModelTests
    {
        [Fact]
        public void Experience_ShouldCreateInstance_WithDefaultValues()
        {
            // Arrange & Act
            var experience = new Experience();

            // Assert
            experience.Should().NotBeNull();
            experience.Id.Should().NotBe(Guid.Empty);
            experience.JobTitle.Should().Be(string.Empty);
            experience.CompanyName.Should().Be(string.Empty);
            experience.IsCurrent.Should().BeFalse();
            experience.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            experience.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void Experience_ShouldSetProperties_WhenValidDataProvided()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var jobTitle = "Software Engineer";
            var companyName = "Tech Corp";
            var startDate = new DateOnly(2022, 1, 15);
            var endDate = new DateOnly(2023, 6, 30);
            var description = "Developed web applications using .NET";
            var skillsUsed = new[] { "C#", ".NET", "SQL" };

            // Act
            var experience = new Experience
            {
                PortfolioId = portfolioId,
                JobTitle = jobTitle,
                CompanyName = companyName,
                StartDate = startDate,
                EndDate = endDate,
                IsCurrent = false,
                Description = description,
                SkillsUsed = skillsUsed
            };

            // Assert
            experience.PortfolioId.Should().Be(portfolioId);
            experience.JobTitle.Should().Be(jobTitle);
            experience.CompanyName.Should().Be(companyName);
            experience.StartDate.Should().Be(startDate);
            experience.EndDate.Should().Be(endDate);
            experience.IsCurrent.Should().BeFalse();
            experience.Description.Should().Be(description);
            experience.SkillsUsed.Should().BeEquivalentTo(skillsUsed);
        }

        [Fact]
        public void Experience_ShouldSetCurrentPosition_WhenIsCurrentIsTrue()
        {
            // Arrange & Act
            var experience = new Experience
            {
                PortfolioId = Guid.NewGuid(),
                JobTitle = "Senior Developer",
                CompanyName = "Current Company",
                StartDate = new DateOnly(2023, 1, 1),
                IsCurrent = true,
                EndDate = null
            };

            // Assert
            experience.IsCurrent.Should().BeTrue();
            experience.EndDate.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Experience_ShouldFailValidation_WhenJobTitleIsInvalid(string invalidJobTitle)
        {
            // Arrange
            var experience = new Experience
            {
                PortfolioId = Guid.NewGuid(),
                JobTitle = invalidJobTitle,
                CompanyName = "Valid Company"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(experience);
            var isValid = Validator.TryValidateObject(experience, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("JobTitle"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Experience_ShouldFailValidation_WhenCompanyNameIsInvalid(string invalidCompanyName)
        {
            // Arrange
            var experience = new Experience
            {
                PortfolioId = Guid.NewGuid(),
                JobTitle = "Valid Title",
                CompanyName = invalidCompanyName
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(experience);
            var isValid = Validator.TryValidateObject(experience, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("CompanyName"));
        }

        [Fact]
        public void Experience_ShouldFailValidation_WhenJobTitleExceedsMaxLength()
        {
            // Arrange
            var longJobTitle = new string('a', 256); // Exceeds 255 character limit
            var experience = new Experience
            {
                PortfolioId = Guid.NewGuid(),
                JobTitle = longJobTitle,
                CompanyName = "Valid Company"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(experience);
            var isValid = Validator.TryValidateObject(experience, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("JobTitle"));
        }

        [Fact]
        public void Experience_ShouldFailValidation_WhenCompanyNameExceedsMaxLength()
        {
            // Arrange
            var longCompanyName = new string('a', 256); // Exceeds 255 character limit
            var experience = new Experience
            {
                PortfolioId = Guid.NewGuid(),
                JobTitle = "Valid Title",
                CompanyName = longCompanyName
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(experience);
            var isValid = Validator.TryValidateObject(experience, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("CompanyName"));
        }

        [Fact]
        public void Experience_ShouldPassValidation_WhenRequiredFieldsAreProvided()
        {
            // Arrange
            var experience = new Experience
            {
                PortfolioId = Guid.NewGuid(),
                JobTitle = "Software Developer",
                CompanyName = "Tech Company"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(experience);
            var isValid = Validator.TryValidateObject(experience, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void Experience_ShouldAllowNullOptionalFields()
        {
            // Arrange & Act
            var experience = new Experience
            {
                PortfolioId = Guid.NewGuid(),
                JobTitle = "Developer",
                CompanyName = "Company",
                Description = null,
                SkillsUsed = null,
                EndDate = null
            };

            // Assert
            experience.Description.Should().BeNull();
            experience.SkillsUsed.Should().BeNull();
            experience.EndDate.Should().BeNull();
        }

        [Fact]
        public void Experience_ShouldInitializeWithNullNavigationProperty()
        {
            // Arrange & Act
            var experience = new Experience();

            // Assert
            // Reference navigation properties should be null until loaded by EF
            experience.Portfolio.Should().BeNull();
        }
    }
} 