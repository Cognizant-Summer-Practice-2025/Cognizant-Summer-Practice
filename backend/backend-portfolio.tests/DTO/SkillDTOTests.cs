using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using backend_portfolio.DTO.Skill.Request;
using backend_portfolio.DTO.Skill.Response;
using AutoFixture;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend_portfolio.tests.DTO
{
    public class SkillDTOTests
    {
        private readonly Fixture _fixture;

        public SkillDTOTests()
        {
            _fixture = new Fixture();
        }

        #region SkillCreateRequest Tests

        [Fact]
        public void SkillCreateRequest_ShouldBeInstantiable()
        {
            // Act
            var dto = new SkillCreateRequest();

            // Assert
            dto.Should().NotBeNull();
            dto.PortfolioId.Should().Be(Guid.Empty);
            dto.Name.Should().NotBeNull();
        }

        [Fact]
        public void SkillCreateRequest_ShouldAcceptValidValues()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var name = "C#";
            var categoryType = "Programming Language";
            var subcategory = "Backend";
            var category = "Development";
            var proficiencyLevel = 5;
            var displayOrder = 1;

            // Act
            var dto = new SkillCreateRequest
            {
                PortfolioId = portfolioId,
                Name = name,
                CategoryType = categoryType,
                Subcategory = subcategory,
                Category = category,
                ProficiencyLevel = proficiencyLevel,
                DisplayOrder = displayOrder
            };

            // Assert
            dto.PortfolioId.Should().Be(portfolioId);
            dto.Name.Should().Be(name);
            dto.CategoryType.Should().Be(categoryType);
            dto.Subcategory.Should().Be(subcategory);
            dto.Category.Should().Be(category);
            dto.ProficiencyLevel.Should().Be(proficiencyLevel);
            dto.DisplayOrder.Should().Be(displayOrder);
        }

        [Fact]
        public void SkillCreateRequest_ShouldHandleNullOptionalFields()
        {
            // Act
            var dto = new SkillCreateRequest
            {
                CategoryType = null,
                Subcategory = null,
                Category = null,
                ProficiencyLevel = null,
                DisplayOrder = null
            };

            // Assert
            dto.CategoryType.Should().BeNull();
            dto.Subcategory.Should().BeNull();
            dto.Category.Should().BeNull();
            dto.ProficiencyLevel.Should().BeNull();
            dto.DisplayOrder.Should().BeNull();
        }

        [Fact]
        public void SkillCreateRequest_ShouldHandleEmptyStrings()
        {
            // Act
            var dto = new SkillCreateRequest
            {
                Name = "",
                CategoryType = "",
                Subcategory = "",
                Category = ""
            };

            // Assert
            dto.Name.Should().Be("");
            dto.CategoryType.Should().Be("");
            dto.Subcategory.Should().Be("");
            dto.Category.Should().Be("");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(-1)]
        [InlineData(100)]
        public void SkillCreateRequest_ShouldHandleVariousProficiencyLevels(int proficiencyLevel)
        {
            // Act
            var dto = new SkillCreateRequest { ProficiencyLevel = proficiencyLevel };

            // Assert
            dto.ProficiencyLevel.Should().Be(proficiencyLevel);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(50)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void SkillCreateRequest_ShouldHandleVariousDisplayOrders(int displayOrder)
        {
            // Act
            var dto = new SkillCreateRequest { DisplayOrder = displayOrder };

            // Assert
            dto.DisplayOrder.Should().Be(displayOrder);
        }

        [Theory]
        [InlineData("C#")]
        [InlineData("ASP.NET Core")]
        [InlineData("React.js")]
        [InlineData("Node.js")]
        [InlineData("Vue.js")]
        [InlineData("TypeScript")]
        [InlineData("Entity Framework Core")]
        [InlineData("SQL Server")]
        [InlineData("MongoDB")]
        [InlineData("Docker & Kubernetes")]
        public void SkillCreateRequest_ShouldHandleCommonSkillNames(string skillName)
        {
            // Act
            var dto = new SkillCreateRequest { Name = skillName };

            // Assert
            dto.Name.Should().Be(skillName);
        }

        [Fact]
        public void SkillCreateRequest_ShouldHandleSpecialCharactersInName()
        {
            // Arrange
            var specialName = "C++/C# & .NET Framework (v4.8+)";

            // Act
            var dto = new SkillCreateRequest { Name = specialName };

            // Assert
            dto.Name.Should().Be(specialName);
        }

        [Fact]
        public void SkillCreateRequest_ShouldHandleUnicodeCharacters()
        {
            // Arrange
            var unicodeName = "程序设计 🚀 プログラミング";
            var unicodeCategory = "开发技能 デベロップメント";

            // Act
            var dto = new SkillCreateRequest
            {
                Name = unicodeName,
                Category = unicodeCategory
            };

            // Assert
            dto.Name.Should().Be(unicodeName);
            dto.Category.Should().Be(unicodeCategory);
        }

        [Fact]
        public void SkillCreateRequest_ShouldHandleLongStrings()
        {
            // Arrange
            var longName = new string('a', 5000);
            var longCategory = new string('b', 5000);

            // Act
            var dto = new SkillCreateRequest
            {
                Name = longName,
                Category = longCategory
            };

            // Assert
            dto.Name.Should().Be(longName);
            dto.Category.Should().Be(longCategory);
        }

        [Theory]
        [InlineData("Frontend")]
        [InlineData("Backend")]
        [InlineData("Full Stack")]
        [InlineData("DevOps")]
        [InlineData("Database")]
        [InlineData("Mobile")]
        [InlineData("Testing")]
        [InlineData("Design")]
        public void SkillCreateRequest_ShouldHandleCommonCategories(string category)
        {
            // Act
            var dto = new SkillCreateRequest { Category = category };

            // Assert
            dto.Category.Should().Be(category);
        }

        #endregion

        #region SkillUpdateRequest Tests

        [Fact]
        public void SkillUpdateRequest_ShouldBeInstantiable()
        {
            // Act
            var dto = new SkillUpdateRequest();

            // Assert
            dto.Should().NotBeNull();
        }

        [Fact]
        public void SkillUpdateRequest_ShouldAcceptValidValues()
        {
            // Arrange
            var name = "Updated C#";
            var categoryType = "Updated Programming Language";
            var proficiencyLevel = 8;
            var displayOrder = 2;

            // Act
            var dto = new SkillUpdateRequest
            {
                Name = name,
                CategoryType = categoryType,
                ProficiencyLevel = proficiencyLevel,
                DisplayOrder = displayOrder
            };

            // Assert
            dto.Name.Should().Be(name);
            dto.CategoryType.Should().Be(categoryType);
            dto.ProficiencyLevel.Should().Be(proficiencyLevel);
            dto.DisplayOrder.Should().Be(displayOrder);
        }

        [Fact]
        public void SkillUpdateRequest_ShouldHandleAllNullValues()
        {
            // Act
            var dto = new SkillUpdateRequest
            {
                Name = null,
                CategoryType = null,
                Subcategory = null,
                Category = null,
                ProficiencyLevel = null,
                DisplayOrder = null
            };

            // Assert
            dto.Name.Should().BeNull();
            dto.CategoryType.Should().BeNull();
            dto.Subcategory.Should().BeNull();
            dto.Category.Should().BeNull();
            dto.ProficiencyLevel.Should().BeNull();
            dto.DisplayOrder.Should().BeNull();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(null)]
        public void SkillUpdateRequest_ShouldHandleNullableProficiencyLevel(int? proficiencyLevel)
        {
            // Act
            var dto = new SkillUpdateRequest { ProficiencyLevel = proficiencyLevel };

            // Assert
            dto.ProficiencyLevel.Should().Be(proficiencyLevel);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(-1)]
        [InlineData(null)]
        public void SkillUpdateRequest_ShouldHandleNullableDisplayOrder(int? displayOrder)
        {
            // Act
            var dto = new SkillUpdateRequest { DisplayOrder = displayOrder };

            // Assert
            dto.DisplayOrder.Should().Be(displayOrder);
        }

        [Fact]
        public void SkillUpdateRequest_ShouldHandlePartialUpdates()
        {
            // Act
            var dto = new SkillUpdateRequest
            {
                Name = "New Skill Name",
                // Leave other fields null for partial update
                CategoryType = null,
                ProficiencyLevel = null
            };

            // Assert
            dto.Name.Should().Be("New Skill Name");
            dto.CategoryType.Should().BeNull();
            dto.ProficiencyLevel.Should().BeNull();
        }

        #endregion

        #region SkillResponse Tests

        [Fact]
        public void SkillResponse_ShouldBeInstantiable()
        {
            // Act
            var dto = new SkillResponse();

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(Guid.Empty);
            dto.PortfolioId.Should().Be(Guid.Empty);
            dto.Name.Should().NotBeNull();
            dto.CreatedAt.Should().Be(default(DateTime));
            dto.UpdatedAt.Should().Be(default(DateTime));
        }

        [Fact]
        public void SkillResponse_ShouldAcceptValidValues()
        {
            // Arrange
            var id = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var name = "React";
            var categoryType = "Frontend Framework";
            var proficiencyLevel = 9;
            var createdAt = DateTime.UtcNow.AddDays(-5);
            var updatedAt = DateTime.UtcNow;

            // Act
            var dto = new SkillResponse
            {
                Id = id,
                PortfolioId = portfolioId,
                Name = name,
                CategoryType = categoryType,
                ProficiencyLevel = proficiencyLevel,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };

            // Assert
            dto.Id.Should().Be(id);
            dto.PortfolioId.Should().Be(portfolioId);
            dto.Name.Should().Be(name);
            dto.CategoryType.Should().Be(categoryType);
            dto.ProficiencyLevel.Should().Be(proficiencyLevel);
            dto.CreatedAt.Should().Be(createdAt);
            dto.UpdatedAt.Should().Be(updatedAt);
        }

        [Fact]
        public void SkillResponse_ShouldHandleNullOptionalFields()
        {
            // Act
            var dto = new SkillResponse
            {
                CategoryType = null,
                Subcategory = null,
                Category = null,
                ProficiencyLevel = null,
                DisplayOrder = null
            };

            // Assert
            dto.CategoryType.Should().BeNull();
            dto.Subcategory.Should().BeNull();
            dto.Category.Should().BeNull();
            dto.ProficiencyLevel.Should().BeNull();
            dto.DisplayOrder.Should().BeNull();
        }

        [Fact]
        public void SkillResponse_ShouldHandleDateTimeVariations()
        {
            // Arrange
            var minDate = DateTime.MinValue;
            var maxDate = DateTime.MaxValue;
            var utcNow = DateTime.UtcNow;
            var localNow = DateTime.Now;

            // Act
            var dto1 = new SkillResponse { CreatedAt = minDate, UpdatedAt = maxDate };
            var dto2 = new SkillResponse { CreatedAt = utcNow, UpdatedAt = localNow };

            // Assert
            dto1.CreatedAt.Should().Be(minDate);
            dto1.UpdatedAt.Should().Be(maxDate);
            dto2.CreatedAt.Should().Be(utcNow);
            dto2.UpdatedAt.Should().Be(localNow);
        }

        #endregion

        #region SkillSummaryResponse Tests

        [Fact]
        public void SkillSummaryResponse_ShouldBeInstantiable()
        {
            // Act
            var dto = new SkillSummaryResponse();

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(Guid.Empty);
            dto.Name.Should().NotBeNull();
        }

        [Fact]
        public void SkillSummaryResponse_ShouldAcceptValidValues()
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "Summary Skill";
            var categoryType = "Summary Category";
            var proficiencyLevel = 7;
            var displayOrder = 3;

            // Act
            var dto = new SkillSummaryResponse
            {
                Id = id,
                Name = name,
                CategoryType = categoryType,
                ProficiencyLevel = proficiencyLevel,
                DisplayOrder = displayOrder
            };

            // Assert
            dto.Id.Should().Be(id);
            dto.Name.Should().Be(name);
            dto.CategoryType.Should().Be(categoryType);
            dto.ProficiencyLevel.Should().Be(proficiencyLevel);
            dto.DisplayOrder.Should().Be(displayOrder);
        }

        [Fact]
        public void SkillSummaryResponse_ShouldHandleNullOptionalFields()
        {
            // Act
            var dto = new SkillSummaryResponse
            {
                CategoryType = null,
                Subcategory = null,
                Category = null,
                ProficiencyLevel = null,
                DisplayOrder = null
            };

            // Assert
            dto.CategoryType.Should().BeNull();
            dto.Subcategory.Should().BeNull();
            dto.Category.Should().BeNull();
            dto.ProficiencyLevel.Should().BeNull();
            dto.DisplayOrder.Should().BeNull();
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public void AllSkillDTOs_ShouldHandleGuidEmpty()
        {
            // Act
            var createRequest = new SkillCreateRequest { PortfolioId = Guid.Empty };
            var response = new SkillResponse { Id = Guid.Empty, PortfolioId = Guid.Empty };
            var summaryResponse = new SkillSummaryResponse { Id = Guid.Empty };

            // Assert
            createRequest.PortfolioId.Should().Be(Guid.Empty);
            response.Id.Should().Be(Guid.Empty);
            response.PortfolioId.Should().Be(Guid.Empty);
            summaryResponse.Id.Should().Be(Guid.Empty);
        }

        [Fact]
        public void AllSkillDTOs_ShouldHandleMaxGuidValues()
        {
            // Arrange
            var maxGuid = new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");

            // Act
            var createRequest = new SkillCreateRequest { PortfolioId = maxGuid };
            var response = new SkillResponse { Id = maxGuid, PortfolioId = maxGuid };

            // Assert
            createRequest.PortfolioId.Should().Be(maxGuid);
            response.Id.Should().Be(maxGuid);
            response.PortfolioId.Should().Be(maxGuid);
        }

        [Fact]
        public void AllSkillDTOs_ShouldHandleWhitespaceStrings()
        {
            // Arrange
            var whitespace = "   \t\n\r   ";

            // Act
            var createRequest = new SkillCreateRequest
            {
                Name = whitespace,
                Category = whitespace,
                CategoryType = whitespace
            };

            var updateRequest = new SkillUpdateRequest
            {
                Name = whitespace,
                Category = whitespace,
                CategoryType = whitespace
            };

            // Assert
            createRequest.Name.Should().Be(whitespace);
            createRequest.Category.Should().Be(whitespace);
            createRequest.CategoryType.Should().Be(whitespace);
            updateRequest.Name.Should().Be(whitespace);
            updateRequest.Category.Should().Be(whitespace);
            updateRequest.CategoryType.Should().Be(whitespace);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1000)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(1000)]
        [InlineData(int.MaxValue)]
        public void AllSkillDTOs_ShouldHandleExtremeProficiencyValues(int proficiencyLevel)
        {
            // Act
            var createRequest = new SkillCreateRequest { ProficiencyLevel = proficiencyLevel };
            var updateRequest = new SkillUpdateRequest { ProficiencyLevel = proficiencyLevel };
            var response = new SkillResponse { ProficiencyLevel = proficiencyLevel };

            // Assert
            createRequest.ProficiencyLevel.Should().Be(proficiencyLevel);
            updateRequest.ProficiencyLevel.Should().Be(proficiencyLevel);
            response.ProficiencyLevel.Should().Be(proficiencyLevel);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1000)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(1000)]
        [InlineData(int.MaxValue)]
        public void AllSkillDTOs_ShouldHandleExtremeDisplayOrderValues(int displayOrder)
        {
            // Act
            var createRequest = new SkillCreateRequest { DisplayOrder = displayOrder };
            var updateRequest = new SkillUpdateRequest { DisplayOrder = displayOrder };
            var response = new SkillResponse { DisplayOrder = displayOrder };

            // Assert
            createRequest.DisplayOrder.Should().Be(displayOrder);
            updateRequest.DisplayOrder.Should().Be(displayOrder);
            response.DisplayOrder.Should().Be(displayOrder);
        }

        [Fact]
        public void AllSkillDTOs_ShouldHandleComplexHierarchicalCategories()
        {
            // Arrange
            var complexCategoryType = "Programming Languages";
            var complexSubcategory = "Object-Oriented Languages";
            var complexCategory = "Backend Development > Server-Side Languages > Compiled Languages";

            // Act
            var createRequest = new SkillCreateRequest
            {
                CategoryType = complexCategoryType,
                Subcategory = complexSubcategory,
                Category = complexCategory
            };

            var updateRequest = new SkillUpdateRequest
            {
                CategoryType = complexCategoryType,
                Subcategory = complexSubcategory,
                Category = complexCategory
            };

            // Assert
            createRequest.CategoryType.Should().Be(complexCategoryType);
            createRequest.Subcategory.Should().Be(complexSubcategory);
            createRequest.Category.Should().Be(complexCategory);
            updateRequest.CategoryType.Should().Be(complexCategoryType);
            updateRequest.Subcategory.Should().Be(complexSubcategory);
            updateRequest.Category.Should().Be(complexCategory);
        }

        [Fact]
        public void SkillDTOs_ShouldBeSerializationFriendly()
        {
            // This test ensures DTOs have proper structure for serialization
            
            // Arrange
            var createRequest = new SkillCreateRequest();
            var updateRequest = new SkillUpdateRequest();
            var response = new SkillResponse();
            var summaryResponse = new SkillSummaryResponse();

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
        public void SkillDTOs_ShouldHandleMemoryStress()
        {
            // Arrange & Act
            var skills = new List<SkillResponse>();
            for (int i = 0; i < 1000; i++)
            {
                skills.Add(new SkillResponse
                {
                    Id = Guid.NewGuid(),
                    Name = _fixture.Create<string>(),
                    CategoryType = _fixture.Create<string>(),
                    Category = _fixture.Create<string>(),
                    ProficiencyLevel = _fixture.Create<int>(),
                    DisplayOrder = i
                });
            }

            // Assert - Should not throw OutOfMemoryException
            skills.Should().HaveCount(1000);
        }

        [Fact]
        public void SkillCreateRequest_ShouldHandleConcurrentAccess()
        {
            // This test verifies thread safety of DTO instantiation
            var tasks = new List<Task<SkillCreateRequest>>();

            for (int i = 0; i < 100; i++)
            {
                var index = i; // Capture for closure
                tasks.Add(Task.Run(() => new SkillCreateRequest
                {
                    Name = $"Skill {index}",
                    CategoryType = $"Category {index}",
                    ProficiencyLevel = index % 10,
                    DisplayOrder = index
                }));
            }

            var results = Task.WhenAll(tasks).Result;

            // Assert
            results.Should().HaveCount(100);
            results.Should().AllSatisfy(dto => dto.Should().NotBeNull());
        }

        [Fact]
        public void SkillDTOs_ShouldHandleSkillNameVariations()
        {
            // Arrange - Test various real-world skill naming patterns
            var skillNames = new[]
            {
                "C#", "C++", "F#",
                "ASP.NET", "ASP.NET Core", ".NET Framework", ".NET 6+",
                "React.js", "Vue.js", "Angular.js", "Node.js",
                "TypeScript", "JavaScript ES6+",
                "Entity Framework", "Entity Framework Core",
                "SQL Server", "MySQL", "PostgreSQL", "MongoDB",
                "Docker", "Kubernetes", "Docker Compose",
                "AWS", "Azure", "Google Cloud Platform",
                "Git", "GitHub", "GitLab", "Azure DevOps",
                "Agile/Scrum", "Test-Driven Development (TDD)",
                "RESTful APIs", "GraphQL",
                "Microservices Architecture",
                "CI/CD", "Jenkins", "GitHub Actions"
            };

            // Act & Assert
            foreach (var skillName in skillNames)
            {
                var dto = new SkillCreateRequest { Name = skillName };
                dto.Name.Should().Be(skillName);
            }
        }

        #endregion
    }
} 