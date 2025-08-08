using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using backend_portfolio.DTO.BlogPost.Request;
using backend_portfolio.DTO.BlogPost.Response;
using AutoFixture;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend_portfolio.tests.DTO
{
    public class BlogPostDTOTests
    {
        private readonly Fixture _fixture;

        public BlogPostDTOTests()
        {
            _fixture = new Fixture();
        }

        #region BlogPostCreateRequest Tests

        [Fact]
        public void BlogPostCreateRequest_ShouldBeInstantiable()
        {
            // Act
            var dto = new BlogPostCreateRequest();

            // Assert
            dto.Should().NotBeNull();
            dto.PortfolioId.Should().Be(Guid.Empty);
            dto.Title.Should().NotBeNull();
            dto.IsPublished.Should().BeFalse();
        }

        [Fact]
        public void BlogPostCreateRequest_ShouldAcceptValidValues()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var title = "My First Blog Post";
            var excerpt = "This is an excerpt of my blog post";
            var content = "This is the full content of my blog post with detailed information.";
            var featuredImageUrl = "https://example.com/image.jpg";
            var tags = new[] { "programming", "web-development", "tutorial" };

            // Act
            var dto = new BlogPostCreateRequest
            {
                PortfolioId = portfolioId,
                Title = title,
                Excerpt = excerpt,
                Content = content,
                FeaturedImageUrl = featuredImageUrl,
                Tags = tags,
                IsPublished = true
            };

            // Assert
            dto.PortfolioId.Should().Be(portfolioId);
            dto.Title.Should().Be(title);
            dto.Excerpt.Should().Be(excerpt);
            dto.Content.Should().Be(content);
            dto.FeaturedImageUrl.Should().Be(featuredImageUrl);
            dto.Tags.Should().BeEquivalentTo(tags);
            dto.IsPublished.Should().BeTrue();
        }

        [Fact]
        public void BlogPostCreateRequest_ShouldHandleNullOptionalFields()
        {
            // Act
            var dto = new BlogPostCreateRequest
            {
                Excerpt = null,
                Content = null,
                FeaturedImageUrl = null,
                Tags = null
            };

            // Assert
            dto.Excerpt.Should().BeNull();
            dto.Content.Should().BeNull();
            dto.FeaturedImageUrl.Should().BeNull();
            dto.Tags.Should().BeNull();
        }

        [Fact]
        public void BlogPostCreateRequest_ShouldHandleEmptyStrings()
        {
            // Act
            var dto = new BlogPostCreateRequest
            {
                Title = "",
                Excerpt = "",
                Content = "",
                FeaturedImageUrl = ""
            };

            // Assert
            dto.Title.Should().Be("");
            dto.Excerpt.Should().Be("");
            dto.Content.Should().Be("");
            dto.FeaturedImageUrl.Should().Be("");
        }

        [Fact]
        public void BlogPostCreateRequest_ShouldHandleEmptyTagsArray()
        {
            // Act
            var dto = new BlogPostCreateRequest
            {
                Tags = new string[0]
            };

            // Assert
            dto.Tags.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void BlogPostCreateRequest_ShouldHandleLargeTagsArray()
        {
            // Arrange
            var tags = _fixture.CreateMany<string>(100).ToArray();

            // Act
            var dto = new BlogPostCreateRequest { Tags = tags };

            // Assert
            dto.Tags.Should().HaveCount(100);
            dto.Tags.Should().BeEquivalentTo(tags);
        }

        [Theory]
        [InlineData("https://example.com/image.jpg")]
        [InlineData("http://example.com/image.png")]
        [InlineData("https://cdn.example.com/images/featured.gif")]
        [InlineData("relative/path/image.webp")]
        [InlineData("")]
        public void BlogPostCreateRequest_ShouldHandleVariousFeaturedImageUrls(string imageUrl)
        {
            // Act
            var dto = new BlogPostCreateRequest { FeaturedImageUrl = imageUrl };

            // Assert
            dto.FeaturedImageUrl.Should().Be(imageUrl);
        }

        [Fact]
        public void BlogPostCreateRequest_ShouldHandleSpecialCharactersInTitle()
        {
            // Arrange
            var specialTitle = "How to use C# & .NET: A Guide to Modern Development (2024)";

            // Act
            var dto = new BlogPostCreateRequest { Title = specialTitle };

            // Assert
            dto.Title.Should().Be(specialTitle);
        }

        [Fact]
        public void BlogPostCreateRequest_ShouldHandleUnicodeCharacters()
        {
            // Arrange
            var unicodeTitle = "编程教程 🚀 プログラミングチュートリアル";
            var unicodeContent = "学习编程的最佳方法 プログラミングを学ぶ最良の方法";
            var unicodeExcerpt = "简介：本文将介绍... 概要：この記事では...";

            // Act
            var dto = new BlogPostCreateRequest
            {
                Title = unicodeTitle,
                Content = unicodeContent,
                Excerpt = unicodeExcerpt
            };

            // Assert
            dto.Title.Should().Be(unicodeTitle);
            dto.Content.Should().Be(unicodeContent);
            dto.Excerpt.Should().Be(unicodeExcerpt);
        }

        [Fact]
        public void BlogPostCreateRequest_ShouldHandleLongContent()
        {
            // Arrange
            var longTitle = new string('a', 10000);
            var longExcerpt = new string('b', 50000);
            var longContent = new string('c', 500000);

            // Act
            var dto = new BlogPostCreateRequest
            {
                Title = longTitle,
                Excerpt = longExcerpt,
                Content = longContent
            };

            // Assert
            dto.Title.Should().Be(longTitle);
            dto.Excerpt.Should().Be(longExcerpt);
            dto.Content.Should().Be(longContent);
        }

        [Fact]
        public void BlogPostCreateRequest_ShouldHandleMarkdownContent()
        {
            // Arrange
            var markdownContent = @"
# My Blog Post

This is a **bold** statement and this is *italic*.

## Code Example

```csharp
public class Example
{
    public string Name { get; set; }
}
```

### List of Items
- Item 1
- Item 2
- Item 3

[Link to example](https://example.com)
";

            // Act
            var dto = new BlogPostCreateRequest { Content = markdownContent };

            // Assert
            dto.Content.Should().Be(markdownContent);
        }

        [Fact]
        public void BlogPostCreateRequest_ShouldHandleCommonTags()
        {
            // Arrange
            var commonTags = new[]
            {
                "programming", "web-development", "tutorial", "guide",
                "javascript", "react", "node.js", "backend", "frontend",
                "database", "sql", "mongodb", "aws", "docker", "kubernetes"
            };

            // Act
            var dto = new BlogPostCreateRequest { Tags = commonTags };

            // Assert
            dto.Tags.Should().BeEquivalentTo(commonTags);
        }

        [Fact]
        public void BlogPostCreateRequest_ShouldHandleTagsWithSpecialCharacters()
        {
            // Arrange
            var specialTags = new[] { "C#", "ASP.NET", ".NET Core", "React.js", "Node.js", "AI/ML" };

            // Act
            var dto = new BlogPostCreateRequest { Tags = specialTags };

            // Assert
            dto.Tags.Should().BeEquivalentTo(specialTags);
        }

        #endregion

        #region BlogPostUpdateRequest Tests

        [Fact]
        public void BlogPostUpdateRequest_ShouldBeInstantiable()
        {
            // Act
            var dto = new BlogPostUpdateRequest();

            // Assert
            dto.Should().NotBeNull();
        }

        [Fact]
        public void BlogPostUpdateRequest_ShouldAcceptValidValues()
        {
            // Arrange
            var title = "Updated Blog Post Title";
            var excerpt = "Updated excerpt";
            var content = "Updated content with more information";
            var featuredImageUrl = "https://example.com/updated-image.jpg";
            var tags = new[] { "updated", "blog", "post" };

            // Act
            var dto = new BlogPostUpdateRequest
            {
                Title = title,
                Excerpt = excerpt,
                Content = content,
                FeaturedImageUrl = featuredImageUrl,
                Tags = tags,
                IsPublished = true
            };

            // Assert
            dto.Title.Should().Be(title);
            dto.Excerpt.Should().Be(excerpt);
            dto.Content.Should().Be(content);
            dto.FeaturedImageUrl.Should().Be(featuredImageUrl);
            dto.Tags.Should().BeEquivalentTo(tags);
            dto.IsPublished.Should().BeTrue();
        }

        [Fact]
        public void BlogPostUpdateRequest_ShouldHandleAllNullValues()
        {
            // Act
            var dto = new BlogPostUpdateRequest
            {
                Title = null,
                Excerpt = null,
                Content = null,
                FeaturedImageUrl = null,
                Tags = null,
                IsPublished = null
            };

            // Assert
            dto.Title.Should().BeNull();
            dto.Excerpt.Should().BeNull();
            dto.Content.Should().BeNull();
            dto.FeaturedImageUrl.Should().BeNull();
            dto.Tags.Should().BeNull();
            dto.IsPublished.Should().BeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(null)]
        public void BlogPostUpdateRequest_ShouldHandleNullableIsPublished(bool? isPublished)
        {
            // Act
            var dto = new BlogPostUpdateRequest { IsPublished = isPublished };

            // Assert
            dto.IsPublished.Should().Be(isPublished);
        }

        [Fact]
        public void BlogPostUpdateRequest_ShouldHandlePartialUpdates()
        {
            // Act
            var dto = new BlogPostUpdateRequest
            {
                Title = "New Title",
                // Leave other fields null for partial update
                Content = null,
                Tags = null
            };

            // Assert
            dto.Title.Should().Be("New Title");
            dto.Content.Should().BeNull();
            dto.Tags.Should().BeNull();
        }

        [Fact]
        public void BlogPostUpdateRequest_ShouldHandlePublishStatusUpdate()
        {
            // Act
            var dto = new BlogPostUpdateRequest
            {
                IsPublished = true,
                Title = "Publishing this post"
            };

            // Assert
            dto.IsPublished.Should().BeTrue();
            dto.Title.Should().Be("Publishing this post");
        }

        #endregion

        #region BlogPostResponse Tests

        [Fact]
        public void BlogPostResponse_ShouldBeInstantiable()
        {
            // Act
            var dto = new BlogPostResponse();

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(Guid.Empty);
            dto.PortfolioId.Should().Be(Guid.Empty);
            dto.Title.Should().NotBeNull();
            dto.IsPublished.Should().BeFalse();
            dto.CreatedAt.Should().Be(default(DateTime));
            dto.UpdatedAt.Should().Be(default(DateTime));
        }

        [Fact]
        public void BlogPostResponse_ShouldAcceptValidValues()
        {
            // Arrange
            var id = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var title = "Test Blog Post Response";
            var excerpt = "Test excerpt";
            var content = "Test content";
            var featuredImageUrl = "https://example.com/test-image.jpg";
            var tags = new[] { "test", "blog" };
            var publishedAt = DateTime.UtcNow.AddDays(-1);
            var createdAt = DateTime.UtcNow.AddDays(-5);
            var updatedAt = DateTime.UtcNow;

            // Act
            var dto = new BlogPostResponse
            {
                Id = id,
                PortfolioId = portfolioId,
                Title = title,
                Excerpt = excerpt,
                Content = content,
                FeaturedImageUrl = featuredImageUrl,
                Tags = tags,
                IsPublished = true,
                PublishedAt = publishedAt,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };

            // Assert
            dto.Id.Should().Be(id);
            dto.PortfolioId.Should().Be(portfolioId);
            dto.Title.Should().Be(title);
            dto.Excerpt.Should().Be(excerpt);
            dto.Content.Should().Be(content);
            dto.FeaturedImageUrl.Should().Be(featuredImageUrl);
            dto.Tags.Should().BeEquivalentTo(tags);
            dto.IsPublished.Should().BeTrue();
            dto.PublishedAt.Should().Be(publishedAt);
            dto.CreatedAt.Should().Be(createdAt);
            dto.UpdatedAt.Should().Be(updatedAt);
        }

        [Fact]
        public void BlogPostResponse_ShouldHandleNullOptionalFields()
        {
            // Act
            var dto = new BlogPostResponse
            {
                Excerpt = null,
                Content = null,
                FeaturedImageUrl = null,
                Tags = null,
                PublishedAt = null
            };

            // Assert
            dto.Excerpt.Should().BeNull();
            dto.Content.Should().BeNull();
            dto.FeaturedImageUrl.Should().BeNull();
            dto.Tags.Should().BeNull();
            dto.PublishedAt.Should().BeNull();
        }

        [Fact]
        public void BlogPostResponse_ShouldHandleDateTimeVariations()
        {
            // Arrange
            var minDate = DateTime.MinValue;
            var maxDate = DateTime.MaxValue;
            var utcNow = DateTime.UtcNow;
            var localNow = DateTime.Now;

            // Act
            var dto1 = new BlogPostResponse 
            { 
                CreatedAt = minDate, 
                UpdatedAt = maxDate,
                PublishedAt = utcNow
            };
            var dto2 = new BlogPostResponse 
            { 
                CreatedAt = utcNow, 
                UpdatedAt = localNow,
                PublishedAt = localNow
            };

            // Assert
            dto1.CreatedAt.Should().Be(minDate);
            dto1.UpdatedAt.Should().Be(maxDate);
            dto1.PublishedAt.Should().Be(utcNow);
            dto2.CreatedAt.Should().Be(utcNow);
            dto2.UpdatedAt.Should().Be(localNow);
            dto2.PublishedAt.Should().Be(localNow);
        }

        [Fact]
        public void BlogPostResponse_ShouldHandleUnpublishedPost()
        {
            // Act
            var dto = new BlogPostResponse
            {
                IsPublished = false,
                PublishedAt = null
            };

            // Assert
            dto.IsPublished.Should().BeFalse();
            dto.PublishedAt.Should().BeNull();
        }

        #endregion

        #region BlogPostSummaryResponse Tests

        [Fact]
        public void BlogPostSummaryResponse_ShouldBeInstantiable()
        {
            // Act
            var dto = new BlogPostSummaryResponse();

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(Guid.Empty);
            dto.Title.Should().NotBeNull();
            dto.IsPublished.Should().BeFalse();
            dto.UpdatedAt.Should().Be(default(DateTime));
        }

        [Fact]
        public void BlogPostSummaryResponse_ShouldAcceptValidValues()
        {
            // Arrange
            var id = Guid.NewGuid();
            var title = "Summary Blog Post";
            var excerpt = "Summary excerpt";
            var featuredImageUrl = "https://example.com/summary-image.jpg";
            var tags = new[] { "summary", "test" };
            var publishedAt = DateTime.UtcNow.AddDays(-2);
            var updatedAt = DateTime.UtcNow;

            // Act
            var dto = new BlogPostSummaryResponse
            {
                Id = id,
                Title = title,
                Excerpt = excerpt,
                FeaturedImageUrl = featuredImageUrl,
                Tags = tags,
                IsPublished = true,
                PublishedAt = publishedAt,
                UpdatedAt = updatedAt
            };

            // Assert
            dto.Id.Should().Be(id);
            dto.Title.Should().Be(title);
            dto.Excerpt.Should().Be(excerpt);
            dto.FeaturedImageUrl.Should().Be(featuredImageUrl);
            dto.Tags.Should().BeEquivalentTo(tags);
            dto.IsPublished.Should().BeTrue();
            dto.PublishedAt.Should().Be(publishedAt);
            dto.UpdatedAt.Should().Be(updatedAt);
        }

        [Fact]
        public void BlogPostSummaryResponse_ShouldHandleNullOptionalFields()
        {
            // Act
            var dto = new BlogPostSummaryResponse
            {
                Excerpt = null,
                FeaturedImageUrl = null,
                Tags = null,
                PublishedAt = null
            };

            // Assert
            dto.Excerpt.Should().BeNull();
            dto.FeaturedImageUrl.Should().BeNull();
            dto.Tags.Should().BeNull();
            dto.PublishedAt.Should().BeNull();
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public void AllBlogPostDTOs_ShouldHandleGuidEmpty()
        {
            // Act
            var createRequest = new BlogPostCreateRequest { PortfolioId = Guid.Empty };
            var response = new BlogPostResponse { Id = Guid.Empty, PortfolioId = Guid.Empty };
            var summaryResponse = new BlogPostSummaryResponse { Id = Guid.Empty };

            // Assert
            createRequest.PortfolioId.Should().Be(Guid.Empty);
            response.Id.Should().Be(Guid.Empty);
            response.PortfolioId.Should().Be(Guid.Empty);
            summaryResponse.Id.Should().Be(Guid.Empty);
        }

        [Fact]
        public void AllBlogPostDTOs_ShouldHandleMaxGuidValues()
        {
            // Arrange
            var maxGuid = new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");

            // Act
            var createRequest = new BlogPostCreateRequest { PortfolioId = maxGuid };
            var response = new BlogPostResponse { Id = maxGuid, PortfolioId = maxGuid };

            // Assert
            createRequest.PortfolioId.Should().Be(maxGuid);
            response.Id.Should().Be(maxGuid);
            response.PortfolioId.Should().Be(maxGuid);
        }

        [Fact]
        public void AllBlogPostDTOs_ShouldHandleWhitespaceStrings()
        {
            // Arrange
            var whitespace = "   \t\n\r   ";

            // Act
            var createRequest = new BlogPostCreateRequest
            {
                Title = whitespace,
                Excerpt = whitespace,
                Content = whitespace
            };

            var updateRequest = new BlogPostUpdateRequest
            {
                Title = whitespace,
                Excerpt = whitespace,
                Content = whitespace
            };

            // Assert
            createRequest.Title.Should().Be(whitespace);
            createRequest.Excerpt.Should().Be(whitespace);
            createRequest.Content.Should().Be(whitespace);
            updateRequest.Title.Should().Be(whitespace);
            updateRequest.Excerpt.Should().Be(whitespace);
            updateRequest.Content.Should().Be(whitespace);
        }

        [Fact]
        public void AllBlogPostDTOs_ShouldHandleDuplicateTags()
        {
            // Arrange
            var tags = new[] { "programming", "programming", "tutorial", "tutorial" };

            // Act
            var createRequest = new BlogPostCreateRequest { Tags = tags };
            var updateRequest = new BlogPostUpdateRequest { Tags = tags };

            // Assert
            createRequest.Tags.Should().HaveCount(4);
            createRequest.Tags.Should().BeEquivalentTo(tags);
            updateRequest.Tags.Should().HaveCount(4);
            updateRequest.Tags.Should().BeEquivalentTo(tags);
        }

        [Fact]
        public void AllBlogPostDTOs_ShouldHandleInvalidUrls()
        {
            // Arrange
            var invalidUrls = new[]
            {
                "not-a-url",
                "ftp://invalid.com",
                "javascript:alert('xss')",
                "file:///etc/passwd",
                "data:text/html,<script>alert('xss')</script>"
            };

            // Act & Assert - DTOs should accept any string, validation happens elsewhere
            foreach (var url in invalidUrls)
            {
                var dto = new BlogPostCreateRequest { FeaturedImageUrl = url };
                dto.FeaturedImageUrl.Should().Be(url);
            }
        }

        [Fact]
        public void AllBlogPostDTOs_ShouldHandleVeryLongUrls()
        {
            // Arrange
            var longUrl = "https://example.com/" + new string('a', 10000);

            // Act
            var dto = new BlogPostCreateRequest { FeaturedImageUrl = longUrl };

            // Assert
            dto.FeaturedImageUrl.Should().Be(longUrl);
        }

        [Fact]
        public void BlogPostDTOs_ShouldHandleHtmlInContent()
        {
            // Arrange
            var htmlContent = @"<h1>My Blog Post</h1>
<p>This is a paragraph with <strong>bold</strong> and <em>italic</em> text.</p>
<ul>
<li>Item 1</li>
<li>Item 2</li>
</ul>
<a href=""https://example.com"">Link</a>";

            // Act
            var dto = new BlogPostCreateRequest { Content = htmlContent };

            // Assert
            dto.Content.Should().Be(htmlContent);
        }

        [Fact]
        public void BlogPostDTOs_ShouldHandleJsonInContent()
        {
            // Arrange
            var jsonContent = @"{
  ""title"": ""My API Response"",
  ""data"": [
    {""id"": 1, ""name"": ""Test""},
    {""id"": 2, ""name"": ""Example""}
  ]
}";

            // Act
            var dto = new BlogPostCreateRequest { Content = jsonContent };

            // Assert
            dto.Content.Should().Be(jsonContent);
        }

        [Fact]
        public void BlogPostDTOs_ShouldHandleCodeBlocksInContent()
        {
            // Arrange
            var codeContent = @"```csharp
public class BlogPost
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
}
```";

            // Act
            var dto = new BlogPostCreateRequest { Content = codeContent };

            // Assert
            dto.Content.Should().Be(codeContent);
        }

        [Fact]
        public void BlogPostDTOs_ShouldBeSerializationFriendly()
        {
            // This test ensures DTOs have proper structure for serialization
            
            // Arrange
            var createRequest = new BlogPostCreateRequest();
            var updateRequest = new BlogPostUpdateRequest();
            var response = new BlogPostResponse();
            var summaryResponse = new BlogPostSummaryResponse();

            // Act & Assert - Should not throw
            var createRequestProperties = createRequest.GetType().GetProperties();
            var updateRequestProperties = updateRequest.GetType().GetProperties();
            var responseProperties = response.GetType().GetProperties();
            var summaryResponseProperties = summaryResponse.GetType().GetProperties();

            createRequestProperties.Should().AllSatisfy(p => p.CanRead.Should().BeTrue());
            updateRequestProperties.Should().AllSatisfy(p => p.CanRead.Should().BeTrue());
            responseProperties.Should().AllSatisfy(p => p.CanRead.Should().BeTrue());
            summaryResponseProperties.Should().AllSatisfy(p => p.CanRead.Should().BeTrue());
        }

        [Fact]
        public void BlogPostDTOs_ShouldHandleMemoryStress()
        {
            // Arrange & Act
            var blogPosts = new List<BlogPostResponse>();
            for (int i = 0; i < 1000; i++)
            {
                blogPosts.Add(new BlogPostResponse
                {
                    Id = Guid.NewGuid(),
                    Title = _fixture.Create<string>(),
                    Content = _fixture.Create<string>(),
                    Excerpt = _fixture.Create<string>(),
                    Tags = _fixture.CreateMany<string>(5).ToArray()
                });
            }

            // Assert - Should not throw OutOfMemoryException
            blogPosts.Should().HaveCount(1000);
        }

        [Fact]
        public void BlogPostCreateRequest_ShouldHandleConcurrentAccess()
        {
            // This test verifies thread safety of DTO instantiation
            var tasks = new List<Task<BlogPostCreateRequest>>();

            for (int i = 0; i < 100; i++)
            {
                var index = i; // Capture for closure
                tasks.Add(Task.Run(() => new BlogPostCreateRequest
                {
                    Title = $"Blog Post {index}",
                    Content = $"Content for post {index}",
                    Tags = new[] { "tag1", "tag2" }
                }));
            }

            var results = Task.WhenAll(tasks).Result;

            // Assert
            results.Should().HaveCount(100);
            results.Should().AllSatisfy(dto => dto.Should().NotBeNull());
        }

        [Fact]
        public void BlogPostDTOs_ShouldHandlePublishingWorkflow()
        {
            // This test simulates a typical publishing workflow
            
            // Arrange - Create draft
            var createRequest = new BlogPostCreateRequest
            {
                Title = "Draft Post",
                Content = "Draft content",
                IsPublished = false
            };

            // Act - Update to publish
            var updateRequest = new BlogPostUpdateRequest
            {
                IsPublished = true
            };

            var response = new BlogPostResponse
            {
                Title = createRequest.Title,
                Content = createRequest.Content,
                IsPublished = updateRequest.IsPublished.Value,
                PublishedAt = DateTime.UtcNow
            };

            // Assert
            createRequest.IsPublished.Should().BeFalse();
            updateRequest.IsPublished.Should().BeTrue();
            response.IsPublished.Should().BeTrue();
            response.PublishedAt.Should().NotBeNull();
        }

        #endregion
    }
} 