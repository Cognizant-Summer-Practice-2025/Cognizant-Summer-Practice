using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using backend_portfolio.DTO.Experience.Request;
using backend_portfolio.DTO.Experience.Response;
using AutoFixture;
using AutoFixture.Kernel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend_portfolio.tests.DTO
{
    public class ExperienceDTOTests
    {
        private readonly Fixture _fixture;

        public ExperienceDTOTests()
        {
            _fixture = CreateFixtureWithDateOnlySupport();
        }

        private static Fixture CreateFixtureWithDateOnlySupport()
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new DateOnlyGenerator());
            return fixture;
        }

        // Custom DateOnly generator for AutoFixture
        private class DateOnlyGenerator : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                if (request is Type type && type == typeof(DateOnly))
                {
                    var random = new Random();
                    var year = random.Next(1900, 2100);
                    var month = random.Next(1, 13);
                    var day = random.Next(1, DateTime.DaysInMonth(year, month) + 1);
                    return new DateOnly(year, month, day);
                }
                return new NoSpecimen();
            }
        }

        #region ExperienceCreateRequest Tests

        [Fact]
        public void ExperienceCreateRequest_ShouldBeInstantiable()
        {
            // Act
            var dto = new ExperienceCreateRequest();

            // Assert
            dto.Should().NotBeNull();
            dto.PortfolioId.Should().Be(Guid.Empty);
            dto.JobTitle.Should().NotBeNull();
            dto.CompanyName.Should().NotBeNull();
            dto.StartDate.Should().Be(default(DateOnly));
            dto.IsCurrent.Should().BeFalse();
        }

        [Fact]
        public void ExperienceCreateRequest_ShouldAcceptValidValues()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var jobTitle = "Senior Software Developer";
            var companyName = "Tech Corp";
            var startDate = new DateOnly(2020, 1, 15);
            var endDate = new DateOnly(2023, 6, 30);
            var description = "Developed web applications";
            var skillsUsed = new[] { "C#", "React", "SQL" };

            // Act
            var dto = new ExperienceCreateRequest
            {
                PortfolioId = portfolioId,
                JobTitle = jobTitle,
                CompanyName = companyName,
                StartDate = startDate,
                EndDate = endDate,
                IsCurrent = false,
                Description = description,
                SkillsUsed = skillsUsed
            };

            // Assert
            dto.PortfolioId.Should().Be(portfolioId);
            dto.JobTitle.Should().Be(jobTitle);
            dto.CompanyName.Should().Be(companyName);
            dto.StartDate.Should().Be(startDate);
            dto.EndDate.Should().Be(endDate);
            dto.IsCurrent.Should().BeFalse();
            dto.Description.Should().Be(description);
            dto.SkillsUsed.Should().BeEquivalentTo(skillsUsed);
        }

        [Fact]
        public void ExperienceCreateRequest_ShouldHandleCurrentPosition()
        {
            // Act
            var dto = new ExperienceCreateRequest
            {
                JobTitle = "Current Developer",
                CompanyName = "Current Company",
                StartDate = new DateOnly(2023, 1, 1),
                EndDate = null,
                IsCurrent = true
            };

            // Assert
            dto.IsCurrent.Should().BeTrue();
            dto.EndDate.Should().BeNull();
        }

        [Fact]
        public void ExperienceCreateRequest_ShouldHandleNullOptionalFields()
        {
            // Act
            var dto = new ExperienceCreateRequest
            {
                EndDate = null,
                Description = null,
                SkillsUsed = null
            };

            // Assert
            dto.EndDate.Should().BeNull();
            dto.Description.Should().BeNull();
            dto.SkillsUsed.Should().BeNull();
        }

        [Fact]
        public void ExperienceCreateRequest_ShouldHandleEmptyStrings()
        {
            // Act
            var dto = new ExperienceCreateRequest
            {
                JobTitle = "",
                CompanyName = "",
                Description = ""
            };

            // Assert
            dto.JobTitle.Should().Be("");
            dto.CompanyName.Should().Be("");
            dto.Description.Should().Be("");
        }

        [Fact]
        public void ExperienceCreateRequest_ShouldHandleEmptySkillsArray()
        {
            // Act
            var dto = new ExperienceCreateRequest
            {
                SkillsUsed = new string[0]
            };

            // Assert
            dto.SkillsUsed.Should().NotBeNull().And.BeEmpty();
        }

        [Theory]
        [InlineData(1900, 1, 1)]
        [InlineData(2000, 12, 31)]
        [InlineData(2100, 6, 15)]
        public void ExperienceCreateRequest_ShouldHandleVariousStartDates(int year, int month, int day)
        {
            // Act
            var startDate = new DateOnly(year, month, day);
            var dto = new ExperienceCreateRequest { StartDate = startDate };

            // Assert
            dto.StartDate.Should().Be(startDate);
        }

        [Theory]
        [InlineData(1900, 1, 1)]
        [InlineData(2000, 12, 31)]
        [InlineData(2100, 6, 15)]
        public void ExperienceCreateRequest_ShouldHandleVariousEndDates(int year, int month, int day)
        {
            // Act
            var endDate = new DateOnly(year, month, day);
            var dto = new ExperienceCreateRequest { EndDate = endDate };

            // Assert
            dto.EndDate.Should().Be(endDate);
        }

        [Fact]
        public void ExperienceCreateRequest_ShouldHandleSpecialCharactersInStrings()
        {
            // Arrange
            var specialJobTitle = "Senior Developer/Engineer & Team Lead (C#/.NET)";
            var specialCompanyName = "Tech Corp. & Associates, Inc.";
            var specialDescription = "Developed applications using C#, .NET & SQL Server. Responsibilities included: coding, testing & deployment.";

            // Act
            var dto = new ExperienceCreateRequest
            {
                JobTitle = specialJobTitle,
                CompanyName = specialCompanyName,
                Description = specialDescription
            };

            // Assert
            dto.JobTitle.Should().Be(specialJobTitle);
            dto.CompanyName.Should().Be(specialCompanyName);
            dto.Description.Should().Be(specialDescription);
        }

        [Fact]
        public void ExperienceCreateRequest_ShouldHandleUnicodeCharacters()
        {
            // Arrange
            var unicodeJobTitle = "高级软件工程师 🚀 シニアエンジニア";
            var unicodeCompanyName = "テック株式会社 (Tech Company)";
            var unicodeDescription = "开发网络应用程序 ウェブアプリケーションの開発";

            // Act
            var dto = new ExperienceCreateRequest
            {
                JobTitle = unicodeJobTitle,
                CompanyName = unicodeCompanyName,
                Description = unicodeDescription
            };

            // Assert
            dto.JobTitle.Should().Be(unicodeJobTitle);
            dto.CompanyName.Should().Be(unicodeCompanyName);
            dto.Description.Should().Be(unicodeDescription);
        }

        [Fact]
        public void ExperienceCreateRequest_ShouldHandleLongStrings()
        {
            // Arrange
            var longJobTitle = new string('a', 5000);
            var longCompanyName = new string('b', 5000);
            var longDescription = new string('c', 50000);

            // Act
            var dto = new ExperienceCreateRequest
            {
                JobTitle = longJobTitle,
                CompanyName = longCompanyName,
                Description = longDescription
            };

            // Assert
            dto.JobTitle.Should().Be(longJobTitle);
            dto.CompanyName.Should().Be(longCompanyName);
            dto.Description.Should().Be(longDescription);
        }

        [Fact]
        public void ExperienceCreateRequest_ShouldHandleLargeSkillsArray()
        {
            // Arrange
            var skills = _fixture.CreateMany<string>(1000).ToArray();

            // Act
            var dto = new ExperienceCreateRequest { SkillsUsed = skills };

            // Assert
            dto.SkillsUsed.Should().HaveCount(1000);
            dto.SkillsUsed.Should().BeEquivalentTo(skills);
        }

        #endregion

        #region ExperienceUpdateRequest Tests

        [Fact]
        public void ExperienceUpdateRequest_ShouldBeInstantiable()
        {
            // Act
            var dto = new ExperienceUpdateRequest();

            // Assert
            dto.Should().NotBeNull();
        }

        [Fact]
        public void ExperienceUpdateRequest_ShouldAcceptValidValues()
        {
            // Arrange
            var jobTitle = "Updated Senior Developer";
            var companyName = "Updated Tech Corp";
            var startDate = new DateOnly(2021, 3, 1);
            var endDate = new DateOnly(2023, 12, 31);
            var description = "Updated responsibilities";
            var skillsUsed = new[] { "TypeScript", "Angular", "PostgreSQL" };

            // Act
            var dto = new ExperienceUpdateRequest
            {
                JobTitle = jobTitle,
                CompanyName = companyName,
                StartDate = startDate,
                EndDate = endDate,
                IsCurrent = false,
                Description = description,
                SkillsUsed = skillsUsed
            };

            // Assert
            dto.JobTitle.Should().Be(jobTitle);
            dto.CompanyName.Should().Be(companyName);
            dto.StartDate.Should().Be(startDate);
            dto.EndDate.Should().Be(endDate);
            dto.IsCurrent.Should().BeFalse();
            dto.Description.Should().Be(description);
            dto.SkillsUsed.Should().BeEquivalentTo(skillsUsed);
        }

        [Fact]
        public void ExperienceUpdateRequest_ShouldHandleAllNullValues()
        {
            // Act
            var dto = new ExperienceUpdateRequest
            {
                JobTitle = null,
                CompanyName = null,
                StartDate = null,
                EndDate = null,
                IsCurrent = null,
                Description = null,
                SkillsUsed = null
            };

            // Assert
            dto.JobTitle.Should().BeNull();
            dto.CompanyName.Should().BeNull();
            dto.StartDate.Should().BeNull();
            dto.EndDate.Should().BeNull();
            dto.IsCurrent.Should().BeNull();
            dto.Description.Should().BeNull();
            dto.SkillsUsed.Should().BeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(null)]
        public void ExperienceUpdateRequest_ShouldHandleNullableIsCurrent(bool? isCurrent)
        {
            // Act
            var dto = new ExperienceUpdateRequest { IsCurrent = isCurrent };

            // Assert
            dto.IsCurrent.Should().Be(isCurrent);
        }

        [Fact]
        public void ExperienceUpdateRequest_ShouldHandlePartialUpdates()
        {
            // Act
            var dto = new ExperienceUpdateRequest
            {
                JobTitle = "New Title",
                // Leave other fields null for partial update
                CompanyName = null,
                Description = null
            };

            // Assert
            dto.JobTitle.Should().Be("New Title");
            dto.CompanyName.Should().BeNull();
            dto.Description.Should().BeNull();
        }

        [Fact]
        public void ExperienceUpdateRequest_ShouldHandleCurrentPositionUpdate()
        {
            // Act
            var dto = new ExperienceUpdateRequest
            {
                IsCurrent = true,
                EndDate = null
            };

            // Assert
            dto.IsCurrent.Should().BeTrue();
            dto.EndDate.Should().BeNull();
        }

        #endregion

        #region ExperienceResponse Tests

        [Fact]
        public void ExperienceResponse_ShouldBeInstantiable()
        {
            // Act
            var dto = new ExperienceResponse();

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(Guid.Empty);
            dto.PortfolioId.Should().Be(Guid.Empty);
            dto.JobTitle.Should().NotBeNull();
            dto.CompanyName.Should().NotBeNull();
            dto.StartDate.Should().Be(default(DateOnly));
            dto.IsCurrent.Should().BeFalse();
            dto.CreatedAt.Should().Be(default(DateTime));
            dto.UpdatedAt.Should().Be(default(DateTime));
        }

        [Fact]
        public void ExperienceResponse_ShouldAcceptValidValues()
        {
            // Arrange
            var id = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var jobTitle = "Software Engineer";
            var companyName = "Test Company";
            var startDate = new DateOnly(2022, 1, 1);
            var endDate = new DateOnly(2023, 12, 31);
            var createdAt = DateTime.UtcNow.AddDays(-10);
            var updatedAt = DateTime.UtcNow;

            // Act
            var dto = new ExperienceResponse
            {
                Id = id,
                PortfolioId = portfolioId,
                JobTitle = jobTitle,
                CompanyName = companyName,
                StartDate = startDate,
                EndDate = endDate,
                IsCurrent = false,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };

            // Assert
            dto.Id.Should().Be(id);
            dto.PortfolioId.Should().Be(portfolioId);
            dto.JobTitle.Should().Be(jobTitle);
            dto.CompanyName.Should().Be(companyName);
            dto.StartDate.Should().Be(startDate);
            dto.EndDate.Should().Be(endDate);
            dto.IsCurrent.Should().BeFalse();
            dto.CreatedAt.Should().Be(createdAt);
            dto.UpdatedAt.Should().Be(updatedAt);
        }

        [Fact]
        public void ExperienceResponse_ShouldHandleNullOptionalFields()
        {
            // Act
            var dto = new ExperienceResponse
            {
                EndDate = null,
                Description = null,
                SkillsUsed = null
            };

            // Assert
            dto.EndDate.Should().BeNull();
            dto.Description.Should().BeNull();
            dto.SkillsUsed.Should().BeNull();
        }

        [Fact]
        public void ExperienceResponse_ShouldHandleDateTimeVariations()
        {
            // Arrange
            var minDate = DateTime.MinValue;
            var maxDate = DateTime.MaxValue;
            var utcNow = DateTime.UtcNow;
            var localNow = DateTime.Now;

            // Act
            var dto1 = new ExperienceResponse { CreatedAt = minDate, UpdatedAt = maxDate };
            var dto2 = new ExperienceResponse { CreatedAt = utcNow, UpdatedAt = localNow };

            // Assert
            dto1.CreatedAt.Should().Be(minDate);
            dto1.UpdatedAt.Should().Be(maxDate);
            dto2.CreatedAt.Should().Be(utcNow);
            dto2.UpdatedAt.Should().Be(localNow);
        }

        [Fact]
        public void ExperienceResponse_ShouldHandleDateOnlyVariations()
        {
            // Arrange
            var minDateOnly = DateOnly.MinValue;
            var maxDateOnly = DateOnly.MaxValue;
            var currentDateOnly = DateOnly.FromDateTime(DateTime.Today);

            // Act
            var dto = new ExperienceResponse
            {
                StartDate = minDateOnly,
                EndDate = maxDateOnly
            };

            var dto2 = new ExperienceResponse
            {
                StartDate = currentDateOnly,
                EndDate = null
            };

            // Assert
            dto.StartDate.Should().Be(minDateOnly);
            dto.EndDate.Should().Be(maxDateOnly);
            dto2.StartDate.Should().Be(currentDateOnly);
            dto2.EndDate.Should().BeNull();
        }

        #endregion

        #region ExperienceSummaryResponse Tests

        [Fact]
        public void ExperienceSummaryResponse_ShouldBeInstantiable()
        {
            // Act
            var dto = new ExperienceSummaryResponse();

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(Guid.Empty);
            dto.JobTitle.Should().NotBeNull();
            dto.CompanyName.Should().NotBeNull();
            dto.StartDate.Should().Be(default(DateOnly));
            dto.IsCurrent.Should().BeFalse();
        }

        [Fact]
        public void ExperienceSummaryResponse_ShouldAcceptValidValues()
        {
            // Arrange
            var id = Guid.NewGuid();
            var jobTitle = "Summary Software Engineer";
            var companyName = "Summary Company";
            var startDate = new DateOnly(2023, 1, 1);
            var endDate = new DateOnly(2023, 12, 31);
            var skillsUsed = new[] { "JavaScript", "Node.js" };

            // Act
            var dto = new ExperienceSummaryResponse
            {
                Id = id,
                JobTitle = jobTitle,
                CompanyName = companyName,
                StartDate = startDate,
                EndDate = endDate,
                IsCurrent = false,
                SkillsUsed = skillsUsed
            };

            // Assert
            dto.Id.Should().Be(id);
            dto.JobTitle.Should().Be(jobTitle);
            dto.CompanyName.Should().Be(companyName);
            dto.StartDate.Should().Be(startDate);
            dto.EndDate.Should().Be(endDate);
            dto.IsCurrent.Should().BeFalse();
            dto.SkillsUsed.Should().BeEquivalentTo(skillsUsed);
        }

        [Fact]
        public void ExperienceSummaryResponse_ShouldHandleNullOptionalFields()
        {
            // Act
            var dto = new ExperienceSummaryResponse
            {
                EndDate = null,
                Description = null,
                SkillsUsed = null
            };

            // Assert
            dto.EndDate.Should().BeNull();
            dto.Description.Should().BeNull();
            dto.SkillsUsed.Should().BeNull();
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public void AllExperienceDTOs_ShouldHandleGuidEmpty()
        {
            // Act
            var createRequest = new ExperienceCreateRequest { PortfolioId = Guid.Empty };
            var response = new ExperienceResponse { Id = Guid.Empty, PortfolioId = Guid.Empty };
            var summaryResponse = new ExperienceSummaryResponse { Id = Guid.Empty };

            // Assert
            createRequest.PortfolioId.Should().Be(Guid.Empty);
            response.Id.Should().Be(Guid.Empty);
            response.PortfolioId.Should().Be(Guid.Empty);
            summaryResponse.Id.Should().Be(Guid.Empty);
        }

        [Fact]
        public void AllExperienceDTOs_ShouldHandleMaxGuidValues()
        {
            // Arrange
            var maxGuid = new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");

            // Act
            var createRequest = new ExperienceCreateRequest { PortfolioId = maxGuid };
            var response = new ExperienceResponse { Id = maxGuid, PortfolioId = maxGuid };

            // Assert
            createRequest.PortfolioId.Should().Be(maxGuid);
            response.Id.Should().Be(maxGuid);
            response.PortfolioId.Should().Be(maxGuid);
        }

        [Fact]
        public void AllExperienceDTOs_ShouldHandleWhitespaceStrings()
        {
            // Arrange
            var whitespace = "   \t\n\r   ";

            // Act
            var createRequest = new ExperienceCreateRequest
            {
                JobTitle = whitespace,
                CompanyName = whitespace,
                Description = whitespace
            };

            var updateRequest = new ExperienceUpdateRequest
            {
                JobTitle = whitespace,
                CompanyName = whitespace,
                Description = whitespace
            };

            // Assert
            createRequest.JobTitle.Should().Be(whitespace);
            createRequest.CompanyName.Should().Be(whitespace);
            createRequest.Description.Should().Be(whitespace);
            updateRequest.JobTitle.Should().Be(whitespace);
            updateRequest.CompanyName.Should().Be(whitespace);
            updateRequest.Description.Should().Be(whitespace);
        }

        [Fact]
        public void AllExperienceDTOs_ShouldHandleDuplicateSkills()
        {
            // Arrange
            var skills = new[] { "C#", "C#", "React", "React", "SQL", "SQL" };

            // Act
            var createRequest = new ExperienceCreateRequest { SkillsUsed = skills };
            var updateRequest = new ExperienceUpdateRequest { SkillsUsed = skills };

            // Assert
            createRequest.SkillsUsed.Should().HaveCount(6);
            createRequest.SkillsUsed.Should().BeEquivalentTo(skills);
            updateRequest.SkillsUsed.Should().HaveCount(6);
            updateRequest.SkillsUsed.Should().BeEquivalentTo(skills);
        }

        [Fact]
        public void AllExperienceDTOs_ShouldHandleComplexSkillNames()
        {
            // Arrange
            var complexSkills = new[]
            {
                "ASP.NET Core",
                "Entity Framework Core",
                "SignalR",
                "Azure DevOps",
                "Docker & Kubernetes",
                "React.js v18+",
                "TypeScript 4.9",
                "SQL Server 2019",
                "MongoDB Atlas",
                "AWS Lambda & API Gateway"
            };

            // Act
            var createRequest = new ExperienceCreateRequest { SkillsUsed = complexSkills };

            // Assert
            createRequest.SkillsUsed.Should().BeEquivalentTo(complexSkills);
        }

        [Fact]
        public void ExperienceDTOs_ShouldHandleDateOnlyEdgeCases()
        {
            // Arrange
            var leapYearDate = new DateOnly(2020, 2, 29);
            var centuryDate = new DateOnly(2000, 1, 1);
            var futureDate = new DateOnly(2050, 12, 31);

            // Act
            var createRequest = new ExperienceCreateRequest
            {
                StartDate = leapYearDate,
                EndDate = futureDate
            };

            var response = new ExperienceResponse
            {
                StartDate = centuryDate,
                EndDate = leapYearDate
            };

            // Assert
            createRequest.StartDate.Should().Be(leapYearDate);
            createRequest.EndDate.Should().Be(futureDate);
            response.StartDate.Should().Be(centuryDate);
            response.EndDate.Should().Be(leapYearDate);
        }

        [Fact]
        public void ExperienceDTOs_ShouldHandleStartDateAfterEndDate()
        {
            // Note: DTOs should accept invalid business logic, validation happens elsewhere
            
            // Arrange
            var startDate = new DateOnly(2023, 12, 31);
            var endDate = new DateOnly(2023, 1, 1); // Earlier than start date

            // Act
            var dto = new ExperienceCreateRequest
            {
                StartDate = startDate,
                EndDate = endDate
            };

            // Assert
            dto.StartDate.Should().Be(startDate);
            dto.EndDate.Should().Be(endDate);
        }

        [Fact]
        public void ExperienceDTOs_ShouldBeSerializationFriendly()
        {
            // This test ensures DTOs have proper structure for serialization
            
            // Arrange
            var createRequest = new ExperienceCreateRequest();
            var updateRequest = new ExperienceUpdateRequest();
            var response = new ExperienceResponse();
            var summaryResponse = new ExperienceSummaryResponse();

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
        public void ExperienceDTOs_ShouldHandleMemoryStress()
        {
            // Arrange & Act
            var experiences = new List<ExperienceResponse>();
            for (int i = 0; i < 1000; i++)
            {
                experiences.Add(new ExperienceResponse
                {
                    Id = Guid.NewGuid(),
                    JobTitle = _fixture.Create<string>(),
                    CompanyName = _fixture.Create<string>(),
                    Description = _fixture.Create<string>(),
                    SkillsUsed = _fixture.CreateMany<string>(10).ToArray(),
                    StartDate = _fixture.Create<DateOnly>()
                });
            }

            // Assert - Should not throw OutOfMemoryException
            experiences.Should().HaveCount(1000);
        }

        [Fact]
        public void ExperienceCreateRequest_ShouldHandleConcurrentAccess()
        {
            // This test verifies thread safety of DTO instantiation
            var tasks = new List<Task<ExperienceCreateRequest>>();

            for (int i = 0; i < 100; i++)
            {
                var index = i; // Capture for closure
                tasks.Add(Task.Run(() => new ExperienceCreateRequest
                {
                    JobTitle = $"Job {index}",
                    CompanyName = $"Company {index}",
                    StartDate = new DateOnly(2020 + (index % 5), 1, 1),
                    SkillsUsed = new[] { "Skill1", "Skill2" }
                }));
            }

            var results = Task.WhenAll(tasks).Result;

            // Assert
            results.Should().HaveCount(100);
            results.Should().AllSatisfy(dto => dto.Should().NotBeNull());
        }

        #endregion
    }
} 