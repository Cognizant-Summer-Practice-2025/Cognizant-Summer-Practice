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

    private static OAuth2Middleware CreateMiddleware(RequestDelegate next)
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
        var mw = CreateMiddleware(next);
        var security = new backend_AI.Services.SecurityHeadersService();
        var authz = new Mock<IAuthorizationPathService>();
        authz.Setup(a => a.RequiresAuthentication(It.IsAny<HttpContext>())).Returns(false);
        var ctxAuth = new Mock<IAuthenticationContextService>();

        await mw.InvokeAsync(context, security, authz.Object, ctxAuth.Object);

        // Trigger OnStarting callbacks to apply headers
        await context.Response.WriteAsync("");

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task MissingAuthorizationHeader_ShouldReturn401()
    {
        var context = CreateContext("/api/ai/generate");
        RequestDelegate next = _ => Task.CompletedTask;
        var mw = CreateMiddleware(next);
        var security = new Mock<ISecurityHeadersService>();
        var authz = new Mock<IAuthorizationPathService>();
        authz.Setup(a => a.RequiresAuthentication(It.IsAny<HttpContext>())).Returns(true);
        var ctxAuth = new Mock<IAuthenticationContextService>();
        ctxAuth.Setup(s => s.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync((ClaimsPrincipal?)null);

        await mw.InvokeAsync(context, security.Object, authz.Object, ctxAuth.Object);

        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvalidAuthorizationScheme_ShouldReturn401()
    {
        var context = CreateContext("/api/ai/generate", authHeader: "Basic abc");
        RequestDelegate next = _ => Task.CompletedTask;
        var mw = CreateMiddleware(next);
        var security = new Mock<ISecurityHeadersService>();
        var authz = new Mock<IAuthorizationPathService>();
        authz.Setup(a => a.RequiresAuthentication(It.IsAny<HttpContext>())).Returns(true);
        var ctxAuth = new Mock<IAuthenticationContextService>();
        ctxAuth.Setup(s => s.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync((ClaimsPrincipal?)null);

        await mw.InvokeAsync(context, security.Object, authz.Object, ctxAuth.Object);

        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task EmptyBearerToken_ShouldReturn401()
    {
        var context = CreateContext("/api/ai/generate", authHeader: "Bearer   ");
        RequestDelegate next = _ => Task.CompletedTask;
        var mw = CreateMiddleware(next);
        var security = new Mock<ISecurityHeadersService>();
        var authz = new Mock<IAuthorizationPathService>();
        authz.Setup(a => a.RequiresAuthentication(It.IsAny<HttpContext>())).Returns(true);
        var ctxAuth = new Mock<IAuthenticationContextService>();
        ctxAuth.Setup(s => s.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync((ClaimsPrincipal?)null);

        await mw.InvokeAsync(context, security.Object, authz.Object, ctxAuth.Object);

        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvalidToken_ShouldReturn401()
    {
        var context = CreateContext("/api/ai/generate", authHeader: "Bearer token");
        RequestDelegate next = _ => Task.CompletedTask;
        var mw = CreateMiddleware(next);
        var security = new Mock<ISecurityHeadersService>();
        var authz = new Mock<IAuthorizationPathService>();
        authz.Setup(a => a.RequiresAuthentication(It.IsAny<HttpContext>())).Returns(true);
        var ctxAuth = new Mock<IAuthenticationContextService>();
        ctxAuth.Setup(s => s.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync((ClaimsPrincipal?)null);

        await mw.InvokeAsync(context, security.Object, authz.Object, ctxAuth.Object);

        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task ExceptionDuringValidation_ShouldReturn401()
    {
        var context = CreateContext("/api/ai/generate", authHeader: "Bearer token");
        RequestDelegate next = _ => Task.CompletedTask;
        var mw = CreateMiddleware(next);
        var security = new Mock<ISecurityHeadersService>();
        var authz = new Mock<IAuthorizationPathService>();
        authz.Setup(a => a.RequiresAuthentication(It.IsAny<HttpContext>())).Returns(true);
        var ctxAuth = new Mock<IAuthenticationContextService>();
        ctxAuth.Setup(s => s.AuthenticateAsync(It.IsAny<HttpContext>())).ThrowsAsync(new InvalidOperationException("boom"));

        await mw.InvokeAsync(context, security.Object, authz.Object, ctxAuth.Object);

        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task ValidToken_ShouldInvokeNextAndSetUser()
    {
        var context = CreateContext("/api/ai/generate", authHeader: "Bearer token");
        var nextCalled = false;
        RequestDelegate next = ctx => { nextCalled = true; return Task.CompletedTask; };
        var mw = CreateMiddleware(next);
        var security = new Mock<ISecurityHeadersService>();
        var authz = new Mock<IAuthorizationPathService>();
        authz.Setup(a => a.RequiresAuthentication(It.IsAny<HttpContext>())).Returns(true);
        var ctxAuth = new Mock<IAuthenticationContextService>();
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "u") }, "OAuth2");
        ctxAuth.Setup(s => s.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync(new ClaimsPrincipal(identity));

        await mw.InvokeAsync(context, security.Object, authz.Object, ctxAuth.Object);

        nextCalled.Should().BeTrue();
        context.User.Identity?.IsAuthenticated.Should().BeTrue();
        context.User.Identity?.AuthenticationType.Should().Be("OAuth2");
    }
}


