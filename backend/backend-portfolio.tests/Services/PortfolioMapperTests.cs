using Xunit;
using FluentAssertions;
using backend_portfolio.Services.Mappers;
using backend_portfolio.Models;
using backend_portfolio.DTO.Portfolio.Request;
using backend_portfolio.DTO.Portfolio.Response;
using backend_portfolio.tests.Helpers;
using System.Collections.Generic;

namespace backend_portfolio.tests.Services
{
    public class PortfolioMapperTests
    {
        private readonly PortfolioMapper _mapper;

        public PortfolioMapperTests()
        {
            _mapper = new PortfolioMapper();
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            // Act
            var mapper = new PortfolioMapper();

            // Assert
            mapper.Should().NotBeNull();
        }

        #endregion

        #region MapToResponseDto Tests

        [Fact]
        public void MapToResponseDto_ShouldMapPortfolioToResponseDto_WhenValidPortfolioProvided()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            portfolio.Id = Guid.NewGuid();
            portfolio.UserId = Guid.NewGuid();
            portfolio.Title = "Test Portfolio";
            portfolio.Bio = "Test Bio";
            portfolio.ViewCount = 100;
            portfolio.LikeCount = 50;
            portfolio.Visibility = backend_portfolio.Models.Visibility.Public;
            portfolio.IsPublished = true;
            portfolio.CreatedAt = DateTime.UtcNow.AddDays(-1);
            portfolio.UpdatedAt = DateTime.UtcNow;
            portfolio.Components = "{\"sections\":[\"about\",\"projects\"]}";

            // Act
            var result = _mapper.MapToResponseDto(portfolio);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(portfolio.Id);
            result.UserId.Should().Be(portfolio.UserId);
            result.Title.Should().Be(portfolio.Title);
            result.Bio.Should().Be(portfolio.Bio);
            result.ViewCount.Should().Be(portfolio.ViewCount);
            result.LikeCount.Should().Be(portfolio.LikeCount);
            result.Visibility.Should().Be(portfolio.Visibility);
            result.IsPublished.Should().Be(portfolio.IsPublished);
            result.CreatedAt.Should().Be(portfolio.CreatedAt);
            result.UpdatedAt.Should().Be(portfolio.UpdatedAt);
            result.Components.Should().Be(portfolio.Components);
        }

        [Fact]
        public void MapToResponseDto_ShouldMapPortfolioToResponseDto_WhenPortfolioHasNullValues()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            portfolio.Bio = null;
            portfolio.Components = null;

            // Act
            var result = _mapper.MapToResponseDto(portfolio);

            // Assert
            result.Should().NotBeNull();
            result.Bio.Should().BeNull();
            result.Components.Should().BeNull();
        }

        [Fact]
        public void MapToResponseDto_ShouldMapPortfolioToResponseDto_WhenPortfolioHasEmptyStrings()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            portfolio.Title = "";
            portfolio.Bio = "";
            portfolio.Components = "";

            // Act
            var result = _mapper.MapToResponseDto(portfolio);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("");
            result.Bio.Should().Be("");
            result.Components.Should().Be("");
        }

        [Fact]
        public void MapToResponseDto_ShouldMapPortfolioToResponseDto_WhenPortfolioHasWhitespaceStrings()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            portfolio.Title = "   ";
            portfolio.Bio = "   ";
            portfolio.Components = "   ";

            // Act
            var result = _mapper.MapToResponseDto(portfolio);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("   ");
            result.Bio.Should().Be("   ");
            result.Components.Should().Be("   ");
        }

        [Fact]
        public void MapToResponseDto_ShouldMapPortfolioToResponseDto_WhenPortfolioHasZeroCounts()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            portfolio.ViewCount = 0;
            portfolio.LikeCount = 0;

            // Act
            var result = _mapper.MapToResponseDto(portfolio);

            // Assert
            result.Should().NotBeNull();
            result.ViewCount.Should().Be(0);
            result.LikeCount.Should().Be(0);
        }

        [Fact]
        public void MapToResponseDto_ShouldMapPortfolioToResponseDto_WhenPortfolioHasNegativeCounts()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            portfolio.ViewCount = -10;
            portfolio.LikeCount = -5;

            // Act
            var result = _mapper.MapToResponseDto(portfolio);

            // Assert
            result.Should().NotBeNull();
            result.ViewCount.Should().Be(-10);
            result.LikeCount.Should().Be(-5);
        }

        [Fact]
        public void MapToResponseDto_ShouldMapPortfolioToResponseDto_WhenPortfolioHasLargeCounts()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            portfolio.ViewCount = int.MaxValue;
            portfolio.LikeCount = int.MaxValue;

            // Act
            var result = _mapper.MapToResponseDto(portfolio);

            // Assert
            result.Should().NotBeNull();
            result.ViewCount.Should().Be(int.MaxValue);
            result.LikeCount.Should().Be(int.MaxValue);
        }

        [Fact]
        public void MapToResponseDto_ShouldMapPortfolioToResponseDto_WhenPortfolioHasDifferentVisibilityValues()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            portfolio.Visibility = backend_portfolio.Models.Visibility.Private;

            // Act
            var result = _mapper.MapToResponseDto(portfolio);

            // Assert
            result.Should().NotBeNull();
            result.Visibility.Should().Be(backend_portfolio.Models.Visibility.Private);
        }

        [Fact]
        public void MapToResponseDto_ShouldMapPortfolioToResponseDto_WhenPortfolioIsNotPublished()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            portfolio.IsPublished = false;

            // Act
            var result = _mapper.MapToResponseDto(portfolio);

            // Assert
            result.Should().NotBeNull();
            result.IsPublished.Should().BeFalse();
        }

        [Fact]
        public void MapToResponseDto_ShouldMapPortfolioToResponseDto_WhenPortfolioHasSpecialCharacters()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            portfolio.Title = "Portfolio with special chars: áéíóú ñ & < > \" '";
            portfolio.Bio = "Bio with emojis: 🚀 💻 📱";
            portfolio.Components = "{\"sections\":[\"about\",\"projects\",\"contact\"]}";

            // Act
            var result = _mapper.MapToResponseDto(portfolio);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("Portfolio with special chars: áéíóú ñ & < > \" '");
            result.Bio.Should().Be("Bio with emojis: 🚀 💻 📱");
            result.Components.Should().Be("{\"sections\":[\"about\",\"projects\",\"contact\"]}");
        }

        #endregion

        #region MapToResponseDtos Tests

        [Fact]
        public void MapToResponseDtos_ShouldMapPortfoliosToResponseDtos_WhenValidPortfoliosProvided()
        {
            // Arrange
            var portfolios = new List<Portfolio>
            {
                TestDataFactory.CreatePortfolio(),
                TestDataFactory.CreatePortfolio(),
                TestDataFactory.CreatePortfolio()
            };

            // Act
            var result = _mapper.MapToResponseDtos(portfolios);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().AllSatisfy(dto => dto.Should().NotBeNull());
        }

        [Fact]
        public void MapToResponseDtos_ShouldReturnEmptyEnumerable_WhenEmptyPortfoliosProvided()
        {
            // Arrange
            var portfolios = new List<Portfolio>();

            // Act
            var result = _mapper.MapToResponseDtos(portfolios);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void MapToResponseDtos_ShouldReturnEmptyEnumerable_WhenNullPortfoliosProvided()
        {
            // Arrange
            IEnumerable<Portfolio> portfolios = null!;

            // Act
            var result = _mapper.MapToResponseDtos(portfolios);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void MapToResponseDtos_ShouldHandlePortfoliosWithNullValues()
        {
            // Arrange
            var portfolios = new List<Portfolio>
            {
                TestDataFactory.CreatePortfolio(),
                null!,
                TestDataFactory.CreatePortfolio()
            };

            // Act
            var result = _mapper.MapToResponseDtos(portfolios);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2); // Only 2 valid portfolios after filtering out null
            result.Should().AllSatisfy(dto => dto.Should().NotBeNull());
        }

        #endregion

        #region MapFromCreateDto Tests

        [Fact]
        public void MapFromCreateDto_ShouldMapCreateRequestToPortfolio_WhenValidRequestProvided()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "New Portfolio",
                Bio = "New Bio",
                Visibility = backend_portfolio.Models.Visibility.Public,
                IsPublished = true,
                Components = "{\"sections\":[\"about\"]}"
            };

            // Act
            var result = _mapper.MapFromCreateDto(request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBe(Guid.Empty);
            result.UserId.Should().Be(request.UserId);
            result.Title.Should().Be(request.Title);
            result.Bio.Should().Be(request.Bio);
            result.Visibility.Should().Be(request.Visibility);
            result.IsPublished.Should().Be(request.IsPublished);
            result.Components.Should().Be(request.Components);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void MapFromCreateDto_ShouldMapCreateRequestToPortfolio_WhenRequestHasNullValues()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "New Portfolio",
                Bio = null,
                Components = null
            };

            // Act
            var result = _mapper.MapFromCreateDto(request);

            // Assert
            result.Should().NotBeNull();
            result.Bio.Should().BeNull();
            result.Components.Should().BeNull();
        }

        [Fact]
        public void MapFromCreateDto_ShouldMapCreateRequestToPortfolio_WhenRequestHasEmptyStrings()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "",
                Bio = "",
                Components = ""
            };

            // Act
            var result = _mapper.MapFromCreateDto(request);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("");
            result.Bio.Should().Be("");
            result.Components.Should().Be("");
        }

        [Fact]
        public void MapFromCreateDto_ShouldMapCreateRequestToPortfolio_WhenRequestHasWhitespaceStrings()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "   ",
                Bio = "   ",
                Components = "   "
            };

            // Act
            var result = _mapper.MapFromCreateDto(request);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("   ");
            result.Bio.Should().Be("   ");
            result.Components.Should().Be("   ");
        }

        [Fact]
        public void MapFromCreateDto_ShouldMapCreateRequestToPortfolio_WhenRequestHasEmptyUserId()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.Empty,
                Title = "New Portfolio"
            };

            // Act
            var result = _mapper.MapFromCreateDto(request);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(Guid.Empty);
        }

        [Fact]
        public void MapFromCreateDto_ShouldMapCreateRequestToPortfolio_WhenRequestHasSpecialCharacters()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "Portfolio with special chars: áéíóú ñ & < > \" '",
                Bio = "Bio with emojis: 🚀 💻 📱",
                Components = "{\"sections\":[\"about\",\"projects\",\"contact\"]}"
            };

            // Act
            var result = _mapper.MapFromCreateDto(request);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("Portfolio with special chars: áéíóú ñ & < > \" '");
            result.Bio.Should().Be("Bio with emojis: 🚀 💻 📱");
            result.Components.Should().Be("{\"sections\":[\"about\",\"projects\",\"contact\"]}");
        }

        #endregion

        #region UpdateEntityFromDto Tests

        [Fact]
        public void UpdateEntityFromDto_ShouldUpdatePortfolioFromUpdateRequest_WhenValidRequestProvided()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            var originalTitle = portfolio.Title;
            var originalBio = portfolio.Bio;
            var originalVisibility = portfolio.Visibility;
            var originalIsPublished = portfolio.IsPublished;
            var originalComponents = portfolio.Components;
            var originalUpdatedAt = portfolio.UpdatedAt;

            var request = new PortfolioUpdateRequest
            {
                Title = "Updated Title",
                Bio = "Updated Bio",
                Visibility = backend_portfolio.Models.Visibility.Private,
                IsPublished = false,
                Components = "{\"sections\":[\"about\",\"projects\"]}"
            };

            // Act
            _mapper.UpdateEntityFromDto(portfolio, request);

            // Assert
            portfolio.Title.Should().Be("Updated Title");
            portfolio.Bio.Should().Be("Updated Bio");
            portfolio.Visibility.Should().Be(backend_portfolio.Models.Visibility.Private);
            portfolio.IsPublished.Should().BeFalse();
            portfolio.Components.Should().Be("{\"sections\":[\"about\",\"projects\"]}");
            portfolio.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void UpdateEntityFromDto_ShouldUpdatePortfolioFromUpdateRequest_WhenRequestHasNullValues()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            var originalTitle = portfolio.Title;
            var originalBio = portfolio.Bio;
            var originalVisibility = portfolio.Visibility;
            var originalIsPublished = portfolio.IsPublished;
            var originalComponents = portfolio.Components;

            var request = new PortfolioUpdateRequest
            {
                Title = null,
                Bio = null,
                Visibility = null,
                IsPublished = null,
                Components = null
            };

            // Act
            _mapper.UpdateEntityFromDto(portfolio, request);

            // Assert
            portfolio.Title.Should().Be(originalTitle);
            portfolio.Bio.Should().Be(originalBio);
            portfolio.Visibility.Should().Be(originalVisibility);
            portfolio.IsPublished.Should().Be(originalIsPublished);
            portfolio.Components.Should().Be(originalComponents);
            portfolio.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void UpdateEntityFromDto_ShouldUpdatePortfolioFromUpdateRequest_WhenRequestHasPartialUpdates()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            var originalTitle = portfolio.Title;
            var originalBio = portfolio.Bio;
            var originalVisibility = portfolio.Visibility;
            var originalIsPublished = portfolio.IsPublished;
            var originalComponents = portfolio.Components;

            var request = new PortfolioUpdateRequest
            {
                Title = "Updated Title"
                // Only updating title, other fields should remain unchanged
            };

            // Act
            _mapper.UpdateEntityFromDto(portfolio, request);

            // Assert
            portfolio.Title.Should().Be("Updated Title");
            portfolio.Bio.Should().Be(originalBio);
            portfolio.Visibility.Should().Be(originalVisibility);
            portfolio.IsPublished.Should().Be(originalIsPublished);
            portfolio.Components.Should().Be(originalComponents);
            portfolio.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void UpdateEntityFromDto_ShouldUpdatePortfolioFromUpdateRequest_WhenRequestHasEmptyStrings()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            var originalTitle = portfolio.Title;
            var originalBio = portfolio.Bio;
            var originalComponents = portfolio.Components;

            var request = new PortfolioUpdateRequest
            {
                Title = "",
                Bio = "",
                Components = ""
            };

            // Act
            _mapper.UpdateEntityFromDto(portfolio, request);

            // Assert
            portfolio.Title.Should().Be("");
            portfolio.Bio.Should().Be("");
            portfolio.Components.Should().Be("");
            portfolio.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void UpdateEntityFromDto_ShouldUpdatePortfolioFromUpdateRequest_WhenRequestHasWhitespaceStrings()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            var originalTitle = portfolio.Title;
            var originalBio = portfolio.Bio;
            var originalComponents = portfolio.Components;

            var request = new PortfolioUpdateRequest
            {
                Title = "   ",
                Bio = "   ",
                Components = "   "
            };

            // Act
            _mapper.UpdateEntityFromDto(portfolio, request);

            // Assert
            portfolio.Title.Should().Be("   ");
            portfolio.Bio.Should().Be("   ");
            portfolio.Components.Should().Be("   ");
            portfolio.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void UpdateEntityFromDto_ShouldUpdatePortfolioFromUpdateRequest_WhenRequestHasSpecialCharacters()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();

            var request = new PortfolioUpdateRequest
            {
                Title = "Updated with special chars: áéíóú ñ & < > \" '",
                Bio = "Updated bio with emojis: 🚀 💻 📱",
                Components = "{\"sections\":[\"about\",\"projects\",\"contact\"]}"
            };

            // Act
            _mapper.UpdateEntityFromDto(portfolio, request);

            // Assert
            portfolio.Title.Should().Be("Updated with special chars: áéíóú ñ & < > \" '");
            portfolio.Bio.Should().Be("Updated bio with emojis: 🚀 💻 📱");
            portfolio.Components.Should().Be("{\"sections\":[\"about\",\"projects\",\"contact\"]}");
            portfolio.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        #endregion

        #region MapToSummaryDto Tests

        [Fact]
        public void MapToSummaryDto_ShouldMapPortfolioToSummaryDto_WhenValidPortfolioProvided()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            portfolio.Id = Guid.NewGuid();
            portfolio.UserId = Guid.NewGuid();
            portfolio.Title = "Test Portfolio";
            portfolio.Bio = "Test Bio";
            portfolio.ViewCount = 100;
            portfolio.LikeCount = 50;
            portfolio.Visibility = backend_portfolio.Models.Visibility.Public;
            portfolio.IsPublished = true;
            portfolio.CreatedAt = DateTime.UtcNow.AddDays(-1);
            portfolio.UpdatedAt = DateTime.UtcNow;
            portfolio.Components = "{\"sections\":[\"about\",\"projects\"]}";

            // Act
            var result = _mapper.MapToSummaryDto(portfolio);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(portfolio.Id);
            result.UserId.Should().Be(portfolio.UserId);
            result.Title.Should().Be(portfolio.Title);
            result.Bio.Should().Be(portfolio.Bio);
            result.ViewCount.Should().Be(portfolio.ViewCount);
            result.LikeCount.Should().Be(portfolio.LikeCount);
            result.Visibility.Should().Be(portfolio.Visibility);
            result.IsPublished.Should().Be(portfolio.IsPublished);
            result.CreatedAt.Should().Be(portfolio.CreatedAt);
            result.UpdatedAt.Should().Be(portfolio.UpdatedAt);
            result.Components.Should().Be(portfolio.Components);
            result.Template.Should().BeNull(); // No template assigned
        }

        [Fact]
        public void MapToSummaryDto_ShouldMapPortfolioToSummaryDto_WhenPortfolioHasTemplate()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            var template = TestDataFactory.CreatePortfolioTemplate();
            portfolio.Template = template;

            // Act
            var result = _mapper.MapToSummaryDto(portfolio);

            // Assert
            result.Should().NotBeNull();
            result.Template.Should().NotBeNull();
            result.Template!.Id.Should().Be(template.Id);
            result.Template.Name.Should().Be(template.Name);
            result.Template.Description.Should().Be(template.Description);
            result.Template.PreviewImageUrl.Should().Be(template.PreviewImageUrl);
            result.Template.IsActive.Should().Be(template.IsActive);
        }

        [Fact]
        public void MapToSummaryDto_ShouldMapPortfolioToSummaryDto_WhenPortfolioHasNullValues()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            portfolio.Bio = null;
            portfolio.Components = null;
            portfolio.Template = null;

            // Act
            var result = _mapper.MapToSummaryDto(portfolio);

            // Assert
            result.Should().NotBeNull();
            result.Bio.Should().BeNull();
            result.Components.Should().BeNull();
            result.Template.Should().BeNull();
        }

        #endregion

        #region MapToSummaryDtos Tests

        [Fact]
        public void MapToSummaryDtos_ShouldMapPortfoliosToSummaryDtos_WhenValidPortfoliosProvided()
        {
            // Arrange
            var portfolios = new List<Portfolio>
            {
                TestDataFactory.CreatePortfolio(),
                TestDataFactory.CreatePortfolio(),
                TestDataFactory.CreatePortfolio()
            };

            // Act
            var result = _mapper.MapToSummaryDtos(portfolios);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().AllSatisfy(dto => dto.Should().NotBeNull());
        }

        [Fact]
        public void MapToSummaryDtos_ShouldReturnEmptyEnumerable_WhenEmptyPortfoliosProvided()
        {
            // Arrange
            var portfolios = new List<Portfolio>();

            // Act
            var result = _mapper.MapToSummaryDtos(portfolios);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void MapToSummaryDtos_ShouldReturnEmptyEnumerable_WhenNullPortfoliosProvided()
        {
            // Arrange
            IEnumerable<Portfolio> portfolios = null!;

            // Act
            var result = _mapper.MapToSummaryDtos(portfolios);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void MapToSummaryDtos_ShouldHandlePortfoliosWithNullValues()
        {
            // Arrange
            var portfolios = new List<Portfolio>
            {
                TestDataFactory.CreatePortfolio(),
                null!,
                TestDataFactory.CreatePortfolio()
            };

            // Act
            var result = _mapper.MapToSummaryDtos(portfolios);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2); // Only 2 valid portfolios after filtering out null
            result.Should().AllSatisfy(dto => dto.Should().NotBeNull());
        }

        #endregion

        #region MapToDetailDto Tests

        [Fact]
        public void MapToDetailDto_ShouldMapPortfolioToDetailDto_WhenValidPortfolioProvided()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            portfolio.Id = Guid.NewGuid();
            portfolio.UserId = Guid.NewGuid();
            portfolio.Title = "Test Portfolio";
            portfolio.Bio = "Test Bio";
            portfolio.ViewCount = 100;
            portfolio.LikeCount = 50;
            portfolio.Visibility = backend_portfolio.Models.Visibility.Public;
            portfolio.IsPublished = true;
            portfolio.CreatedAt = DateTime.UtcNow.AddDays(-1);
            portfolio.UpdatedAt = DateTime.UtcNow;
            portfolio.Components = "{\"sections\":[\"about\",\"projects\"]}";

            // Act
            var result = _mapper.MapToDetailDto(portfolio);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(portfolio.Id);
            result.UserId.Should().Be(portfolio.UserId);
            result.Title.Should().Be(portfolio.Title);
            result.Bio.Should().Be(portfolio.Bio);
            result.ViewCount.Should().Be(portfolio.ViewCount);
            result.LikeCount.Should().Be(portfolio.LikeCount);
            result.Visibility.Should().Be(portfolio.Visibility);
            result.IsPublished.Should().Be(portfolio.IsPublished);
            result.CreatedAt.Should().Be(portfolio.CreatedAt);
            result.UpdatedAt.Should().Be(portfolio.UpdatedAt);
            result.Components.Should().Be(portfolio.Components);
        }

        [Fact]
        public void MapToDetailDto_ShouldMapPortfolioToDetailDto_WhenPortfolioHasNullValues()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            portfolio.Bio = null;
            portfolio.Components = null;

            // Act
            var result = _mapper.MapToDetailDto(portfolio);

            // Assert
            result.Should().NotBeNull();
            result.Bio.Should().BeNull();
            result.Components.Should().BeNull();
        }

        #endregion

        #region Edge Cases and Performance Tests

        [Fact]
        public void MapToResponseDto_ShouldHandleLargePortfolioData()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            portfolio.Title = new string('A', 1000);
            portfolio.Bio = new string('B', 5000);
            portfolio.Components = new string('C', 10000);

            // Act
            var result = _mapper.MapToResponseDto(portfolio);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().HaveLength(1000);
            result.Bio.Should().HaveLength(5000);
            result.Components.Should().HaveLength(10000);
        }

        [Fact]
        public void MapToResponseDtos_ShouldHandleLargeCollection()
        {
            // Arrange
            var portfolios = Enumerable.Range(0, 1000)
                .Select(_ => TestDataFactory.CreatePortfolio())
                .ToList();

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = _mapper.MapToResponseDtos(portfolios);
            stopwatch.Stop();

            // Assert
            result.Should().HaveCount(1000);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should complete within 1 second
        }

        [Fact]
        public void MapFromCreateDto_ShouldGenerateUniqueIds()
        {
            // Arrange
            var request = new PortfolioCreateRequest
            {
                UserId = Guid.NewGuid(),
                Title = "Test Portfolio"
            };

            // Act
            var result1 = _mapper.MapFromCreateDto(request);
            var result2 = _mapper.MapFromCreateDto(request);

            // Assert
            result1.Id.Should().NotBe(result2.Id);
        }

        [Fact]
        public void UpdateEntityFromDto_ShouldNotModifyId()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            var originalId = portfolio.Id;
            var request = new PortfolioUpdateRequest
            {
                Title = "Updated Title"
            };

            // Act
            _mapper.UpdateEntityFromDto(portfolio, request);

            // Assert
            portfolio.Id.Should().Be(originalId);
        }

        [Fact]
        public void UpdateEntityFromDto_ShouldNotModifyUserId()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            var originalUserId = portfolio.UserId;
            var request = new PortfolioUpdateRequest
            {
                Title = "Updated Title"
            };

            // Act
            _mapper.UpdateEntityFromDto(portfolio, request);

            // Assert
            portfolio.UserId.Should().Be(originalUserId);
        }

        [Fact]
        public void UpdateEntityFromDto_ShouldNotModifyCreatedAt()
        {
            // Arrange
            var portfolio = TestDataFactory.CreatePortfolio();
            var originalCreatedAt = portfolio.CreatedAt;
            var request = new PortfolioUpdateRequest
            {
                Title = "Updated Title"
            };

            // Act
            _mapper.UpdateEntityFromDto(portfolio, request);

            // Assert
            portfolio.CreatedAt.Should().Be(originalCreatedAt);
        }

        #endregion
    }
} 