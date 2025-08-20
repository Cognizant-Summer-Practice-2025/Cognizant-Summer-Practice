using System.Net;
using System.Text;
using backend_AI.Services.External;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace backend_AI.tests.Services.External;

public class PortfolioApiClientTests
{
    private static (PortfolioApiClient client, FakeHandler handler) CreateClient(string baseUrl = "http://test")
    {
        var handler = new FakeHandler();
        var http = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PortfolioService:BaseUrl"] = baseUrl }).Build();
        var logger = Mock.Of<ILogger<PortfolioApiClient>>();
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("PortfolioService")).Returns(http);
        // Ensure env var required by GetAllPortfoliosDetailedJsonAsync is set for tests
        Environment.SetEnvironmentVariable("PORTFOLIO_SERVICE_URL", baseUrl);
        var client = new PortfolioApiClient(factory.Object, cfg, logger, accessor);
        return (client, handler);
    }

    [Fact]
    public async Task GetAllPortfoliosBasicJsonAsync_ShouldForwardBearerToken_AndReturnBody()
    {
        var (client, handler) = CreateClient();
        handler.Next = req =>
        {
            req.Headers.Authorization?.Scheme.Should().Be("Bearer");
            req.RequestUri!.AbsolutePath.Should().Be("/api/portfolio");
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]", Encoding.UTF8, "application/json") };
        };
        ((HttpContextAccessor)typeof(PortfolioApiClient).GetField("_httpContextAccessor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(client)!).HttpContext!.Request.Headers["Authorization"] = "Bearer tok";

        var json = await client.GetAllPortfoliosBasicJsonAsync();
        json.Should().Be("[]");
    }

    [Fact]
    public async Task GetAllPortfoliosDetailedJsonAsync_ShouldReturnBody()
    {
        var (client, handler) = CreateClient();
        handler.Next = req =>
        {
            req.RequestUri!.AbsolutePath.Should().Be("/api/portfolio/detailed-all");
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]", Encoding.UTF8, "application/json") };
        };
        var json = await client.GetAllPortfoliosDetailedJsonAsync();
        json.Should().Be("[]");
    }

    [Fact]
    public async Task GetPortfolioByIdAsync_ShouldReturnNull_OnNotFound()
    {
        var (client, handler) = CreateClient();
        handler.Next = _ => new HttpResponseMessage(HttpStatusCode.NotFound);
        var el = await client.GetPortfolioByIdAsync(Guid.NewGuid());
        el.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task GetPortfolioByIdAsync_ShouldReturnNull_OnServerError()
    {
        var (client, handler) = CreateClient();
        handler.Next = _ => new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("boom") };
        var el = await client.GetPortfolioByIdAsync(Guid.NewGuid());
        el.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task GetPortfolioByIdAsync_ShouldReturnElement_OnSuccess()
    {
        var (client, handler) = CreateClient();
        handler.Next = _ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"id\":1}", Encoding.UTF8, "application/json") };
        var el = await client.GetPortfolioByIdAsync(Guid.NewGuid());
        el.HasValue.Should().BeTrue();
        el!.Value.TryGetProperty("id", out var id).Should().BeTrue();
        id.GetInt32().Should().Be(1);
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, HttpResponseMessage>? Next { get; set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(Next!(request));
    }
}


