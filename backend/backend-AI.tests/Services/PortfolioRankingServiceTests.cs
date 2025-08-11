using System.Text.Json;
using System.Linq;
using backend_AI.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace backend_AI.tests.Services;

public class PortfolioRankingServiceTests
{
    private static PortfolioRankingService CreateService()
    {
        var logger = Mock.Of<ILogger<PortfolioRankingService>>();
        return new PortfolioRankingService(logger);
    }

    [Fact]
    public void SelectTopCandidates_ShouldReturnEmpty_OnInvalidJson()
    {
        var svc = CreateService();
        var result = svc.SelectTopCandidates("{not-json}");
        result.Should().BeEmpty();
    }

    [Fact]
    public void SelectTopCandidates_ComputesScores_AndOrders()
    {
        var data = new object[]
        {
            new
            {
                id = "11111111-1111-1111-1111-111111111111",
                userId = "u1",
                experience = new[] { new { startDate = "2020-01-01", endDate = "2021-01-01" } },
                skills = new[] {
                    new { proficiencyLevel = 3, category = "backend" },
                    new { proficiencyLevel = 4, category = "frontend" }
                },
                blogPosts = new[] { new { isPublished = true, excerpt = new string('x', 120), content = "", tags = new[] { "t" } } },
                bio = "Software developer. Loves code.",
                projects = new[] { new { description = new string('d', 100), demoUrl = "https://demo", githubUrl = "https://git", technologies = new[] { "a", "b", "c" } } }
            },
            new
            {
                id = "22222222-2222-2222-2222-222222222222",
                userId = "u2",
                experience = Array.Empty<object>(),
                skills = Array.Empty<object>(),
                blogPosts = Array.Empty<object>(),
                bio = "junior",
                projects = Array.Empty<object>()
            }
        };
        var json = JsonSerializer.Serialize(data);
        var svc = CreateService();
        var result = svc.SelectTopCandidates(json, 2);
        result.Should().HaveCount(2);
        result.First().Id.Should().Be("11111111-1111-1111-1111-111111111111");
        result.Last().Id.Should().Be("22222222-2222-2222-2222-222222222222");
        result.First().TotalScore.Should().BeGreaterThan(result.Last().TotalScore);
    }

    [Fact]
    public void SelectTopCandidates_ShouldCoverBranches_ForDiverseInputs()
    {
        var data = new object[]
        {
            // First user with two portfolios: one lower, one higher to exercise keep/replace logic
            new
            {
                id = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
                userId = "u1",
                experience = new[]
                {
                    new { startDate = "2022-01-01", endDate = "2021-01-01" }, // negative months -> clamp to 0
                    new { startDate = (string?)null, endDate = (string?)null } // ignored
                },
                skills = new[]
                {
                    new { proficiencyLevel = 5, category = "backend" },
                    new { proficiencyLevel = 2, category = "backend" }, // duplicate category
                    new { proficiencyLevel = 4, category = "devops" }
                },
                blogPosts = new[] { new { isPublished = false, excerpt = "short", content = "", tags = Array.Empty<string>() } }, // not relevant
                bio = "lorem lorem lorem", // placeholder true, short -> negative score path
                projects = new[]
                {
                    new { description = new string('d', 100), demoUrl = "https://demo", githubUrl = "", technologies = new[] { "a", "b", "c", "d", "e", "f" } }, // many techs -> min(5)
                    new { description = "", demoUrl = (string?)null, githubUrl = (string?)null, technologies = Array.Empty<string>() }
                }
            },
            new
            {
                id = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
                userId = "u1",
                experience = Array.Empty<object>(),
                skills = Array.Empty<object>(),
                blogPosts = Array.Empty<object>(),
                bio = "", // no bio branch
                projects = Array.Empty<object>()
            },
            // Second user single portfolio
            new
            {
                id = "cccccccc-cccc-cccc-cccc-cccccccccccc",
                userId = "u2",
                experience = new[] { new { startDate = "2020-01-01", endDate = "2022-01-01" } },
                skills = new[] { new { proficiencyLevel = 3, category = "frontend" } },
                blogPosts = new[] { new { isPublished = true, excerpt = new string('x', 200), content = "", tags = new[] { "tag" } } }, // relevant
                bio = "Senior developer. Works on React.", // has keywords
                projects = new[] { new { description = new string('y', 90), demoUrl = "https://demo", githubUrl = "https://git", technologies = new[] { "a", "b" } } }
            }
        };
        var json = JsonSerializer.Serialize(data);
        var svc = CreateService();
        var result = svc.SelectTopCandidates(json, 3);
        result.Should().HaveCount(2); // best per user
        result.Any(c => c.Id == "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa").Should().BeTrue();
        result.Any(c => c.Id == "cccccccc-cccc-cccc-cccc-cccccccccccc").Should().BeTrue();
    }
}


