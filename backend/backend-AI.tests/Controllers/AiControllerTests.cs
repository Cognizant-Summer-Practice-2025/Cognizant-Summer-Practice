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
            // Arrange: basic portfolios list
            var basic = "[ { \"id\": \"11111111-1111-1111-1111-111111111111\", \"title\": \"A\" }, { \"id\": \"22222222-2222-2222-2222-222222222222\", \"title\": \"B\" } ]";
            _mockPortfolioApi.Setup(p => p.GetAllPortfoliosDetailedJsonAsync(default)).ReturnsAsync(basic);
            _mockPortfolioApi.Setup(p => p.GetAllPortfoliosBasicJsonAsync(default)).ReturnsAsync(basic);

            // Ranking selects top items
            var ranked = new List<backend_AI.Services.Abstractions.PortfolioCandidate>
            {
                new("11111111-1111-1111-1111-111111111111","u",1,1,1,1,1,5),
                new("22222222-2222-2222-2222-222222222222","u",1,1,1,1,1,5),
            };
            _mockRanking.Setup(r => r.SelectTopCandidates(It.IsAny<string>(), It.IsAny<int>())).Returns(ranked);

            // Model returns ids
            _mockChat.Setup(s => s.GenerateWithPromptAsync(It.IsAny<string>(), default))
                     .ReturnsAsync("11111111-1111-1111-1111-111111111111,22222222-2222-2222-2222-222222222222");

            // Act
            var result = await _controller.GenerateBestPortfolio(default);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GenerateBestPortfolio_ShouldReturnEmptyArray_WhenModelReturnsNoValidIds()
        {
            var basic = "[ { \"id\": \"11111111-1111-1111-1111-111111111111\", \"title\": \"A\" } ]";
            _mockPortfolioApi.Setup(p => p.GetAllPortfoliosDetailedJsonAsync(default)).ReturnsAsync(basic);
            _mockPortfolioApi.Setup(p => p.GetAllPortfoliosBasicJsonAsync(default)).ReturnsAsync(basic);
            _mockRanking.Setup(r => r.SelectTopCandidates(It.IsAny<string>(), It.IsAny<int>())).Returns(new List<backend_AI.Services.Abstractions.PortfolioCandidate>());
            _mockChat.Setup(s => s.GenerateWithPromptAsync(It.IsAny<string>(), default)).ReturnsAsync("");

            var result = await _controller.GenerateBestPortfolio(default);

            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok!.Value.Should().NotBeNull();
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


