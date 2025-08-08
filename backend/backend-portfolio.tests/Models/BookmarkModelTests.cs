using Xunit;
using FluentAssertions;
using backend_portfolio.Models;
using System.ComponentModel.DataAnnotations;

namespace backend_portfolio.tests.Models
{
    public class BookmarkModelTests
    {
        [Fact]
        public void Bookmark_ShouldCreateInstance_WithDefaultValues()
        {
            // Arrange & Act
            var bookmark = new Bookmark();

            // Assert
            bookmark.Should().NotBeNull();
            bookmark.Id.Should().NotBe(Guid.Empty);
            bookmark.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void Bookmark_ShouldSetProperties_WhenValidDataProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var collectionName = "Favorites";
            var notes = "This is a great portfolio";

            // Act
            var bookmark = new Bookmark
            {
                UserId = userId,
                PortfolioId = portfolioId,
                CollectionName = collectionName,
                Notes = notes
            };

            // Assert
            bookmark.UserId.Should().Be(userId);
            bookmark.PortfolioId.Should().Be(portfolioId);
            bookmark.CollectionName.Should().Be(collectionName);
            bookmark.Notes.Should().Be(notes);
        }

        [Fact]
        public void Bookmark_ShouldFailValidation_WhenUserIdIsEmpty()
        {
            // Arrange
            var bookmark = new Bookmark
            {
                UserId = Guid.Empty,
                PortfolioId = Guid.NewGuid()
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(bookmark);
            var isValid = Validator.TryValidateObject(bookmark, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("UserId"));
        }

        [Fact]
        public void Bookmark_ShouldFailValidation_WhenPortfolioIdIsEmpty()
        {
            // Arrange
            var bookmark = new Bookmark
            {
                UserId = Guid.NewGuid(),
                PortfolioId = Guid.Empty
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(bookmark);
            var isValid = Validator.TryValidateObject(bookmark, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("PortfolioId"));
        }

        [Fact]
        public void Bookmark_ShouldFailValidation_WhenCollectionNameExceedsMaxLength()
        {
            // Arrange
            var longCollectionName = new string('a', 101); // Exceeds 100 character limit
            var bookmark = new Bookmark
            {
                UserId = Guid.NewGuid(),
                PortfolioId = Guid.NewGuid(),
                CollectionName = longCollectionName
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(bookmark);
            var isValid = Validator.TryValidateObject(bookmark, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("CollectionName"));
        }

        [Fact]
        public void Bookmark_ShouldPassValidation_WhenRequiredFieldsAreProvided()
        {
            // Arrange
            var bookmark = new Bookmark
            {
                UserId = Guid.NewGuid(),
                PortfolioId = Guid.NewGuid()
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(bookmark);
            var isValid = Validator.TryValidateObject(bookmark, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void Bookmark_ShouldAllowNullOptionalFields()
        {
            // Arrange & Act
            var bookmark = new Bookmark
            {
                UserId = Guid.NewGuid(),
                PortfolioId = Guid.NewGuid(),
                CollectionName = null,
                Notes = null
            };

            // Assert
            bookmark.CollectionName.Should().BeNull();
            bookmark.Notes.Should().BeNull();
        }

        [Fact]
        public void Bookmark_ShouldInitializeWithNullNavigationProperty()
        {
            // Arrange & Act
            var bookmark = new Bookmark();

            // Assert
            // Reference navigation properties should be null until loaded by EF
            bookmark.Portfolio.Should().BeNull();
        }
    }
} 