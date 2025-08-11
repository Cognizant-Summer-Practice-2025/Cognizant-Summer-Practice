using Xunit;
using FluentAssertions;
using backend_portfolio.Models;
using System.ComponentModel.DataAnnotations;

namespace backend_portfolio.tests.Models
{
    public class SkillModelTests
    {
        [Fact]
        public void Skill_ShouldCreateInstance_WithDefaultValues()
        {
            // Arrange & Act
            var skill = new Skill();

            // Assert
            skill.Should().NotBeNull();
            skill.Id.Should().NotBe(Guid.Empty);
            skill.Name.Should().Be(string.Empty);
            skill.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            skill.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void Skill_ShouldSetProperties_WhenValidDataProvided()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var name = "C#";
            var categoryType = "Programming";
            var subcategory = "Backend";
            var category = "Languages";
            var proficiencyLevel = 5;
            var displayOrder = 1;

            // Act
            var skill = new Skill
            {
                PortfolioId = portfolioId,
                Name = name,
                CategoryType = categoryType,
                Subcategory = subcategory,
                Category = category,
                ProficiencyLevel = proficiencyLevel,
                DisplayOrder = displayOrder
            };

            // Assert
            skill.PortfolioId.Should().Be(portfolioId);
            skill.Name.Should().Be(name);
            skill.CategoryType.Should().Be(categoryType);
            skill.Subcategory.Should().Be(subcategory);
            skill.Category.Should().Be(category);
            skill.ProficiencyLevel.Should().Be(proficiencyLevel);
            skill.DisplayOrder.Should().Be(displayOrder);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Skill_ShouldFailValidation_WhenNameIsInvalid(string invalidName)
        {
            // Arrange
            var skill = new Skill
            {
                PortfolioId = Guid.NewGuid(),
                Name = invalidName
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(skill);
            var isValid = Validator.TryValidateObject(skill, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("Name"));
        }

        [Fact]
        public void Skill_ShouldFailValidation_WhenNameExceedsMaxLength()
        {
            // Arrange
            var longName = new string('a', 101); // Exceeds 100 character limit
            var skill = new Skill
            {
                PortfolioId = Guid.NewGuid(),
                Name = longName
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(skill);
            var isValid = Validator.TryValidateObject(skill, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("Name"));
        }

        [Fact]
        public void Skill_ShouldFailValidation_WhenCategoryTypeExceedsMaxLength()
        {
            // Arrange
            var longCategoryType = new string('a', 51); // Exceeds 50 character limit
            var skill = new Skill
            {
                PortfolioId = Guid.NewGuid(),
                Name = "Valid Name",
                CategoryType = longCategoryType
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(skill);
            var isValid = Validator.TryValidateObject(skill, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("CategoryType"));
        }

        [Fact]
        public void Skill_ShouldFailValidation_WhenSubcategoryExceedsMaxLength()
        {
            // Arrange
            var longSubcategory = new string('a', 101); // Exceeds 100 character limit
            var skill = new Skill
            {
                PortfolioId = Guid.NewGuid(),
                Name = "Valid Name",
                Subcategory = longSubcategory
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(skill);
            var isValid = Validator.TryValidateObject(skill, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("Subcategory"));
        }

        [Fact]
        public void Skill_ShouldFailValidation_WhenCategoryExceedsMaxLength()
        {
            // Arrange
            var longCategory = new string('a', 256); // Exceeds 255 character limit
            var skill = new Skill
            {
                PortfolioId = Guid.NewGuid(),
                Name = "Valid Name",
                Category = longCategory
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(skill);
            var isValid = Validator.TryValidateObject(skill, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("Category"));
        }

        [Fact]
        public void Skill_ShouldPassValidation_WhenRequiredFieldsAreProvided()
        {
            // Arrange
            var skill = new Skill
            {
                PortfolioId = Guid.NewGuid(),
                Name = "JavaScript"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(skill);
            var isValid = Validator.TryValidateObject(skill, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void Skill_ShouldAllowNullOptionalFields()
        {
            // Arrange & Act
            var skill = new Skill
            {
                PortfolioId = Guid.NewGuid(),
                Name = "Python",
                CategoryType = null,
                Subcategory = null,
                Category = null,
                ProficiencyLevel = null,
                DisplayOrder = null
            };

            // Assert
            skill.CategoryType.Should().BeNull();
            skill.Subcategory.Should().BeNull();
            skill.Category.Should().BeNull();
            skill.ProficiencyLevel.Should().BeNull();
            skill.DisplayOrder.Should().BeNull();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public void Skill_ShouldSetProficiencyLevel_WhenValidValueProvided(int proficiencyLevel)
        {
            // Arrange & Act
            var skill = new Skill
            {
                PortfolioId = Guid.NewGuid(),
                Name = "Java",
                ProficiencyLevel = proficiencyLevel
            };

            // Assert
            skill.ProficiencyLevel.Should().Be(proficiencyLevel);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        [InlineData(100)]
        public void Skill_ShouldSetDisplayOrder_WhenValidValueProvided(int displayOrder)
        {
            // Arrange & Act
            var skill = new Skill
            {
                PortfolioId = Guid.NewGuid(),
                Name = "React",
                DisplayOrder = displayOrder
            };

            // Assert
            skill.DisplayOrder.Should().Be(displayOrder);
        }

        [Fact]
        public void Skill_ShouldInitializeWithNullNavigationProperty()
        {
            // Arrange & Act
            var skill = new Skill();

            // Assert
            // Reference navigation properties should be null until loaded by EF
            skill.Portfolio.Should().BeNull();
        }
    }
} 