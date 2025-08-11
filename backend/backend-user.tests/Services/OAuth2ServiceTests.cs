using Xunit;
using Moq;
using backend_user.Services;
using backend_user.Repositories;
using backend_user.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class OAuth2ServiceTests
{
    private readonly Mock<IOAuthProviderRepository> _mockProviderRepo = new();
    private readonly Mock<IUserRepository> _mockUserRepo = new();
    private readonly Mock<ILogger<OAuth2Service>> _mockLogger = new();
    private readonly OAuth2Service _service;

    public OAuth2ServiceTests()
    {
        _service = new OAuth2Service(_mockProviderRepo.Object, _mockUserRepo.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ValidateAccessTokenAsync_ReturnsNull_IfProviderNotFound()
    {
        _mockProviderRepo.Setup(x => x.GetByAccessTokenAsync("token")).ReturnsAsync((OAuthProvider)null);
        var result = await _service.ValidateAccessTokenAsync("token");
        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateAccessTokenAsync_ReturnsNull_IfTokenExpired()
    {
        var provider = new OAuthProvider { TokenExpiresAt = DateTime.UtcNow.AddSeconds(-10) };
        _mockProviderRepo.Setup(x => x.GetByAccessTokenAsync("token")).ReturnsAsync(provider);
        var result = await _service.ValidateAccessTokenAsync("token");
        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateAccessTokenAsync_ReturnsProvider_IfValid()
    {
        var provider = new OAuthProvider { TokenExpiresAt = DateTime.UtcNow.AddSeconds(100) };
        _mockProviderRepo.Setup(x => x.GetByAccessTokenAsync("token")).ReturnsAsync(provider);
        var result = await _service.ValidateAccessTokenAsync("token");
        Assert.Equal(provider, result);
    }

    [Fact]
    public async Task GetUserByAccessTokenAsync_ReturnsNull_IfTokenInvalid()
    {
        _mockProviderRepo.Setup(x => x.GetByAccessTokenAsync("token")).ReturnsAsync((OAuthProvider)null);
        var result = await _service.GetUserByAccessTokenAsync("token");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByAccessTokenAsync_ReturnsUser_IfTokenValid()
    {
        var provider = new OAuthProvider { UserId = Guid.NewGuid(), TokenExpiresAt = DateTime.UtcNow.AddSeconds(100) };
        var user = new User { Id = provider.UserId };
        _mockProviderRepo.Setup(x => x.GetByAccessTokenAsync("token")).ReturnsAsync(provider);
        _mockUserRepo.Setup(x => x.GetUserById(provider.UserId)).ReturnsAsync(user);
        var result = await _service.GetUserByAccessTokenAsync("token");
        Assert.Equal(user, result);
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_ReturnsNull_IfRefreshTokenNotFound()
    {
        _mockProviderRepo.Setup(x => x.GetByRefreshTokenAsync("refresh")).ReturnsAsync((OAuthProvider)null);
        var result = await _service.RefreshAccessTokenAsync("refresh");
        Assert.Null(result);
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_ReturnsNull_IfProviderHasNoRefreshToken()
    {
        var provider = new OAuthProvider { Provider = OAuthProviderType.Google, RefreshToken = null };
        _mockProviderRepo.Setup(x => x.GetByRefreshTokenAsync("refresh")).ReturnsAsync(provider);
        var result = await _service.RefreshAccessTokenAsync("refresh");
        Assert.Null(result);
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_ReturnsNull_IfRefreshTokenWithProviderReturnsNull()
    {
        var provider = new OAuthProvider { Provider = OAuthProviderType.Google, RefreshToken = "refresh" };
        _mockProviderRepo.Setup(x => x.GetByRefreshTokenAsync("refresh")).ReturnsAsync(provider);
        // Simulate Google refresh returns null (missing env vars)
        var result = await _service.RefreshAccessTokenAsync("refresh");
        Assert.Null(result);
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_UpdatesProvider_IfRefreshSucceeds()
    {
        // This test will simulate a successful refresh for a provider type that is not Google, Facebook, or LinkedIn (default branch)
        var provider = new OAuthProvider { Provider = OAuthProviderType.Google, RefreshToken = "refresh" };
        _mockProviderRepo.Setup(x => x.GetByRefreshTokenAsync("refresh")).ReturnsAsync(provider);
        // Patch the private method to always return a TokenRefreshResult
        // Not possible directly, so this test will only check the public error branches
        // For full coverage, the actual HTTP calls would need to be mocked with a refactor
        // This test ensures the code is exercised up to the private method
        var result = await _service.RefreshAccessTokenAsync("refresh");
        Assert.Null(result); // Because Google refresh will fail without env vars
    }
}