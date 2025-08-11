using Xunit;
using FluentAssertions;
using backend_user.Models;
using System;

namespace backend_user.tests.Models
{
    public class BookmarkModelTests
    {
        [Fact]
        public void Bookmark_DefaultConstructor_ShouldInitializeProperties()
        {
            var bookmark = new Bookmark();
            bookmark.Id.Should().NotBe(Guid.Empty);
            bookmark.UserId.Should().Be(Guid.Empty);
            bookmark.PortfolioId.Should().Be("");
            bookmark.PortfolioTitle.Should().BeNull();
            bookmark.PortfolioOwnerName.Should().BeNull();
            bookmark.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            bookmark.User.Should().BeNull(); // navigation property default
        }

        [Fact]
        public void Bookmark_PropertySetters_ShouldWorkCorrectly()
        {
            var bookmark = new Bookmark();
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var portfolioId = "portfolio-123";
            var portfolioTitle = "Test Portfolio";
            var portfolioOwnerName = "Owner Name";
            var createdAt = DateTime.UtcNow.AddDays(-1);

            bookmark.Id = id;
            bookmark.UserId = userId;
            bookmark.PortfolioId = portfolioId;
            bookmark.PortfolioTitle = portfolioTitle;
            bookmark.PortfolioOwnerName = portfolioOwnerName;
            bookmark.CreatedAt = createdAt;

            bookmark.Id.Should().Be(id);
            bookmark.UserId.Should().Be(userId);
            bookmark.PortfolioId.Should().Be(portfolioId);
            bookmark.PortfolioTitle.Should().Be(portfolioTitle);
            bookmark.PortfolioOwnerName.Should().Be(portfolioOwnerName);
            bookmark.CreatedAt.Should().Be(createdAt);
        }

        [Fact]
        public void Bookmark_NavigationProperty_ShouldAllowUserAssignment()
        {
            var user = new User { Id = Guid.NewGuid(), Email = "test@example.com" };
            var bookmark = new Bookmark { UserId = user.Id };
            bookmark.User = user;
            bookmark.User.Should().Be(user);
            bookmark.UserId.Should().Be(user.Id);
        }

        [Fact]
        public void Bookmark_EmptyPortfolioId_ShouldBeAllowed()
        {
            var bookmark = new Bookmark { PortfolioId = "" };
            bookmark.PortfolioId.Should().Be("");
        }
    }
}