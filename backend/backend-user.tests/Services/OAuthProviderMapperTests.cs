using Xunit;
using backend_user.Services.Mappers;
using backend_user.DTO.OAuthProvider.Request;
using backend_user.DTO.OAuthProvider.Response;
using backend_user.DTO.Authentication.Request;
using backend_user.DTO.User.Request;
using backend_user.Models;
using System;

public class OAuthProviderMapperTests
{
    [Fact]
    public void ToResponseDto_NullProvider_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => OAuthProviderMapper.ToResponseDto(null));
    }

    [Fact]
    public void ToResponseDto_ValidProvider_MapsCorrectly()
    {
        var provider = new OAuthProvider { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Provider = OAuthProviderType.Google, ProviderId = "pid", ProviderEmail = "a@b.com", AccessToken = "token", RefreshToken = "refresh", TokenExpiresAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var dto = OAuthProviderMapper.ToResponseDto(provider);
        Assert.Equal(provider.Id, dto.Id);
        Assert.Equal(provider.UserId, dto.UserId);
        Assert.Equal(provider.Provider, dto.Provider);
        Assert.Equal(provider.ProviderId, dto.ProviderId);
        Assert.Equal(provider.ProviderEmail, dto.ProviderEmail);
        Assert.Equal(provider.AccessToken, dto.AccessToken);
        Assert.Equal(provider.RefreshToken, dto.RefreshToken);
        Assert.Equal(provider.TokenExpiresAt, dto.TokenExpiresAt);
        Assert.Equal(provider.CreatedAt, dto.CreatedAt);
        Assert.Equal(provider.UpdatedAt, dto.UpdatedAt);
    }

    [Fact]
    public void ToSummaryDto_NullProvider_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => OAuthProviderMapper.ToSummaryDto(null));
    }

    [Fact]
    public void ToSummaryDto_ValidProvider_MapsCorrectly()
    {
        var provider = new OAuthProvider { Id = Guid.NewGuid(), Provider = OAuthProviderType.Google, ProviderEmail = "a@b.com", CreatedAt = DateTime.UtcNow };
        var dto = OAuthProviderMapper.ToSummaryDto(provider);
        Assert.Equal(provider.Id, dto.Id);
        Assert.Equal(provider.Provider, dto.Provider);
        Assert.Equal(provider.ProviderEmail, dto.ProviderEmail);
        Assert.Equal(provider.CreatedAt, dto.CreatedAt);
    }

    [Fact]
    public void ToCreateRequest_NullRequest_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => OAuthProviderMapper.ToCreateRequest(null, Guid.NewGuid()));
    }

    [Fact]
    public void ToCreateRequest_ValidRequest_MapsCorrectly()
    {
        var req = new RegisterOAuthUserRequest { Provider = OAuthProviderType.Google, ProviderId = "pid", ProviderEmail = "a@b.com", AccessToken = "token", RefreshToken = "refresh", TokenExpiresAt = DateTime.UtcNow };
        var userId = Guid.NewGuid();
        var dto = OAuthProviderMapper.ToCreateRequest(req, userId);
        Assert.Equal(userId, dto.UserId);
        Assert.Equal(req.Provider, dto.Provider);
        Assert.Equal(req.ProviderId, dto.ProviderId);
        Assert.Equal(req.ProviderEmail, dto.ProviderEmail);
        Assert.Equal(req.AccessToken, dto.AccessToken);
        Assert.Equal(req.RefreshToken, dto.RefreshToken);
        Assert.Equal(req.TokenExpiresAt, dto.TokenExpiresAt);
    }

    [Fact]
    public void ToUpdateRequest_NullRequest_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => OAuthProviderMapper.ToUpdateRequest(null));
    }

    [Fact]
    public void ToUpdateRequest_ValidRequest_MapsCorrectly()
    {
        var req = new OAuthLoginRequestDto { AccessToken = "token", RefreshToken = "refresh", TokenExpiresAt = DateTime.UtcNow };
        var dto = OAuthProviderMapper.ToUpdateRequest(req);
        Assert.Equal(req.AccessToken, dto.AccessToken);
        Assert.Equal(req.RefreshToken, dto.RefreshToken);
        Assert.Equal(req.TokenExpiresAt, dto.TokenExpiresAt);
    }
}