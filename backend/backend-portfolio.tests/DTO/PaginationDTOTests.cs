using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;
using FluentAssertions;
using backend_portfolio.DTO.Pagination;
using AutoFixture;

namespace backend_portfolio.tests.DTO
{
    public class PaginationDTOTests
    {
        private readonly Fixture _fixture;

        public PaginationDTOTests()
        {
            _fixture = new Fixture();
        }

        #region PaginationRequest Tests

        [Fact]
        public void PaginationRequest_ShouldBeInstantiable()
        {
            // Act
            var dto = new PaginationRequest();

            // Assert
            dto.Should().NotBeNull();
            dto.Page.Should().Be(1);
            dto.PageSize.Should().Be(20);
            dto.SortBy.Should().Be("most-recent");
            dto.SortDirection.Should().Be("desc");
        }

        [Fact]
        public void PaginationRequest_ShouldAcceptValidValues()
        {
            // Arrange
            var skills = new List<string> { "C#", "React" };
            var roles = new List<string> { "Developer", "Designer" };
            var dateFrom = DateTime.Now.AddDays(-30);
            var dateTo = DateTime.Now;

            // Act
            var dto = new PaginationRequest
            {
                Page = 5,
                PageSize = 25,
                SortBy = "title",
                SortDirection = "asc",
                SearchTerm = "test search",
                Skills = skills,
                Roles = roles,
                Featured = true,
                DateFrom = dateFrom,
                DateTo = dateTo
            };

            // Assert
            dto.Page.Should().Be(5);
            dto.PageSize.Should().Be(25);
            dto.SortBy.Should().Be("title");
            dto.SortDirection.Should().Be("asc");
            dto.SearchTerm.Should().Be("test search");
            dto.Skills.Should().BeEquivalentTo(skills);
            dto.Roles.Should().BeEquivalentTo(roles);
            dto.Featured.Should().BeTrue();
            dto.DateFrom.Should().Be(dateFrom);
            dto.DateTo.Should().Be(dateTo);
        }

        [Fact]
        public void PaginationRequest_PageSize_ShouldEnforceMaximum()
        {
            // Act
            var dto = new PaginationRequest { PageSize = 100 };

            // Assert
            dto.PageSize.Should().Be(50); // Should be capped at MaxPageSize
        }

        [Theory]
        [InlineData(1)]
        [InlineData(25)]
        [InlineData(50)]
        public void PaginationRequest_PageSize_ShouldAcceptValidValues(int pageSize)
        {
            // Act
            var dto = new PaginationRequest { PageSize = pageSize };

            // Assert
            dto.PageSize.Should().Be(pageSize);
        }

        [Theory]
        [InlineData(51)]
        [InlineData(100)]
        [InlineData(1000)]
        public void PaginationRequest_PageSize_ShouldCapAtMaximum(int pageSize)
        {
            // Act
            var dto = new PaginationRequest { PageSize = pageSize };

            // Assert
            dto.PageSize.Should().Be(50);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void PaginationRequest_PageSize_ShouldStillAcceptInvalidValues(int pageSize)
        {
            // Note: The setter only caps at maximum, doesn't enforce minimum
            // Validation attributes handle minimum validation
            
            // Act
            var dto = new PaginationRequest { PageSize = pageSize };

            // Assert
            dto.PageSize.Should().Be(pageSize);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(int.MaxValue)]
        public void PaginationRequest_Page_ShouldAcceptValidValues(int page)
        {
            // Act
            var dto = new PaginationRequest { Page = page };

            // Assert
            dto.Page.Should().Be(page);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void PaginationRequest_Page_ShouldAcceptInvalidValues(int page)
        {
            // Note: Validation attributes handle validation, property accepts any value
            
            // Act
            var dto = new PaginationRequest { Page = page };

            // Assert
            dto.Page.Should().Be(page);
        }

        [Fact]
        public void PaginationRequest_ShouldHandleNullValues()
        {
            // Act
            var dto = new PaginationRequest
            {
                SortBy = null,
                SortDirection = null,
                SearchTerm = null,
                Skills = null,
                Roles = null,
                Featured = null,
                DateFrom = null,
                DateTo = null
            };

            // Assert
            dto.SortBy.Should().BeNull();
            dto.SortDirection.Should().BeNull();
            dto.SearchTerm.Should().BeNull();
            dto.Skills.Should().BeNull();
            dto.Roles.Should().BeNull();
            dto.Featured.Should().BeNull();
            dto.DateFrom.Should().BeNull();
            dto.DateTo.Should().BeNull();
        }

        [Fact]
        public void PaginationRequest_ShouldHandleEmptyCollections()
        {
            // Act
            var dto = new PaginationRequest
            {
                Skills = new List<string>(),
                Roles = new List<string>()
            };

            // Assert
            dto.Skills.Should().NotBeNull().And.BeEmpty();
            dto.Roles.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void PaginationRequest_ShouldHandleLargeCollections()
        {
            // Arrange
            var skills = _fixture.CreateMany<string>(1000).ToList();
            var roles = _fixture.CreateMany<string>(500).ToList();

            // Act
            var dto = new PaginationRequest
            {
                Skills = skills,
                Roles = roles
            };

            // Assert
            dto.Skills.Should().HaveCount(1000);
            dto.Roles.Should().HaveCount(500);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("a")]
        [InlineData("very long search term that might be used in real scenarios")]
        public void PaginationRequest_SearchTerm_ShouldHandleVariousFormats(string searchTerm)
        {
            // Act
            var dto = new PaginationRequest { SearchTerm = searchTerm };

            // Assert
            dto.SearchTerm.Should().Be(searchTerm);
        }

        [Theory]
        [InlineData("title")]
        [InlineData("date")]
        [InlineData("most-recent")]
        [InlineData("")]
        [InlineData("TITLE")]
        public void PaginationRequest_SortBy_ShouldHandleVariousValues(string sortBy)
        {
            // Act
            var dto = new PaginationRequest { SortBy = sortBy };

            // Assert
            dto.SortBy.Should().Be(sortBy);
        }

        [Theory]
        [InlineData("asc")]
        [InlineData("desc")]
        [InlineData("ASC")]
        [InlineData("DESC")]
        [InlineData("")]
        [InlineData("invalid")]
        public void PaginationRequest_SortDirection_ShouldHandleVariousValues(string sortDirection)
        {
            // Act
            var dto = new PaginationRequest { SortDirection = sortDirection };

            // Assert
            dto.SortDirection.Should().Be(sortDirection);
        }

        [Fact]
        public void PaginationRequest_Featured_ShouldHandleBooleanValues()
        {
            // Act & Assert
            var trueFeatured = new PaginationRequest { Featured = true };
            trueFeatured.Featured.Should().BeTrue();

            var falseFeatured = new PaginationRequest { Featured = false };
            falseFeatured.Featured.Should().BeFalse();

            var nullFeatured = new PaginationRequest { Featured = null };
            nullFeatured.Featured.Should().BeNull();
        }

        [Fact]
        public void PaginationRequest_DateRange_ShouldHandleVariousDateScenarios()
        {
            // Arrange
            var pastDate = DateTime.Now.AddYears(-10);
            var futureDate = DateTime.Now.AddYears(10);
            var minDate = DateTime.MinValue;
            var maxDate = DateTime.MaxValue;

            // Act & Assert
            var dto1 = new PaginationRequest { DateFrom = pastDate, DateTo = futureDate };
            dto1.DateFrom.Should().Be(pastDate);
            dto1.DateTo.Should().Be(futureDate);

            var dto2 = new PaginationRequest { DateFrom = minDate, DateTo = maxDate };
            dto2.DateFrom.Should().Be(minDate);
            dto2.DateTo.Should().Be(maxDate);

            // Reversed dates (should still be accepted by DTO)
            var dto3 = new PaginationRequest { DateFrom = futureDate, DateTo = pastDate };
            dto3.DateFrom.Should().Be(futureDate);
            dto3.DateTo.Should().Be(pastDate);
        }

        [Fact]
        public void PaginationRequest_Skills_ShouldHandleSpecialCharacters()
        {
            // Arrange
            var skills = new List<string> { "C#", "C++", ".NET", "ASP.NET Core", "React.js", "Node.js" };

            // Act
            var dto = new PaginationRequest { Skills = skills };

            // Assert
            dto.Skills.Should().BeEquivalentTo(skills);
        }

        [Fact]
        public void PaginationRequest_Roles_ShouldHandleDuplicateValues()
        {
            // Arrange
            var roles = new List<string> { "Developer", "Developer", "Designer", "Designer" };

            // Act
            var dto = new PaginationRequest { Roles = roles };

            // Assert
            dto.Roles.Should().HaveCount(4);
            dto.Roles.Should().BeEquivalentTo(roles);
        }

        [Fact]
        public void PaginationRequest_ShouldHandleUnicodeStrings()
        {
            // Arrange
            var unicode = "测试搜索词 🚀 emoji 日本語";
            var unicodeSkills = new List<string> { "С#", "日本語プログラミング", "العربية" };

            // Act
            var dto = new PaginationRequest
            {
                SearchTerm = unicode,
                Skills = unicodeSkills
            };

            // Assert
            dto.SearchTerm.Should().Be(unicode);
            dto.Skills.Should().BeEquivalentTo(unicodeSkills);
        }

        #endregion

        #region PaginatedResponse Tests

        [Fact]
        public void PaginatedResponse_ShouldBeInstantiable()
        {
            // Act
            var dto = new PaginatedResponse<string>();

            // Assert
            dto.Should().NotBeNull();
            dto.Data.Should().NotBeNull().And.BeEmpty();
            dto.Pagination.Should().NotBeNull();
            dto.CacheKey.Should().BeNull();
            dto.CachedAt.Should().Be(default(DateTime));
        }

        [Fact]
        public void PaginatedResponse_ShouldAcceptData()
        {
            // Arrange
            var data = new List<string> { "item1", "item2", "item3" };
            var pagination = new PaginationMetadata { CurrentPage = 1, PageSize = 10 };
            var cacheKey = "test-cache-key";
            var cachedAt = DateTime.UtcNow;

            // Act
            var dto = new PaginatedResponse<string>
            {
                Data = data,
                Pagination = pagination,
                CacheKey = cacheKey,
                CachedAt = cachedAt
            };

            // Assert
            dto.Data.Should().BeEquivalentTo(data);
            dto.Pagination.Should().Be(pagination);
            dto.CacheKey.Should().Be(cacheKey);
            dto.CachedAt.Should().Be(cachedAt);
        }

        [Fact]
        public void PaginatedResponse_ShouldHandleGenericTypes()
        {
            // Arrange
            var intData = new List<int> { 1, 2, 3 };
            var guidData = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            // Act
            var intDto = new PaginatedResponse<int> { Data = intData };
            var guidDto = new PaginatedResponse<Guid> { Data = guidData };

            // Assert
            intDto.Data.Should().BeEquivalentTo(intData);
            guidDto.Data.Should().BeEquivalentTo(guidData);
        }

        [Fact]
        public void PaginatedResponse_ShouldHandleLargeDataSets()
        {
            // Arrange
            var largeData = _fixture.CreateMany<string>(10000).ToList();

            // Act
            var dto = new PaginatedResponse<string> { Data = largeData };

            // Assert
            dto.Data.Should().HaveCount(10000);
        }

        [Fact]
        public void PaginatedResponse_ShouldHandleComplexObjects()
        {
            // Arrange
            var complexData = new List<object>
            {
                new { Id = 1, Name = "Test1" },
                new { Id = 2, Name = "Test2" }
            };

            // Act
            var dto = new PaginatedResponse<object> { Data = complexData };

            // Assert
            dto.Data.Should().HaveCount(2);
            dto.Data.Should().BeEquivalentTo(complexData);
        }

        #endregion

        #region PaginationMetadata Tests

        [Fact]
        public void PaginationMetadata_ShouldBeInstantiable()
        {
            // Act
            var metadata = new PaginationMetadata();

            // Assert
            metadata.Should().NotBeNull();
            metadata.CurrentPage.Should().Be(0);
            metadata.PageSize.Should().Be(0);
            metadata.TotalCount.Should().Be(0);
            metadata.TotalPages.Should().Be(0);
            metadata.HasNext.Should().BeFalse();
            metadata.HasPrevious.Should().BeFalse();
        }

        [Fact]
        public void PaginationMetadata_ShouldAcceptValidValues()
        {
            // Act
            var metadata = new PaginationMetadata
            {
                CurrentPage = 3,
                PageSize = 20,
                TotalCount = 150,
                TotalPages = 8,
                HasNext = true,
                HasPrevious = true,
                SortBy = "title",
                SortDirection = "asc",
                NextPage = 4,
                PreviousPage = 2
            };

            // Assert
            metadata.CurrentPage.Should().Be(3);
            metadata.PageSize.Should().Be(20);
            metadata.TotalCount.Should().Be(150);
            metadata.TotalPages.Should().Be(8);
            metadata.HasNext.Should().BeTrue();
            metadata.HasPrevious.Should().BeTrue();
            metadata.SortBy.Should().Be("title");
            metadata.SortDirection.Should().Be("asc");
            metadata.NextPage.Should().Be(4);
            metadata.PreviousPage.Should().Be(2);
        }

        [Fact]
        public void PaginationMetadata_ShouldHandleNullOptionalValues()
        {
            // Act
            var metadata = new PaginationMetadata
            {
                SortBy = null,
                SortDirection = null,
                NextPage = null,
                PreviousPage = null
            };

            // Assert
            metadata.SortBy.Should().BeNull();
            metadata.SortDirection.Should().BeNull();
            metadata.NextPage.Should().BeNull();
            metadata.PreviousPage.Should().BeNull();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void PaginationMetadata_ShouldHandleVariousPageValues(int page)
        {
            // Act
            var metadata = new PaginationMetadata
            {
                CurrentPage = page,
                NextPage = page,
                PreviousPage = page
            };

            // Assert
            metadata.CurrentPage.Should().Be(page);
            metadata.NextPage.Should().Be(page);
            metadata.PreviousPage.Should().Be(page);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void PaginationMetadata_ShouldHandleVariousCountValues(int count)
        {
            // Act
            var metadata = new PaginationMetadata
            {
                PageSize = count,
                TotalCount = count,
                TotalPages = count
            };

            // Assert
            metadata.PageSize.Should().Be(count);
            metadata.TotalCount.Should().Be(count);
            metadata.TotalPages.Should().Be(count);
        }

        [Fact]
        public void PaginationMetadata_ShouldHandleFirstPageScenario()
        {
            // Act
            var metadata = new PaginationMetadata
            {
                CurrentPage = 1,
                TotalPages = 5,
                HasNext = true,
                HasPrevious = false,
                NextPage = 2,
                PreviousPage = null
            };

            // Assert
            metadata.CurrentPage.Should().Be(1);
            metadata.HasNext.Should().BeTrue();
            metadata.HasPrevious.Should().BeFalse();
            metadata.NextPage.Should().Be(2);
            metadata.PreviousPage.Should().BeNull();
        }

        [Fact]
        public void PaginationMetadata_ShouldHandleLastPageScenario()
        {
            // Act
            var metadata = new PaginationMetadata
            {
                CurrentPage = 5,
                TotalPages = 5,
                HasNext = false,
                HasPrevious = true,
                NextPage = null,
                PreviousPage = 4
            };

            // Assert
            metadata.CurrentPage.Should().Be(5);
            metadata.HasNext.Should().BeFalse();
            metadata.HasPrevious.Should().BeTrue();
            metadata.NextPage.Should().BeNull();
            metadata.PreviousPage.Should().Be(4);
        }

        [Fact]
        public void PaginationMetadata_ShouldHandleSinglePageScenario()
        {
            // Act
            var metadata = new PaginationMetadata
            {
                CurrentPage = 1,
                TotalPages = 1,
                HasNext = false,
                HasPrevious = false,
                NextPage = null,
                PreviousPage = null
            };

            // Assert
            metadata.CurrentPage.Should().Be(1);
            metadata.TotalPages.Should().Be(1);
            metadata.HasNext.Should().BeFalse();
            metadata.HasPrevious.Should().BeFalse();
            metadata.NextPage.Should().BeNull();
            metadata.PreviousPage.Should().BeNull();
        }

        #endregion

        #region Edge Cases and Validation Tests

        [Fact]
        public void PaginationRequest_Validation_ShouldFailForInvalidPage()
        {
            // Arrange
            var dto = new PaginationRequest { Page = 0 };
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(dto, context, results, true);

            // Assert
            isValid.Should().BeFalse();
            results.Should().Contain(r => r.MemberNames.Contains("Page"));
        }

        [Fact]
        public void PaginationRequest_Validation_ShouldFailForInvalidPageSize()
        {
            // Arrange
            var dto = new PaginationRequest { PageSize = 0 };
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(dto, context, results, true);

            // Assert
            isValid.Should().BeFalse();
            results.Should().Contain(r => r.MemberNames.Contains("PageSize"));
        }

        [Fact]
        public void PaginationRequest_Validation_ShouldPassForValidValues()
        {
            // Arrange
            var dto = new PaginationRequest { Page = 1, PageSize = 20 };
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(dto, context, results, true);

            // Assert
            isValid.Should().BeTrue();
            results.Should().BeEmpty();
        }

        [Fact]
        public void PaginationDTOs_ShouldHandleMemoryStress()
        {
            // Arrange & Act
            var largePaginationRequests = new List<PaginationRequest>();
            for (int i = 0; i < 1000; i++)
            {
                largePaginationRequests.Add(new PaginationRequest
                {
                    Skills = _fixture.CreateMany<string>(10).ToList(),
                    Roles = _fixture.CreateMany<string>(5).ToList(),
                    SearchTerm = _fixture.Create<string>()
                });
            }

            // Assert - Should not throw OutOfMemoryException
            largePaginationRequests.Should().HaveCount(1000);
        }

        [Fact]
        public void PaginationDTOs_ShouldBeSerializationFriendly()
        {
            // This test ensures DTOs have proper structure for serialization
            // (all properties have getters and setters, no complex constructors)
            
            // Arrange
            var request = new PaginationRequest();
            var response = new PaginatedResponse<string>();
            var metadata = new PaginationMetadata();

            // Act & Assert - Should not throw
            var requestProperties = request.GetType().GetProperties();
            var responseProperties = response.GetType().GetProperties();
            var metadataProperties = metadata.GetType().GetProperties();

            requestProperties.Should().AllSatisfy(p => p.CanRead.Should().BeTrue());
            responseProperties.Should().AllSatisfy(p => p.CanRead.Should().BeTrue());
            metadataProperties.Should().AllSatisfy(p => p.CanRead.Should().BeTrue());
        }

        #endregion
    }
} 