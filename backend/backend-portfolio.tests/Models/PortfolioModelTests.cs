using Xunit;
using FluentAssertions;
using backend_portfolio.Models;
using backend_portfolio.tests.Helpers;
using System.ComponentModel.DataAnnotations;

namespace backend_portfolio.tests.Models
{
    public class PortfolioModelTests
    {
        #region Constructor Tests

        [Fact]
        public void Portfolio_ShouldBeInstantiable()
        {
            // Act
            var portfolio = new Portfolio();

            // Assert
            portfolio.Should().NotBeNull();
            portfolio.Projects.Should().NotBeNull();
            portfolio.Projects.Should().BeEmpty();
            portfolio.Experience.Should().NotBeNull();
            portfolio.Experience.Should().BeEmpty();
            portfolio.Skills.Should().NotBeNull();
            portfolio.Skills.Should().BeEmpty();
            portfolio.BlogPosts.Should().NotBeNull();
            portfolio.BlogPosts.Should().BeEmpty();
            portfolio.Bookmarks.Should().NotBeNull();
            portfolio.Bookmarks.Should().BeEmpty();
        }

        [Fact]
        public void Portfolio_ShouldCreateInstance_WithDefaultValues()
        {
            // Arrange & Act
            var portfolio = new Portfolio();

            // Assert
            portfolio.Should().NotBeNull();
            portfolio.Id.Should().NotBe(Guid.Empty);
            portfolio.Title.Should().Be(string.Empty);
            portfolio.ViewCount.Should().Be(0);
            portfolio.LikeCount.Should().Be(0);
            portfolio.Visibility.Should().Be(Visibility.Public);
            portfolio.IsPublished.Should().BeFalse();
            portfolio.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            portfolio.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        #endregion

        #region Property Tests

        [Fact]
        public void Portfolio_ShouldHaveValidProperties()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();

            // Assert
            portfolio.Id.Should().NotBe(Guid.Empty);
            portfolio.UserId.Should().NotBe(Guid.Empty);
            portfolio.Title.Should().NotBeNullOrWhiteSpace();
            portfolio.Visibility.Should().Be(Visibility.Public);
            portfolio.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            portfolio.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Portfolio_Title_ShouldHandleInvalidValues(string? invalidTitle)
        {
            // Arrange
            var portfolio = new Portfolio();

            // Act
            portfolio.Title = invalidTitle!;

            // Assert
            portfolio.Title.Should().Be(invalidTitle);
        }

        [Fact]
        public void Portfolio_UserId_ShouldAcceptValidValues()
        {
            // Arrange
            var portfolio = new Portfolio();
            var validUserId = Guid.NewGuid();

            // Act
            portfolio.UserId = validUserId;

            // Assert
            portfolio.UserId.Should().Be(validUserId);
        }

        [Fact]
        public void Portfolio_Visibility_ShouldDefaultToPublic()
        {
            // Arrange & Act
            var portfolio = new Portfolio();

            // Assert
            portfolio.Visibility.Should().Be(Visibility.Public);
        }

        [Fact]
        public void Portfolio_ShouldSetProperties_WhenValidDataProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var templateId = Guid.NewGuid();
            var title = "My Awesome Portfolio";
            var bio = "I'm a software developer passionate about creating amazing applications";
            var components = "{}"; // JSON string

            // Act
            var portfolio = new Portfolio
            {
                UserId = userId,
                TemplateId = templateId,
                Title = title,
                Bio = bio,
                ViewCount = 100,
                LikeCount = 25,
                Visibility = Visibility.Private,
                IsPublished = true,
                Components = components
            };

            // Assert
            portfolio.UserId.Should().Be(userId);
            portfolio.TemplateId.Should().Be(templateId);
            portfolio.Title.Should().Be(title);
            portfolio.Bio.Should().Be(bio);
            portfolio.ViewCount.Should().Be(100);
            portfolio.LikeCount.Should().Be(25);
            portfolio.Visibility.Should().Be(Visibility.Private);
            portfolio.IsPublished.Should().BeTrue();
            portfolio.Components.Should().Be(components);
        }

        [Theory]
        [InlineData(Visibility.Public)]
        [InlineData(Visibility.Private)]
        [InlineData(Visibility.Unlisted)]
        public void Portfolio_ShouldSetVisibility_WhenValidVisibilityProvided(Visibility visibility)
        {
            // Arrange & Act
            var portfolio = new Portfolio
            {
                UserId = Guid.NewGuid(),
                TemplateId = Guid.NewGuid(),
                Title = "Test Portfolio",
                Visibility = visibility
            };

            // Assert
            portfolio.Visibility.Should().Be(visibility);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(9999)]
        public void Portfolio_ShouldSetViewCount_WhenValidValueProvided(int viewCount)
        {
            // Arrange & Act
            var portfolio = new Portfolio
            {
                UserId = Guid.NewGuid(),
                TemplateId = Guid.NewGuid(),
                Title = "Test Portfolio",
                ViewCount = viewCount
            };

            // Assert
            portfolio.ViewCount.Should().Be(viewCount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(50)]
        [InlineData(1000)]
        public void Portfolio_ShouldSetLikeCount_WhenValidValueProvided(int likeCount)
        {
            // Arrange & Act
            var portfolio = new Portfolio
            {
                UserId = Guid.NewGuid(),
                TemplateId = Guid.NewGuid(),
                Title = "Test Portfolio",
                LikeCount = likeCount
            };

            // Assert
            portfolio.LikeCount.Should().Be(likeCount);
        }

        #endregion

        #region Validation Tests

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Portfolio_ShouldFailValidation_WhenTitleIsInvalid(string invalidTitle)
        {
            // Arrange
            var portfolio = new Portfolio
            {
                UserId = Guid.NewGuid(),
                TemplateId = Guid.NewGuid(),
                Title = invalidTitle
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(portfolio);
            var isValid = Validator.TryValidateObject(portfolio, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("Title"));
        }

        [Fact]
        public void Portfolio_ShouldFailValidation_WhenTitleExceedsMaxLength()
        {
            // Arrange
            var longTitle = new string('a', 256); // Exceeds 255 character limit
            var portfolio = new Portfolio
            {
                UserId = Guid.NewGuid(),
                TemplateId = Guid.NewGuid(),
                Title = longTitle
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(portfolio);
            var isValid = Validator.TryValidateObject(portfolio, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("Title"));
        }

        [Fact]
        public void Portfolio_ShouldFailValidation_WhenUserIdIsEmpty()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                UserId = Guid.Empty,
                TemplateId = Guid.NewGuid(),
                Title = "Valid Title"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(portfolio);
            var isValid = Validator.TryValidateObject(portfolio, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("UserId"));
        }

        [Fact]
        public void Portfolio_ShouldFailValidation_WhenTemplateIdIsEmpty()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                UserId = Guid.NewGuid(),
                TemplateId = Guid.Empty,
                Title = "Valid Title"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(portfolio);
            var isValid = Validator.TryValidateObject(portfolio, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("TemplateId"));
        }

        [Fact]
        public void Portfolio_ShouldPassValidation_WhenRequiredFieldsAreProvided()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                UserId = Guid.NewGuid(),
                TemplateId = Guid.NewGuid(),
                Title = "Valid Portfolio Title"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(portfolio);
            var isValid = Validator.TryValidateObject(portfolio, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void Portfolio_ShouldAllowNullOptionalFields()
        {
            // Arrange & Act
            var portfolio = new Portfolio
            {
                UserId = Guid.NewGuid(),
                TemplateId = Guid.NewGuid(),
                Title = "Test Portfolio",
                Bio = null,
                Components = null
            };

            // Assert
            portfolio.Bio.Should().BeNull();
            portfolio.Components.Should().BeNull();
        }

        [Fact]
        public void Portfolio_ShouldValidateRelationships()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            var project = TestDataFactory.CreateProject(portfolio.Id);
            var experience = TestDataFactory.CreateExperience(portfolio.Id);
            var skill = TestDataFactory.CreateSkill(portfolio.Id);
            var blogPost = TestDataFactory.CreateBlogPost(portfolio.Id);

            // Act
            portfolio.Projects.Add(project);
            portfolio.Experience.Add(experience);
            portfolio.Skills.Add(skill);
            portfolio.BlogPosts.Add(blogPost);

            // Assert
            project.PortfolioId.Should().Be(portfolio.Id);
            experience.PortfolioId.Should().Be(portfolio.Id);
            skill.PortfolioId.Should().Be(portfolio.Id);
            blogPost.PortfolioId.Should().Be(portfolio.Id);
        }

        #endregion

        #region Navigation Property Tests

        [Fact]
        public void Portfolio_Projects_ShouldInitializeAsEmptyCollection()
        {
            // Arrange & Act
            var portfolio = new Portfolio();

            // Assert
            portfolio.Projects.Should().NotBeNull();
            portfolio.Projects.Should().BeEmpty();
        }

        [Fact]
        public void Portfolio_Experience_ShouldInitializeAsEmptyCollection()
        {
            // Arrange & Act
            var portfolio = new Portfolio();

            // Assert
            portfolio.Experience.Should().NotBeNull();
            portfolio.Experience.Should().BeEmpty();
        }

        [Fact]
        public void Portfolio_Skills_ShouldInitializeAsEmptyCollection()
        {
            // Arrange & Act
            var portfolio = new Portfolio();

            // Assert
            portfolio.Skills.Should().NotBeNull();
            portfolio.Skills.Should().BeEmpty();
        }

        [Fact]
        public void Portfolio_BlogPosts_ShouldInitializeAsEmptyCollection()
        {
            // Arrange & Act
            var portfolio = new Portfolio();

            // Assert
            portfolio.BlogPosts.Should().NotBeNull();
            portfolio.BlogPosts.Should().BeEmpty();
        }

        [Fact]
        public void Portfolio_Bookmarks_ShouldInitializeAsEmptyCollection()
        {
            // Arrange & Act
            var portfolio = new Portfolio();

            // Assert
            portfolio.Bookmarks.Should().NotBeNull();
            portfolio.Bookmarks.Should().BeEmpty();
        }

        [Fact]
        public void Portfolio_ShouldAllowAddingProjects()
        {
            // Arrange
            var portfolio = new Portfolio();
            var project = TestDataFactory.CreateProject();

            // Act
            portfolio.Projects.Add(project);

            // Assert
            portfolio.Projects.Should().HaveCount(1);
            portfolio.Projects.Should().Contain(project);
        }

        [Fact]
        public void Portfolio_ShouldAllowAddingExperience()
        {
            // Arrange
            var portfolio = new Portfolio();
            var experience = TestDataFactory.CreateExperience();

            // Act
            portfolio.Experience.Add(experience);

            // Assert
            portfolio.Experience.Should().HaveCount(1);
            portfolio.Experience.Should().Contain(experience);
        }

        [Fact]
        public void Portfolio_ShouldAllowAddingSkills()
        {
            // Arrange
            var portfolio = new Portfolio();
            var skill = TestDataFactory.CreateSkill();

            // Act
            portfolio.Skills.Add(skill);

            // Assert
            portfolio.Skills.Should().HaveCount(1);
            portfolio.Skills.Should().Contain(skill);
        }

        [Fact]
        public void Portfolio_ShouldAllowAddingBlogPosts()
        {
            // Arrange
            var portfolio = new Portfolio();
            var blogPost = TestDataFactory.CreateBlogPost();

            // Act
            portfolio.BlogPosts.Add(blogPost);

            // Assert
            portfolio.BlogPosts.Should().HaveCount(1);
            portfolio.BlogPosts.Should().Contain(blogPost);
        }

        [Fact]
        public void Portfolio_ShouldInitializeCollectionNavigationProperties()
        {
            // Arrange & Act
            var portfolio = new Portfolio();

            // Assert
            portfolio.Projects.Should().NotBeNull();
            portfolio.Experience.Should().NotBeNull();
            portfolio.Skills.Should().NotBeNull();
            portfolio.BlogPosts.Should().NotBeNull();
            portfolio.Bookmarks.Should().NotBeNull();
            
            portfolio.Projects.Should().BeEmpty();
            portfolio.Experience.Should().BeEmpty();
            portfolio.Skills.Should().BeEmpty();
            portfolio.BlogPosts.Should().BeEmpty();
            portfolio.Bookmarks.Should().BeEmpty();
        }

        #endregion
    }
} 