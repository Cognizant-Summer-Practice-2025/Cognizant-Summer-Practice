using System.Security.Claims;
using backend_AI.Middleware;
using backend_AI.Services.Abstractions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace backend_AI.tests.Middleware;

public class OAuth2MiddlewareTests
{
    private static DefaultHttpContext CreateContext(string path = "/", string method = "GET", string? authHeader = null)
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = path;
        ctx.Request.Method = method;
        if (authHeader != null)
        {
            ctx.Request.Headers["Authorization"] = authHeader;
        }
        return ctx;
    }

    private static OAuth2Middleware CreateMiddleware(RequestDelegate next, Mock<IUserAuthenticationService> mockAuth)
    {
        var logger = Mock.Of<ILogger<OAuth2Middleware>>();
        return new OAuth2Middleware(next, logger);
    }

    [Fact]
    public async Task PublicEndpoints_ShouldBypassAuth()
    {
        var context = CreateContext("/health");
        var nextCalled = false;
        RequestDelegate next = ctx => { nextCalled = true; return Task.CompletedTask; };
        var mockAuth = new Mock<IUserAuthenticationService>(MockBehavior.Strict);
        var mw = CreateMiddleware(next, mockAuth);

        await mw.InvokeAsync(context, mockAuth.Object);

        nextCalled.Should().BeTrue();
        context.Response.Headers["X-Content-Type-Options"].ToString().Should().Be("nosniff");
        context.Response.Headers["Cross-Origin-Resource-Policy"].ToString().Should().Be("same-origin");
    }

    [Fact]
    public async Task MissingAuthorizationHeader_ShouldReturn401()
    {
        var context = CreateContext("/api/ai/generate");
        RequestDelegate next = _ => Task.CompletedTask;
        var mockAuth = new Mock<IUserAuthenticationService>(MockBehavior.Strict);
        var mw = CreateMiddleware(next, mockAuth);

        await mw.InvokeAsync(context, mockAuth.Object);

        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvalidAuthorizationScheme_ShouldReturn401()
    {
        var context = CreateContext("/api/ai/generate", authHeader: "Basic abc");
        RequestDelegate next = _ => Task.CompletedTask;
        var mockAuth = new Mock<IUserAuthenticationService>(MockBehavior.Strict);
        var mw = CreateMiddleware(next, mockAuth);

        await mw.InvokeAsync(context, mockAuth.Object);

        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task EmptyBearerToken_ShouldReturn401()
    {
        var context = CreateContext("/api/ai/generate", authHeader: "Bearer   ");
        RequestDelegate next = _ => Task.CompletedTask;
        var mockAuth = new Mock<IUserAuthenticationService>(MockBehavior.Strict);
        var mw = CreateMiddleware(next, mockAuth);

        await mw.InvokeAsync(context, mockAuth.Object);

        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvalidToken_ShouldReturn401()
    {
        var context = CreateContext("/api/ai/generate", authHeader: "Bearer token");
        RequestDelegate next = _ => Task.CompletedTask;
        var mockAuth = new Mock<IUserAuthenticationService>();
        mockAuth.Setup(s => s.ValidateTokenAsync("token")).ReturnsAsync((ClaimsPrincipal?)null);
        var mw = CreateMiddleware(next, mockAuth);

        await mw.InvokeAsync(context, mockAuth.Object);

        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task ExceptionDuringValidation_ShouldReturn401()
    {
        var context = CreateContext("/api/ai/generate", authHeader: "Bearer token");
        RequestDelegate next = _ => Task.CompletedTask;
        var mockAuth = new Mock<IUserAuthenticationService>();
        mockAuth.Setup(s => s.ValidateTokenAsync("token")).ThrowsAsync(new InvalidOperationException("boom"));
        var mw = CreateMiddleware(next, mockAuth);

        await mw.InvokeAsync(context, mockAuth.Object);

        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task ValidToken_ShouldInvokeNextAndSetUser()
    {
        var context = CreateContext("/api/ai/generate", authHeader: "Bearer token");
        var nextCalled = false;
        RequestDelegate next = ctx => { nextCalled = true; return Task.CompletedTask; };
        var mockAuth = new Mock<IUserAuthenticationService>();
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "u") }, "OAuth2");
        mockAuth.Setup(s => s.ValidateTokenAsync("token")).ReturnsAsync(new ClaimsPrincipal(identity));
        var mw = CreateMiddleware(next, mockAuth);

        await mw.InvokeAsync(context, mockAuth.Object);

        nextCalled.Should().BeTrue();
        context.User.Identity?.IsAuthenticated.Should().BeTrue();
        context.User.Identity?.AuthenticationType.Should().Be("OAuth2");
    }
}


