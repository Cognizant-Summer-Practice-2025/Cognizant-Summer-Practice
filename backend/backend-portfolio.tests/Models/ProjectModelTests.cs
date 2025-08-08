using Xunit;
using FluentAssertions;
using backend_portfolio.Models;
using backend_portfolio.tests.Helpers;

namespace backend_portfolio.tests.Models
{
    public class ProjectModelTests
    {
        #region Constructor Tests

        [Fact]
        public void Project_ShouldBeInstantiable()
        {
            // Act
            var project = new Project();

            // Assert
            project.Should().NotBeNull();
            project.Id.Should().Be(Guid.Empty);
            project.PortfolioId.Should().Be(Guid.Empty);
            project.Title.Should().NotBeNull(); // Has default value
            project.Description.Should().BeNull();
        }

        #endregion

        #region Property Tests

        [Fact]
        public void Project_ShouldHaveValidProperties()
        {
            // Arrange
            var project = TestDataFactory.CreateProject();

            // Assert
            project.Id.Should().NotBe(Guid.Empty);
            project.PortfolioId.Should().NotBe(Guid.Empty);
            project.Title.Should().NotBeNullOrWhiteSpace();
            project.Description.Should().NotBeNullOrWhiteSpace();
            project.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            project.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Project_Title_ShouldHandleInvalidValues(string? invalidTitle)
        {
            // Arrange
            var project = new Project();

            // Act
            project.Title = invalidTitle!;

            // Assert
            project.Title.Should().Be(invalidTitle);
        }

        [Fact]
        public void Project_PortfolioId_ShouldAcceptValidValues()
        {
            // Arrange
            var project = new Project();
            var validPortfolioId = Guid.NewGuid();

            // Act
            project.PortfolioId = validPortfolioId;

            // Assert
            project.PortfolioId.Should().Be(validPortfolioId);
        }

        [Fact]
        public void Project_ShouldAllowNullableProperties()
        {
            // Arrange
            var project = new Project();

            // Act
            project.DemoUrl = null;
            project.GithubUrl = null;
            project.ImageUrl = null;

            // Assert
            project.DemoUrl.Should().BeNull();
            project.GithubUrl.Should().BeNull();
            project.ImageUrl.Should().BeNull();
        }

        [Fact]
        public void Project_ShouldAllowValidUrls()
        {
            // Arrange
            var project = new Project();
            var validUrl = "https://example.com";

            // Act
            project.DemoUrl = validUrl;
            project.GithubUrl = validUrl;
            project.ImageUrl = validUrl;

            // Assert
            project.DemoUrl.Should().Be(validUrl);
            project.GithubUrl.Should().Be(validUrl);
            project.ImageUrl.Should().Be(validUrl);
        }

        #endregion

        #region DateTime Tests

        [Fact]
        public void Project_CreatedAt_ShouldBeSetCorrectly()
        {
            // Arrange
            var expectedDate = DateTime.UtcNow;
            var project = new Project();

            // Act
            project.CreatedAt = expectedDate;

            // Assert
            project.CreatedAt.Should().Be(expectedDate);
        }

        [Fact]
        public void Project_UpdatedAt_ShouldBeSetCorrectly()
        {
            // Arrange
            var expectedDate = DateTime.UtcNow;
            var project = new Project();

            // Act
            project.UpdatedAt = expectedDate;

            // Assert
            project.UpdatedAt.Should().Be(expectedDate);
        }

        #endregion

        #region Relationship Tests

        [Fact]
        public void Project_ShouldHavePortfolioReference()
        {
            // Arrange
            var project = new Project();
            var portfolio = TestDataFactory.CreatePortfolio();

            // Act
            project.Portfolio = portfolio;
            project.PortfolioId = portfolio.Id;

            // Assert
            project.Portfolio.Should().Be(portfolio);
            project.PortfolioId.Should().Be(portfolio.Id);
        }

        #endregion
    }
} 