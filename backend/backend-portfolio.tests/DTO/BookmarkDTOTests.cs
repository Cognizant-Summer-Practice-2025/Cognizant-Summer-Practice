using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using backend_portfolio.DTO.Bookmark.Request;
using backend_portfolio.DTO.Bookmark.Response;
using backend_portfolio.DTO.Portfolio.Response;
using AutoFixture;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend_portfolio.tests.DTO
{
    public class BookmarkDTOTests
    {
        private readonly Fixture _fixture;

        public BookmarkDTOTests()
        {
            _fixture = new Fixture();
        }

        #region BookmarkCreateRequest Tests

        [Fact]
        public void BookmarkCreateRequest_ShouldBeInstantiable()
        {
            // Act
            var dto = new BookmarkCreateRequest();

            // Assert
            dto.Should().NotBeNull();
            dto.UserId.Should().Be(Guid.Empty);
            dto.PortfolioId.Should().Be(Guid.Empty);
        }

        [Fact]
        public void BookmarkCreateRequest_ShouldAcceptValidValues()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var collectionName = "My Favorites";
            var notes = "This portfolio has great design inspiration";

            // Act
            var dto = new BookmarkCreateRequest
            {
                UserId = userId,
                PortfolioId = portfolioId,
                CollectionName = collectionName,
                Notes = notes
            };

            // Assert
            dto.UserId.Should().Be(userId);
            dto.PortfolioId.Should().Be(portfolioId);
            dto.CollectionName.Should().Be(collectionName);
            dto.Notes.Should().Be(notes);
        }

        [Fact]
        public void BookmarkCreateRequest_ShouldHandleNullOptionalFields()
        {
            // Act
            var dto = new BookmarkCreateRequest
            {
                CollectionName = null,
                Notes = null
            };

            // Assert
            dto.CollectionName.Should().BeNull();
            dto.Notes.Should().BeNull();
        }

        [Fact]
        public void BookmarkCreateRequest_ShouldHandleEmptyStrings()
        {
            // Act
            var dto = new BookmarkCreateRequest
            {
                CollectionName = "",
                Notes = ""
            };

            // Assert
            dto.CollectionName.Should().Be("");
            dto.Notes.Should().Be("");
        }

        [Theory]
        [InlineData("Favorites")]
        [InlineData("Design Inspiration")]
        [InlineData("Web Development")]
        [InlineData("UI/UX Examples")]
        [InlineData("Career Goals")]
        [InlineData("Learning Resources")]
        public void BookmarkCreateRequest_ShouldHandleCommonCollectionNames(string collectionName)
        {
            // Act
            var dto = new BookmarkCreateRequest { CollectionName = collectionName };

            // Assert
            dto.CollectionName.Should().Be(collectionName);
        }

        [Fact]
        public void BookmarkCreateRequest_ShouldHandleSpecialCharactersInCollectionName()
        {
            // Arrange
            var specialCollectionName = "My Collection #1: Best of 2024 (Top Picks!)";

            // Act
            var dto = new BookmarkCreateRequest { CollectionName = specialCollectionName };

            // Assert
            dto.CollectionName.Should().Be(specialCollectionName);
        }

        [Fact]
        public void BookmarkCreateRequest_ShouldHandleUnicodeCharacters()
        {
            // Arrange
            var unicodeCollectionName = "我的收藏 🌟 お気に入り";
            var unicodeNotes = "这个作品集很棒 この作品集は素晴らしい";

            // Act
            var dto = new BookmarkCreateRequest
            {
                CollectionName = unicodeCollectionName,
                Notes = unicodeNotes
            };

            // Assert
            dto.CollectionName.Should().Be(unicodeCollectionName);
            dto.Notes.Should().Be(unicodeNotes);
        }

        [Fact]
        public void BookmarkCreateRequest_ShouldHandleLongStrings()
        {
            // Arrange
            var longCollectionName = new string('a', 5000);
            var longNotes = new string('b', 50000);

            // Act
            var dto = new BookmarkCreateRequest
            {
                CollectionName = longCollectionName,
                Notes = longNotes
            };

            // Assert
            dto.CollectionName.Should().Be(longCollectionName);
            dto.Notes.Should().Be(longNotes);
        }

        [Fact]
        public void BookmarkCreateRequest_ShouldHandleDetailedNotes()
        {
            // Arrange
            var detailedNotes = @"
Portfolio Analysis:
- Excellent use of typography and color scheme
- Responsive design works well on mobile
- Clean navigation structure
- Impressive project showcase
- Skills section is well organized
- Contact form is user-friendly

Areas of inspiration:
1. Header animation
2. Project card hover effects
3. Skill visualization
4. Timeline layout for experience
";

            // Act
            var dto = new BookmarkCreateRequest { Notes = detailedNotes };

            // Assert
            dto.Notes.Should().Be(detailedNotes);
        }

        #endregion

        #region BookmarkUpdateRequest Tests

        [Fact]
        public void BookmarkUpdateRequest_ShouldBeInstantiable()
        {
            // Act
            var dto = new BookmarkUpdateRequest();

            // Assert
            dto.Should().NotBeNull();
        }

        [Fact]
        public void BookmarkUpdateRequest_ShouldAcceptValidValues()
        {
            // Arrange
            var collectionName = "Updated Collection";
            var notes = "Updated notes with more details";

            // Act
            var dto = new BookmarkUpdateRequest
            {
                CollectionName = collectionName,
                Notes = notes
            };

            // Assert
            dto.CollectionName.Should().Be(collectionName);
            dto.Notes.Should().Be(notes);
        }

        [Fact]
        public void BookmarkUpdateRequest_ShouldHandleAllNullValues()
        {
            // Act
            var dto = new BookmarkUpdateRequest
            {
                CollectionName = null,
                Notes = null
            };

            // Assert
            dto.CollectionName.Should().BeNull();
            dto.Notes.Should().BeNull();
        }

        [Fact]
        public void BookmarkUpdateRequest_ShouldHandlePartialUpdates()
        {
            // Act
            var dto = new BookmarkUpdateRequest
            {
                CollectionName = "New Collection Name",
                // Leave notes null for partial update
                Notes = null
            };

            // Assert
            dto.CollectionName.Should().Be("New Collection Name");
            dto.Notes.Should().BeNull();
        }

        #endregion

        #region BookmarkToggleRequest Tests

        [Fact]
        public void BookmarkToggleRequest_ShouldBeInstantiable()
        {
            // Act
            var dto = new BookmarkToggleRequest();

            // Assert
            dto.Should().NotBeNull();
            dto.UserId.Should().Be(Guid.Empty);
            dto.PortfolioId.Should().Be(Guid.Empty);
        }

        [Fact]
        public void BookmarkToggleRequest_ShouldAcceptValidValues()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var collectionName = "Quick Bookmarks";
            var notes = "Bookmarked for later review";

            // Act
            var dto = new BookmarkToggleRequest
            {
                UserId = userId,
                PortfolioId = portfolioId,
                CollectionName = collectionName,
                Notes = notes
            };

            // Assert
            dto.UserId.Should().Be(userId);
            dto.PortfolioId.Should().Be(portfolioId);
            dto.CollectionName.Should().Be(collectionName);
            dto.Notes.Should().Be(notes);
        }

        [Fact]
        public void BookmarkToggleRequest_ShouldHandleNullOptionalFields()
        {
            // Act
            var dto = new BookmarkToggleRequest
            {
                CollectionName = null,
                Notes = null
            };

            // Assert
            dto.CollectionName.Should().BeNull();
            dto.Notes.Should().BeNull();
        }

        [Fact]
        public void BookmarkToggleRequest_ShouldHandleToggleWithoutCollection()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();

            // Act
            var dto = new BookmarkToggleRequest
            {
                UserId = userId,
                PortfolioId = portfolioId,
                CollectionName = null,
                Notes = null
            };

            // Assert
            dto.UserId.Should().Be(userId);
            dto.PortfolioId.Should().Be(portfolioId);
            dto.CollectionName.Should().BeNull();
            dto.Notes.Should().BeNull();
        }

        #endregion

        #region BookmarkResponse Tests

        [Fact]
        public void BookmarkResponse_ShouldBeInstantiable()
        {
            // Act
            var dto = new BookmarkResponse();

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(Guid.Empty);
            dto.UserId.Should().Be(Guid.Empty);
            dto.PortfolioId.Should().Be(Guid.Empty);
            dto.CreatedAt.Should().Be(default(DateTime));
        }

        [Fact]
        public void BookmarkResponse_ShouldAcceptValidValues()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var collectionName = "Test Collection";
            var notes = "Test notes";
            var createdAt = DateTime.UtcNow.AddDays(-2);
            var portfolio = new PortfolioSummaryResponse { Id = portfolioId, Title = "Test Portfolio" };

            // Act
            var dto = new BookmarkResponse
            {
                Id = id,
                UserId = userId,
                PortfolioId = portfolioId,
                CollectionName = collectionName,
                Notes = notes,
                CreatedAt = createdAt,
                Portfolio = portfolio
            };

            // Assert
            dto.Id.Should().Be(id);
            dto.UserId.Should().Be(userId);
            dto.PortfolioId.Should().Be(portfolioId);
            dto.CollectionName.Should().Be(collectionName);
            dto.Notes.Should().Be(notes);
            dto.CreatedAt.Should().Be(createdAt);
            dto.Portfolio.Should().Be(portfolio);
        }

        [Fact]
        public void BookmarkResponse_ShouldHandleNullOptionalFields()
        {
            // Act
            var dto = new BookmarkResponse
            {
                CollectionName = null,
                Notes = null,
                Portfolio = null
            };

            // Assert
            dto.CollectionName.Should().BeNull();
            dto.Notes.Should().BeNull();
            dto.Portfolio.Should().BeNull();
        }

        [Fact]
        public void BookmarkResponse_ShouldHandleDateTimeVariations()
        {
            // Arrange
            var minDate = DateTime.MinValue;
            var maxDate = DateTime.MaxValue;
            var utcNow = DateTime.UtcNow;
            var localNow = DateTime.Now;

            // Act
            var dto1 = new BookmarkResponse { CreatedAt = minDate };
            var dto2 = new BookmarkResponse { CreatedAt = maxDate };
            var dto3 = new BookmarkResponse { CreatedAt = utcNow };
            var dto4 = new BookmarkResponse { CreatedAt = localNow };

            // Assert
            dto1.CreatedAt.Should().Be(minDate);
            dto2.CreatedAt.Should().Be(maxDate);
            dto3.CreatedAt.Should().Be(utcNow);
            dto4.CreatedAt.Should().Be(localNow);
        }

        [Fact]
        public void BookmarkResponse_ShouldHandlePortfolioReference()
        {
            // Arrange
            var portfolio = new PortfolioSummaryResponse
            {
                Id = Guid.NewGuid(),
                Title = "Referenced Portfolio",
                Bio = "Portfolio bio",
                ViewCount = 100,
                LikeCount = 50
            };

            // Act
            var dto = new BookmarkResponse { Portfolio = portfolio };

            // Assert
            dto.Portfolio.Should().NotBeNull();
            dto.Portfolio.Should().Be(portfolio);
            dto.Portfolio.Title.Should().Be("Referenced Portfolio");
        }

        #endregion

        #region BookmarkSummaryResponse Tests

        [Fact]
        public void BookmarkSummaryResponse_ShouldBeInstantiable()
        {
            // Act
            var dto = new BookmarkSummaryResponse();

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(Guid.Empty);
            dto.UserId.Should().Be(Guid.Empty);
            dto.PortfolioId.Should().Be(Guid.Empty);
            dto.CreatedAt.Should().Be(default(DateTime));
        }

        [Fact]
        public void BookmarkSummaryResponse_ShouldAcceptValidValues()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var collectionName = "Summary Collection";
            var createdAt = DateTime.UtcNow.AddDays(-1);
            var portfolio = new PortfolioSummaryResponse { Id = portfolioId, Title = "Summary Portfolio" };

            // Act
            var dto = new BookmarkSummaryResponse
            {
                Id = id,
                UserId = userId,
                PortfolioId = portfolioId,
                CollectionName = collectionName,
                CreatedAt = createdAt,
                Portfolio = portfolio
            };

            // Assert
            dto.Id.Should().Be(id);
            dto.UserId.Should().Be(userId);
            dto.PortfolioId.Should().Be(portfolioId);
            dto.CollectionName.Should().Be(collectionName);
            dto.CreatedAt.Should().Be(createdAt);
            dto.Portfolio.Should().Be(portfolio);
        }

        [Fact]
        public void BookmarkSummaryResponse_ShouldHandleNullOptionalFields()
        {
            // Act
            var dto = new BookmarkSummaryResponse
            {
                CollectionName = null,
                Portfolio = null
            };

            // Assert
            dto.CollectionName.Should().BeNull();
            dto.Portfolio.Should().BeNull();
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public void AllBookmarkDTOs_ShouldHandleGuidEmpty()
        {
            // Act
            var createRequest = new BookmarkCreateRequest { UserId = Guid.Empty, PortfolioId = Guid.Empty };
            var toggleRequest = new BookmarkToggleRequest { UserId = Guid.Empty, PortfolioId = Guid.Empty };
            var response = new BookmarkResponse { Id = Guid.Empty, UserId = Guid.Empty, PortfolioId = Guid.Empty };
            var summaryResponse = new BookmarkSummaryResponse { Id = Guid.Empty, UserId = Guid.Empty, PortfolioId = Guid.Empty };

            // Assert
            createRequest.UserId.Should().Be(Guid.Empty);
            createRequest.PortfolioId.Should().Be(Guid.Empty);
            toggleRequest.UserId.Should().Be(Guid.Empty);
            toggleRequest.PortfolioId.Should().Be(Guid.Empty);
            response.Id.Should().Be(Guid.Empty);
            response.UserId.Should().Be(Guid.Empty);
            response.PortfolioId.Should().Be(Guid.Empty);
            summaryResponse.Id.Should().Be(Guid.Empty);
            summaryResponse.UserId.Should().Be(Guid.Empty);
            summaryResponse.PortfolioId.Should().Be(Guid.Empty);
        }

        [Fact]
        public void AllBookmarkDTOs_ShouldHandleMaxGuidValues()
        {
            // Arrange
            var maxGuid = new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");

            // Act
            var createRequest = new BookmarkCreateRequest { UserId = maxGuid, PortfolioId = maxGuid };
            var response = new BookmarkResponse { Id = maxGuid, UserId = maxGuid, PortfolioId = maxGuid };

            // Assert
            createRequest.UserId.Should().Be(maxGuid);
            createRequest.PortfolioId.Should().Be(maxGuid);
            response.Id.Should().Be(maxGuid);
            response.UserId.Should().Be(maxGuid);
            response.PortfolioId.Should().Be(maxGuid);
        }

        [Fact]
        public void AllBookmarkDTOs_ShouldHandleWhitespaceStrings()
        {
            // Arrange
            var whitespace = "   \t\n\r   ";

            // Act
            var createRequest = new BookmarkCreateRequest
            {
                CollectionName = whitespace,
                Notes = whitespace
            };

            var updateRequest = new BookmarkUpdateRequest
            {
                CollectionName = whitespace,
                Notes = whitespace
            };

            var toggleRequest = new BookmarkToggleRequest
            {
                CollectionName = whitespace,
                Notes = whitespace
            };

            // Assert
            createRequest.CollectionName.Should().Be(whitespace);
            createRequest.Notes.Should().Be(whitespace);
            updateRequest.CollectionName.Should().Be(whitespace);
            updateRequest.Notes.Should().Be(whitespace);
            toggleRequest.CollectionName.Should().Be(whitespace);
            toggleRequest.Notes.Should().Be(whitespace);
        }

        [Fact]
        public void BookmarkDTOs_ShouldHandleCollectionNameVariations()
        {
            // Arrange
            var collectionNames = new[]
            {
                "Favorites",
                "To Review Later",
                "Design Inspiration",
                "Career References",
                "Learning Examples",
                "UI/UX Showcase",
                "Development Portfolios",
                "Creative Works",
                "Professional Profiles",
                "Industry Leaders"
            };

            // Act & Assert
            foreach (var collectionName in collectionNames)
            {
                var createRequest = new BookmarkCreateRequest { CollectionName = collectionName };
                var updateRequest = new BookmarkUpdateRequest { CollectionName = collectionName };
                var toggleRequest = new BookmarkToggleRequest { CollectionName = collectionName };

                createRequest.CollectionName.Should().Be(collectionName);
                updateRequest.CollectionName.Should().Be(collectionName);
                toggleRequest.CollectionName.Should().Be(collectionName);
            }
        }

        [Fact]
        public void BookmarkDTOs_ShouldHandleNotesWithSpecialFormatting()
        {
            // Arrange
            var formattedNotes = @"
Rating: ⭐⭐⭐⭐⭐ (5/5)

Strengths:
✅ Modern design
✅ Great animations
✅ Mobile responsive
✅ Fast loading

Areas to learn from:
📚 CSS Grid layout
📚 JavaScript animations
📚 Color scheme choices

Follow-up actions:
🔄 Contact for networking
🔄 Study code structure
🔄 Implement similar features
";

            // Act
            var createRequest = new BookmarkCreateRequest { Notes = formattedNotes };
            var updateRequest = new BookmarkUpdateRequest { Notes = formattedNotes };

            // Assert
            createRequest.Notes.Should().Be(formattedNotes);
            updateRequest.Notes.Should().Be(formattedNotes);
        }

        [Fact]
        public void BookmarkDTOs_ShouldBeSerializationFriendly()
        {
            // This test ensures DTOs have proper structure for serialization
            
            // Arrange
            var createRequest = new BookmarkCreateRequest();
            var updateRequest = new BookmarkUpdateRequest();
            var toggleRequest = new BookmarkToggleRequest();
            var response = new BookmarkResponse();
            var summaryResponse = new BookmarkSummaryResponse();

            // Act & Assert - Should not throw
            var createRequestProperties = createRequest.GetType().GetProperties();
            var updateRequestProperties = updateRequest.GetType().GetProperties();
            var toggleRequestProperties = toggleRequest.GetType().GetProperties();
            var responseProperties = response.GetType().GetProperties();
            var summaryResponseProperties = summaryResponse.GetType().GetProperties();

            createRequestProperties.Should().AllSatisfy(p => p.CanRead.Should().BeTrue());
            updateRequestProperties.Should().AllSatisfy(p => p.CanRead.Should().BeTrue());
            toggleRequestProperties.Should().AllSatisfy(p => p.CanRead.Should().BeTrue());
            responseProperties.Should().AllSatisfy(p => p.CanRead.Should().BeTrue());
            summaryResponseProperties.Should().AllSatisfy(p => p.CanRead.Should().BeTrue());
        }

        [Fact]
        public void BookmarkDTOs_ShouldHandleMemoryStress()
        {
            // Arrange & Act
            var bookmarks = new List<BookmarkResponse>();
            for (int i = 0; i < 1000; i++)
            {
                bookmarks.Add(new BookmarkResponse
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    PortfolioId = Guid.NewGuid(),
                    CollectionName = _fixture.Create<string>(),
                    Notes = _fixture.Create<string>(),
                    CreatedAt = DateTime.UtcNow.AddDays(-i)
                });
            }

            // Assert - Should not throw OutOfMemoryException
            bookmarks.Should().HaveCount(1000);
        }

        [Fact]
        public void BookmarkCreateRequest_ShouldHandleConcurrentAccess()
        {
            // This test verifies thread safety of DTO instantiation
            var tasks = new List<Task<BookmarkCreateRequest>>();

            for (int i = 0; i < 100; i++)
            {
                var index = i; // Capture for closure
                tasks.Add(Task.Run(() => new BookmarkCreateRequest
                {
                    UserId = Guid.NewGuid(),
                    PortfolioId = Guid.NewGuid(),
                    CollectionName = $"Collection {index}",
                    Notes = $"Notes for bookmark {index}"
                }));
            }

            var results = Task.WhenAll(tasks).Result;

            // Assert
            results.Should().HaveCount(100);
            results.Should().AllSatisfy(dto => dto.Should().NotBeNull());
        }

        [Fact]
        public void BookmarkDTOs_ShouldHandleBookmarkingWorkflow()
        {
            // This test simulates a typical bookmarking workflow
            
            // Arrange - Create bookmark
            var userId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            
            var createRequest = new BookmarkCreateRequest
            {
                UserId = userId,
                PortfolioId = portfolioId,
                CollectionName = "Inspiration",
                Notes = "Great design patterns"
            };

            // Act - Toggle bookmark (for remove/add functionality)
            var toggleRequest = new BookmarkToggleRequest
            {
                UserId = userId,
                PortfolioId = portfolioId,
                CollectionName = "Inspiration",
                Notes = "Updated notes"
            };

            // Update bookmark details
            var updateRequest = new BookmarkUpdateRequest
            {
                CollectionName = "Design References",
                Notes = "Updated: Excellent use of animations and micro-interactions"
            };

            var response = new BookmarkResponse
            {
                UserId = userId,
                PortfolioId = portfolioId,
                CollectionName = updateRequest.CollectionName,
                Notes = updateRequest.Notes,
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            createRequest.UserId.Should().Be(userId);
            createRequest.PortfolioId.Should().Be(portfolioId);
            toggleRequest.UserId.Should().Be(userId);
            toggleRequest.PortfolioId.Should().Be(portfolioId);
            updateRequest.CollectionName.Should().Be("Design References");
            response.CollectionName.Should().Be("Design References");
            response.Notes.Should().Contain("animations and micro-interactions");
        }

        [Fact]
        public void BookmarkDTOs_ShouldHandleMultipleCollections()
        {
            // This test verifies handling of multiple collections per user
            
            // Arrange
            var userId = Guid.NewGuid();
            var collections = new[]
            {
                "Favorites",
                "Design Inspiration",
                "Development Examples",
                "Career Goals",
                "Learning Resources"
            };

            // Act
            var bookmarks = collections.Select((collection, index) => new BookmarkCreateRequest
            {
                UserId = userId,
                PortfolioId = Guid.NewGuid(),
                CollectionName = collection,
                Notes = $"Portfolio for {collection} collection"
            }).ToList();

            // Assert
            bookmarks.Should().HaveCount(5);
            bookmarks.Should().AllSatisfy(b => b.UserId.Should().Be(userId));
            bookmarks.Select(b => b.CollectionName).Should().BeEquivalentTo(collections);
        }

        #endregion
    }
} 