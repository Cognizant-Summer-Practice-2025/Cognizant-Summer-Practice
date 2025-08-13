using System.Net;
using System.Security.Claims;
using System.Text;
using backend_AI.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace backend_AI.tests.Services;

public class UserAuthenticationServiceTests
{
    [Fact]
    public async Task ValidateTokenAsync_ReturnsPrincipal_OnSuccess()
    {
        var handler = new FakeHandler(_ =>
        {
            var json = "{\"userId\":\"u\",\"email\":\"e@e.com\",\"username\":\"u\",\"isAdmin\":true}";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });
        var http = new HttpClient(handler);
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["UserServiceUrl"] = "http://user" }).Build();
        var logger = Mock.Of<ILogger<UserAuthenticationService>>();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("UserService")).Returns(http);
        var svc = new UserAuthenticationService(factory.Object, cfg, logger);

        var principal = await svc.ValidateTokenAsync("tok");
        principal.Should().NotBeNull();
        principal!.Identity!.IsAuthenticated.Should().BeTrue();
        principal.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be("u");
    }

    [Fact]
    public async Task ValidateTokenAsync_ReturnsNull_OnNonSuccess()
    {
        var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var http = new HttpClient(handler);
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var logger = Mock.Of<ILogger<UserAuthenticationService>>();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("UserService")).Returns(http);
        var svc = new UserAuthenticationService(factory.Object, cfg, logger);

        var principal = await svc.ValidateTokenAsync("tok");
        principal.Should().BeNull();
    }

    [Fact]
    public async Task ValidateTokenAsync_ReturnsNull_OnException()
    {
        var handler = new FakeHandler(_ => throw new InvalidOperationException("boom"));
        var http = new HttpClient(handler);
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var logger = Mock.Of<ILogger<UserAuthenticationService>>();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("UserService")).Returns(http);
        var svc = new UserAuthenticationService(factory.Object, cfg, logger);

        var principal = await svc.ValidateTokenAsync("tok");
        principal.Should().BeNull();
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
        public FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) => _responder = responder;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_responder(request));
    }
}


