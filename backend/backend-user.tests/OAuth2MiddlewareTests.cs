using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using backend_user.Middleware;
using backend_user.Services.Abstractions;
using backend_user.Models;
using System.Security.Claims;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;

public class OAuth2MiddlewareTests
{
    private readonly Mock<IOAuth2Service> _mockOAuth2Service = new();
    private readonly DefaultHttpContext _context = new();
    private bool _nextCalled;
    private RequestDelegate _next;

    public OAuth2MiddlewareTests()
    {
        _nextCalled = false;
        _next = ctx => { _nextCalled = true; return Task.CompletedTask; };
    }

    private OAuth2Middleware CreateMiddleware() => new OAuth2Middleware(_next);

    [Fact]
    public async Task SkipsAuth_ForPublicPaths()
    {
        var middleware = CreateMiddleware();
        _context.Request.Path = "/api/users/login";
        await middleware.InvokeAsync(_context, _mockOAuth2Service.Object);
        _nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Returns401_IfMissingAuthorizationHeader()
    {
        var middleware = CreateMiddleware();
        _context.Request.Path = "/api/secure";
        _context.Request.Headers.Remove("Authorization");
        var responseStream = new MemoryStream();
        _context.Response.Body = responseStream;
        await middleware.InvokeAsync(_context, _mockOAuth2Service.Object);
        _context.Response.StatusCode.Should().Be(401);
        responseStream.Position = 0;
        var body = new StreamReader(responseStream).ReadToEnd();
        body.Should().Contain("Missing or invalid Authorization header");
    }

    [Fact]
    public async Task Returns401_IfAuthorizationHeaderNotBearer()
    {
        var middleware = CreateMiddleware();
        _context.Request.Path = "/api/secure";
        _context.Request.Headers["Authorization"] = "Basic xyz";
        var responseStream = new MemoryStream();
        _context.Response.Body = responseStream;
        await middleware.InvokeAsync(_context, _mockOAuth2Service.Object);
        _context.Response.StatusCode.Should().Be(401);
        responseStream.Position = 0;
        var body = new StreamReader(responseStream).ReadToEnd();
        body.Should().Contain("Missing or invalid Authorization header");
    }

    [Fact]
    public async Task Returns401_IfTokenIsEmpty()
    {
        var middleware = CreateMiddleware();
        _context.Request.Path = "/api/secure";
        _context.Request.Headers["Authorization"] = "Bearer ";
        var responseStream = new MemoryStream();
        _context.Response.Body = responseStream;
        await middleware.InvokeAsync(_context, _mockOAuth2Service.Object);
        _context.Response.StatusCode.Should().Be(401);
        responseStream.Position = 0;
        var body = new StreamReader(responseStream).ReadToEnd();
        body.Should().Contain("Empty access token");
    }

    [Fact]
    public async Task Returns401_IfTokenIsInvalidOrExpired()
    {
        var middleware = CreateMiddleware();
        _context.Request.Path = "/api/secure";
        _context.Request.Headers["Authorization"] = "Bearer invalid";
        _mockOAuth2Service.Setup(x => x.GetUserByAccessTokenAsync("invalid")).ReturnsAsync((User)null);
        var responseStream = new MemoryStream();
        _context.Response.Body = responseStream;
        await middleware.InvokeAsync(_context, _mockOAuth2Service.Object);
        _context.Response.StatusCode.Should().Be(401);
        responseStream.Position = 0;
        var body = new StreamReader(responseStream).ReadToEnd();
        body.Should().Contain("Invalid or expired access token");
    }

    [Fact]
    public async Task SetsUserClaims_AndCallsNext_IfTokenIsValid()
    {
        var middleware = CreateMiddleware();
        _context.Request.Path = "/api/secure";
        _context.Request.Headers["Authorization"] = "Bearer validtoken";
        var user = new User { Id = Guid.NewGuid(), Email = "a@b.com", Username = "user", IsAdmin = true };
        _mockOAuth2Service.Setup(x => x.GetUserByAccessTokenAsync("validtoken")).ReturnsAsync(user);
        await middleware.InvokeAsync(_context, _mockOAuth2Service.Object);
        _nextCalled.Should().BeTrue();
        _context.User.Identity.Should().NotBeNull();
        _context.User.Identity.AuthenticationType.Should().Be("OAuth2");
        _context.User.FindFirst(ClaimTypes.NameIdentifier).Value.Should().Be(user.Id.ToString());
        _context.User.FindFirst(ClaimTypes.Email).Value.Should().Be(user.Email);
        _context.User.FindFirst(ClaimTypes.Name).Value.Should().Be(user.Username);
        _context.User.FindFirst("IsAdmin").Value.Should().Be("True");
    }
}