using Xunit;
using FluentAssertions;
using backend_portfolio.DTO.Portfolio.Request;
using backend_portfolio.DTO.Portfolio.Response;
using backend_portfolio.Models;

namespace backend_portfolio.tests.DTO
{
    public class PortfolioDTOTests
    {
        #region PortfolioCreateRequest Tests

        [Fact]
        public void PortfolioCreateRequest_ShouldBeInstantiable()
        {
            // Act
            var dto = new PortfolioCreateRequest();

            // Assert
            dto.Should().NotBeNull();
            dto.Title.Should().NotBeNull();
            dto.TemplateName.Should().NotBeNull();
            dto.Visibility.Should().Be(Visibility.Public);
            dto.IsPublished.Should().BeFalse();
        }

        [Fact]
        public void PortfolioCreateRequest_ShouldAcceptValidValues()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var title = "Test Portfolio";
            var bio = "Test Bio";

            // Act
            var dto = new PortfolioCreateRequest
            {
                UserId = userId,
                Title = title,
                Bio = bio,
                Visibility = Visibility.Private,
                IsPublished = true
            };

            // Assert
            dto.UserId.Should().Be(userId);
            dto.Title.Should().Be(title);
            dto.Bio.Should().Be(bio);
            dto.Visibility.Should().Be(Visibility.Private);
            dto.IsPublished.Should().BeTrue();
        }

        #endregion

        #region PortfolioUpdateRequest Tests

        [Fact]
        public void PortfolioUpdateRequest_ShouldBeInstantiable()
        {
            // Act
            var dto = new PortfolioUpdateRequest();

            // Assert
            dto.Should().NotBeNull();
        }

        [Fact]
        public void PortfolioUpdateRequest_ShouldAcceptValidValues()
        {
            // Arrange
            var title = "Updated Portfolio";
            var bio = "Updated Bio";

            // Act
            var dto = new PortfolioUpdateRequest
            {
                Title = title,
                Bio = bio,
                Visibility = Visibility.Public,
                IsPublished = false
            };

            // Assert
            dto.Title.Should().Be(title);
            dto.Bio.Should().Be(bio);
            dto.Visibility.Should().Be(Visibility.Public);
            dto.IsPublished.Should().BeFalse();
        }

        #endregion

        #region PortfolioResponse Tests

        [Fact]
        public void PortfolioResponse_ShouldBeInstantiable()
        {
            // Act
            var dto = new PortfolioResponse();

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(Guid.Empty);
            dto.UserId.Should().Be(Guid.Empty);
            dto.Title.Should().NotBeNull();
        }

        [Fact]
        public void PortfolioResponse_ShouldAcceptValidValues()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var title = "Test Portfolio";
            var bio = "Test Bio";
            var visibility = Visibility.Public;
            var isPublished = true;
            var createdAt = DateTime.UtcNow;
            var updatedAt = DateTime.UtcNow;

            // Act
            var dto = new PortfolioResponse
            {
                Id = id,
                UserId = userId,
                Title = title,
                Bio = bio,
                Visibility = visibility,
                IsPublished = isPublished,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };

            // Assert
            dto.Id.Should().Be(id);
            dto.UserId.Should().Be(userId);
            dto.Title.Should().Be(title);
            dto.Bio.Should().Be(bio);
            dto.Visibility.Should().Be(visibility);
            dto.IsPublished.Should().Be(isPublished);
            dto.CreatedAt.Should().Be(createdAt);
            dto.UpdatedAt.Should().Be(updatedAt);
        }

        #endregion
    }
} 