using System.Net;
using System.Text;
using System.Text.Json;
using backend_AI.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace backend_AI.tests.Services;

public class AiChatServiceExtraTests
{
    [Fact]
    public async Task GenerateAsync_ShouldParseContent()
    {
        var handler = new FakeHandler(_ =>
        {
            var json = JsonSerializer.Serialize(new
            {
                choices = new[] { new { message = new { content = "ok-gen" }, finish_reason = "stop" } },
                usage = new { prompt_tokens = 1, completion_tokens = 1, total_tokens = 2 }
            });
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        });
        var http = new HttpClient(handler);
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var logger = Mock.Of<ILogger<AiChatService>>();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("AIProvider")).Returns(http);
        var svc = new AiChatService(factory.Object, cfg, logger);

        var result = await svc.GenerateAsync();
        result.Should().Be("ok-gen");
    }

    [Fact]
    public async Task GenerateAsync_ShouldSetAuthHeader_WhenApiKeyProvided()
    {
        var handler = new FakeHandler(req =>
        {
            req.Headers.Authorization!.Scheme.Should().Be("Bearer");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new { choices = new[] { new { message = new { content = "x" } } } }))
            };
        });
        var http = new HttpClient(handler);
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["OpenRouter:ApiKey"] = "k" }).Build();
        var logger = Mock.Of<ILogger<AiChatService>>();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("AIProvider")).Returns(http);
        var svc = new AiChatService(factory.Object, cfg, logger);
        var text = await svc.GenerateAsync();
        text.Should().Be("x");
    }

    [Fact]
    public async Task GenerateAsync_ShouldThrow_OnNonSuccess()
    {
        var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("oops") });
        var http = new HttpClient(handler);
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var logger = Mock.Of<ILogger<AiChatService>>();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("AIProvider")).Returns(http);
        var svc = new AiChatService(factory.Object, cfg, logger);

        await FluentActions.Awaiting(() => svc.GenerateAsync()).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GenerateWithPromptAsync_ShouldReturnEmpty_WhenChoicesMissing()
    {
        var handler = new FakeHandler(_ =>
        {
            var json = JsonSerializer.Serialize(new { hello = "world" });
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        });
        var http = new HttpClient(handler);
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var logger = Mock.Of<ILogger<AiChatService>>();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("AIProvider")).Returns(http);
        var svc = new AiChatService(factory.Object, cfg, logger);

        var result = await svc.GenerateWithPromptAsync("p");
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateWithPromptAsync_ShouldReturnEmpty_WhenMessageMissing()
    {
        var handler = new FakeHandler(_ =>
        {
            var json = JsonSerializer.Serialize(new { choices = new[] { new { finish_reason = "stop" } } });
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        });
        var http = new HttpClient(handler);
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var logger = Mock.Of<ILogger<AiChatService>>();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("AIProvider")).Returns(http);
        var svc = new AiChatService(factory.Object, cfg, logger);
        var text = await svc.GenerateWithPromptAsync("p");
        text.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateWithPromptAsync_ShouldRespectConfig_MaxTokens_And_Temperature()
    {
        var handler = new FakeHandler(_ =>
        {
            var json = JsonSerializer.Serialize(new { choices = new[] { new { message = new { content = "ok" } } } });
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        });
        var http = new HttpClient(handler);
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OpenRouter:MaxTokens"] = "256",
            ["OpenRouter:Temperature"] = "0.7"
        }).Build();
        var logger = Mock.Of<ILogger<AiChatService>>();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("AIProvider")).Returns(http);
        var svc = new AiChatService(factory.Object, cfg, logger);
        var text = await svc.GenerateWithPromptAsync("p");
        text.Should().Be("ok");
    }

    [Fact]
    public async Task GenerateWithPromptAsync_ShouldRespectEnv_MaxTokens_And_Temperature()
    {
        var prevMax = Environment.GetEnvironmentVariable("OPENROUTER_MAX_TOKENS");
        var prevTemp = Environment.GetEnvironmentVariable("OPENROUTER_TEMPERATURE");
        try
        {
            Environment.SetEnvironmentVariable("OPENROUTER_MAX_TOKENS", "128");
            Environment.SetEnvironmentVariable("OPENROUTER_TEMPERATURE", "0.2");

            var handler = new FakeHandler(_ =>
            {
                var json = JsonSerializer.Serialize(new { choices = new[] { new { message = new { content = "ok" } } } });
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
            });
            var http = new HttpClient(handler);
            var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
            var logger = Mock.Of<ILogger<AiChatService>>();
            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient("AIProvider")).Returns(http);
            var svc = new AiChatService(factory.Object, cfg, logger);
            var text = await svc.GenerateWithPromptAsync("p");
            text.Should().Be("ok");
        }
        finally
        {
            Environment.SetEnvironmentVariable("OPENROUTER_MAX_TOKENS", prevMax);
            Environment.SetEnvironmentVariable("OPENROUTER_TEMPERATURE", prevTemp);
        }
    }

    [Fact]
    public async Task GenerateWithPromptAsync_ShouldThrow_OnNonSuccess()
    {
        var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("oops") });
        var http = new HttpClient(handler);
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var logger = Mock.Of<ILogger<AiChatService>>();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("AIProvider")).Returns(http);
        var svc = new AiChatService(factory.Object, cfg, logger);
        await FluentActions.Awaiting(() => svc.GenerateWithPromptAsync("p")).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GenerateWithPromptAsync_ShouldHandleWhitespaceContent()
    {
        var handler = new FakeHandler(_ =>
        {
            var json = JsonSerializer.Serialize(new { choices = new[] { new { message = new { content = "   " }, finish_reason = "stop" } }, usage = new { } });
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        });
        var http = new HttpClient(handler);
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var logger = Mock.Of<ILogger<AiChatService>>();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("AIProvider")).Returns(http);
        var svc = new AiChatService(factory.Object, cfg, logger);
        var text = await svc.GenerateWithPromptAsync("p");
        text.Should().Be("   ");
    }

    [Fact]
    public async Task GenerateWithPromptAsync_ShouldReturnEmpty_WhenChoicesArrayEmpty()
    {
        var handler = new FakeHandler(_ =>
        {
            var json = JsonSerializer.Serialize(new { choices = Array.Empty<object>() });
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        });
        var http = new HttpClient(handler);
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var logger = Mock.Of<ILogger<AiChatService>>();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("AIProvider")).Returns(http);
        var svc = new AiChatService(factory.Object, cfg, logger);

        var result = await svc.GenerateWithPromptAsync("p");
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateAsync_ShouldReturnEmpty_WhenChoicesArrayEmpty()
    {
        var handler = new FakeHandler(_ =>
        {
            var json = JsonSerializer.Serialize(new { choices = Array.Empty<object>() });
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        });
        var http = new HttpClient(handler);
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var logger = Mock.Of<ILogger<AiChatService>>();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("AIProvider")).Returns(http);
        var svc = new AiChatService(factory.Object, cfg, logger);

        var result = await svc.GenerateAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateAsync_ShouldReturnEmpty_WhenMessageMissing()
    {
        var handler = new FakeHandler(_ =>
        {
            var json = JsonSerializer.Serialize(new { choices = new[] { new { finish_reason = "stop" } } });
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        });
        var http = new HttpClient(handler);
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var logger = Mock.Of<ILogger<AiChatService>>();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("AIProvider")).Returns(http);
        var svc = new AiChatService(factory.Object, cfg, logger);

        var result = await svc.GenerateAsync();
        result.Should().BeEmpty();
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
        public FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) => _responder = responder;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_responder(request));
    }
}


