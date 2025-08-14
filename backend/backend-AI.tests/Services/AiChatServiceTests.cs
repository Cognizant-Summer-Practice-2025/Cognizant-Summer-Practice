using System.Net;
using System.Text;
using System.Text.Json;
using backend_AI.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend_AI.tests.Services
{
    public class AiChatServiceTests
    {
        [Fact]
        public async Task GenerateWithPromptAsync_ShouldParseContent()
        {
            // Arrange
            var handler = new FakeHandler((req) =>
            {
                var json = JsonSerializer.Serialize(new
                {
                    id = "gen-1",
                    object_ = "chat.completion",
                    choices = new[] {
                        new { index = 0, finish_reason = "stop", message = new { role = "assistant", content = "ok" } }
                    },
                    usage = new { prompt_tokens = 1, completion_tokens = 1, total_tokens = 2 }
                });
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var http = new HttpClient(handler);
            var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
            var logger = Mock.Of<ILogger<AiChatService>>();
            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient("AIProvider")).Returns(http);
            var svc = new AiChatService(factory.Object, cfg, logger);

            // Act
            var result = await svc.GenerateWithPromptAsync("hello");

            // Assert
            result.Should().Be("ok");
        }

        private sealed class FakeHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
            public FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) => _responder = responder;
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(_responder(request));
        }
    }
}


