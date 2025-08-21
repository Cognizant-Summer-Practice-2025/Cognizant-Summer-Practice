using System.Text.Json;
using backend_AI.Controllers;
using backend_AI.Services.Abstractions;
using backend_AI.Services.External;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend_AI.tests.Controllers
{
    public class AiControllerTests
    {
        private readonly Mock<IAiChatService> _mockChat;
        private readonly Mock<IPortfolioApiClient> _mockPortfolioApi;
        private readonly Mock<IPortfolioRankingService> _mockRanking;
        private readonly Mock<ILogger<AiController>> _mockLogger;
        private readonly AiController _controller;

        public AiControllerTests()
        {
            _mockChat = new Mock<IAiChatService>();
            _mockPortfolioApi = new Mock<IPortfolioApiClient>();
            _mockRanking = new Mock<IPortfolioRankingService>();
            _mockLogger = new Mock<ILogger<AiController>>();
            _controller = new AiController(_mockChat.Object, _mockPortfolioApi.Object, _mockRanking.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Generate_ShouldReturnOk_WithServiceText()
        {
            _mockChat.Setup(s => s.GenerateAsync(default)).ReturnsAsync("hello");

            var result = await _controller.Generate(default);

            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok!.Value.Should().BeEquivalentTo(new { response = "hello" });
        }

        [Fact]
        public async Task Generate_ShouldReturn502_OnException()
        {
            _mockChat.Setup(s => s.GenerateAsync(default)).ThrowsAsync(new InvalidOperationException("boom"));

            var result = await _controller.Generate(default);

            result.Should().BeOfType<ObjectResult>();
            var obj = result as ObjectResult;
            obj!.StatusCode.Should().Be(502);
        }

        [Fact]
        public async Task GenerateBestPortfolio_ShouldReturnArrayOfBasicPortfolios_WhenModelReturnsIds()
        {
            // Arrange: basic portfolios list with enough entries (10+) to satisfy business logic
            var portfolioIds = new List<string>();
            var portfolioJson = new List<string>();
            var rankedCandidates = new List<backend_AI.Services.Abstractions.PortfolioCandidate>();
            
            // Create 12 mock portfolios to exceed the minimum requirement of 10
            for (int i = 1; i <= 12; i++)
            {
                var id = $"{i:D8}-1111-1111-1111-111111111111";
                portfolioIds.Add(id);
                portfolioJson.Add($"{{ \"id\": \"{id}\", \"title\": \"Portfolio {i}\" }}");
                rankedCandidates.Add(new(id, "user", 1, 1, 1, 1, 1, 5));
            }
            
            var basic = $"[ {string.Join(", ", portfolioJson)} ]";
            _mockPortfolioApi.Setup(p => p.GetAllPortfoliosDetailedJsonAsync(default)).ReturnsAsync(basic);
            _mockPortfolioApi.Setup(p => p.GetAllPortfoliosBasicJsonAsync(default)).ReturnsAsync(basic);

            // Ranking selects top items (12 candidates, more than the minimum 10)
            _mockRanking.Setup(r => r.SelectTopCandidates(It.IsAny<string>(), It.IsAny<int>())).Returns(rankedCandidates);

            // Model returns first 2 ids
            _mockChat.Setup(s => s.GenerateWithPromptAsync(It.IsAny<string>(), default))
                     .ReturnsAsync($"{portfolioIds[0]},{portfolioIds[1]}");

            // Act
            var result = await _controller.GenerateBestPortfolio(default);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GenerateBestPortfolio_ShouldReturnBadRequest_WhenInsufficientPortfolios()
        {
            var basic = "[ { \"id\": \"11111111-1111-1111-1111-111111111111\", \"title\": \"A\" } ]";
            _mockPortfolioApi.Setup(p => p.GetAllPortfoliosDetailedJsonAsync(default)).ReturnsAsync(basic);
            _mockPortfolioApi.Setup(p => p.GetAllPortfoliosBasicJsonAsync(default)).ReturnsAsync(basic);
            // Return insufficient candidates (less than 10)
            _mockRanking.Setup(r => r.SelectTopCandidates(It.IsAny<string>(), It.IsAny<int>())).Returns(new List<backend_AI.Services.Abstractions.PortfolioCandidate>());
            _mockChat.Setup(s => s.GenerateWithPromptAsync(It.IsAny<string>(), default)).ReturnsAsync("");

            var result = await _controller.GenerateBestPortfolio(default);

            // Should return BadRequest due to insufficient portfolio data
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            badRequest!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GenerateBestPortfolio_ShouldReturnBadRequest_WhenModelReturnsNoValidIds()
        {
            // Arrange: Create enough portfolios to pass the initial check
            var portfolioIds = new List<string>();
            var portfolioJson = new List<string>();
            var rankedCandidates = new List<backend_AI.Services.Abstractions.PortfolioCandidate>();
            
            // Create 12 mock portfolios to exceed the minimum requirement of 10
            for (int i = 1; i <= 12; i++)
            {
                var id = $"{i:D8}-1111-1111-1111-111111111111";
                portfolioIds.Add(id);
                portfolioJson.Add($"{{ \"id\": \"{id}\", \"title\": \"Portfolio {i}\" }}");
                rankedCandidates.Add(new(id, "user", 1, 1, 1, 1, 1, 5));
            }
            
            var basic = $"[ {string.Join(", ", portfolioJson)} ]";
            _mockPortfolioApi.Setup(p => p.GetAllPortfoliosDetailedJsonAsync(default)).ReturnsAsync(basic);
            _mockPortfolioApi.Setup(p => p.GetAllPortfoliosBasicJsonAsync(default)).ReturnsAsync(basic);
            _mockRanking.Setup(r => r.SelectTopCandidates(It.IsAny<string>(), It.IsAny<int>())).Returns(rankedCandidates);
            
            // Model returns invalid/non-matching IDs
            _mockChat.Setup(s => s.GenerateWithPromptAsync(It.IsAny<string>(), default))
                     .ReturnsAsync("invalid-id-1,invalid-id-2,not-a-guid");

            var result = await _controller.GenerateBestPortfolio(default);

            // Should return BadRequest because no valid portfolios could be matched
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            badRequest!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GenerateBestPortfolio_ShouldReturn502_OnException()
        {
            _mockPortfolioApi.Setup(p => p.GetAllPortfoliosDetailedJsonAsync(default)).ThrowsAsync(new InvalidOperationException("fail"));
            var result = await _controller.GenerateBestPortfolio(default);
            result.Should().BeOfType<ObjectResult>();
            var obj = (ObjectResult)result;
            obj.StatusCode.Should().Be(502);
        }
    }
}


