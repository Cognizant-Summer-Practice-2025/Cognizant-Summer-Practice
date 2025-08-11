using Xunit;
using FluentAssertions;
using backend_portfolio.Models;
using System.ComponentModel.DataAnnotations;

namespace backend_portfolio.tests.Models
{
    public class BlogPostModelTests
    {
        [Fact]
        public void BlogPost_ShouldCreateInstance_WithDefaultValues()
        {
            // Arrange & Act
            var blogPost = new BlogPost();

            // Assert
            blogPost.Should().NotBeNull();
            blogPost.Id.Should().NotBe(Guid.Empty);
            blogPost.Title.Should().Be(string.Empty);
            blogPost.IsPublished.Should().BeFalse();
            blogPost.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            blogPost.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void BlogPost_ShouldSetProperties_WhenValidDataProvided()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var title = "Test Blog Post";
            var excerpt = "This is a test excerpt";
            var content = "This is the full content of the blog post";
            var featuredImageUrl = "https://example.com/image.jpg";
            var tags = new[] { "tech", "programming", "testing" };
            var publishedAt = DateTime.UtcNow.AddDays(-1);

            // Act
            var blogPost = new BlogPost
            {
                PortfolioId = portfolioId,
                Title = title,
                Excerpt = excerpt,
                Content = content,
                FeaturedImageUrl = featuredImageUrl,
                Tags = tags,
                IsPublished = true,
                PublishedAt = publishedAt
            };

            // Assert
            blogPost.PortfolioId.Should().Be(portfolioId);
            blogPost.Title.Should().Be(title);
            blogPost.Excerpt.Should().Be(excerpt);
            blogPost.Content.Should().Be(content);
            blogPost.FeaturedImageUrl.Should().Be(featuredImageUrl);
            blogPost.Tags.Should().BeEquivalentTo(tags);
            blogPost.IsPublished.Should().BeTrue();
            blogPost.PublishedAt.Should().Be(publishedAt);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void BlogPost_ShouldHandleInvalidTitle_WhenValidating(string invalidTitle)
        {
            // Arrange
            var blogPost = new BlogPost
            {
                PortfolioId = Guid.NewGuid(),
                Title = invalidTitle
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(blogPost);
            var isValid = Validator.TryValidateObject(blogPost, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("Title"));
        }

        [Fact]
        public void BlogPost_ShouldFailValidation_WhenTitleExceedsMaxLength()
        {
            // Arrange
            var longTitle = new string('a', 256); // Exceeds 255 character limit
            var blogPost = new BlogPost
            {
                PortfolioId = Guid.NewGuid(),
                Title = longTitle
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(blogPost);
            var isValid = Validator.TryValidateObject(blogPost, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("Title"));
        }

        [Fact]
        public void BlogPost_ShouldPassValidation_WhenRequiredFieldsAreProvided()
        {
            // Arrange
            var blogPost = new BlogPost
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Valid Blog Post Title"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(blogPost);
            var isValid = Validator.TryValidateObject(blogPost, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void BlogPost_ShouldAllowNullOptionalFields()
        {
            // Arrange & Act
            var blogPost = new BlogPost
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Test Title",
                Excerpt = null,
                Content = null,
                FeaturedImageUrl = null,
                Tags = null,
                PublishedAt = null
            };

            // Assert
            blogPost.Excerpt.Should().BeNull();
            blogPost.Content.Should().BeNull();
            blogPost.FeaturedImageUrl.Should().BeNull();
            blogPost.Tags.Should().BeNull();
            blogPost.PublishedAt.Should().BeNull();
        }

        [Fact]
        public void BlogPost_ShouldInitializeWithNullNavigationProperty()
        {
            // Arrange & Act
            var blogPost = new BlogPost();

            // Assert
            // Reference navigation properties should be null until loaded by EF
            blogPost.Portfolio.Should().BeNull();
        }
    }
} 