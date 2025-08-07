using Xunit;
using backend_user.Services.Validators;
using backend_user.DTO.OAuthProvider.Request;
using backend_user.DTO.Authentication.Request;
using backend_user.Models;
using System;
using System.Collections.Generic;

public class OAuthValidatorTests
{
    [Fact]
    public void ValidateLoginRequest_NullRequest_ReturnsFailure()
    {
        var result = OAuthValidator.ValidateLoginRequest(null);
        Assert.False(result.IsValid);
        Assert.Contains("Request cannot be null", result.Errors);
    }

    [Fact]
    public void ValidateLoginRequest_AllFieldsMissing_ReturnsAllErrors()
    {
        var dto = new OAuthLoginRequestDto();
        var result = OAuthValidator.ValidateLoginRequest(dto);
        Assert.False(result.IsValid);
        Assert.Contains("Provider ID is required", result.Errors);
        Assert.Contains("Provider email is required", result.Errors);
        Assert.Contains("Access token is required", result.Errors);
    }

    [Fact]
    public void ValidateLoginRequest_InvalidEmail_ReturnsEmailFormatError()
    {
        var dto = new OAuthLoginRequestDto { ProviderId = "id", ProviderEmail = "bademail", AccessToken = "token" };
        var result = OAuthValidator.ValidateLoginRequest(dto);
        Assert.False(result.IsValid);
        Assert.Contains("Provider email format is invalid", result.Errors);
    }

    [Fact]
    public void ValidateLoginRequest_ValidRequest_ReturnsSuccess()
    {
        var dto = new OAuthLoginRequestDto { ProviderId = "id", ProviderEmail = "a@b.com", AccessToken = "token" };
        var result = OAuthValidator.ValidateLoginRequest(dto);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateCreateRequest_NullRequest_ReturnsFailure()
    {
        var result = OAuthValidator.ValidateCreateRequest(null);
        Assert.False(result.IsValid);
        Assert.Contains("Request cannot be null", result.Errors);
    }

    [Fact]
    public void ValidateCreateRequest_AllFieldsMissing_ReturnsAllErrors()
    {
        var dto = new OAuthProviderCreateRequestDto();
        var result = OAuthValidator.ValidateCreateRequest(dto);
        Assert.False(result.IsValid);
        Assert.Contains("User ID is required", result.Errors);
        Assert.Contains("Provider ID is required", result.Errors);
        Assert.Contains("Provider email is required", result.Errors);
        Assert.Contains("Access token is required", result.Errors);
    }

    [Fact]
    public void ValidateCreateRequest_InvalidEmail_ReturnsEmailFormatError()
    {
        var dto = new OAuthProviderCreateRequestDto { UserId = Guid.NewGuid(), ProviderId = "id", ProviderEmail = "bademail", AccessToken = "token" };
        var result = OAuthValidator.ValidateCreateRequest(dto);
        Assert.False(result.IsValid);
        Assert.Contains("Provider email format is invalid", result.Errors);
    }

    [Fact]
    public void ValidateCreateRequest_ValidRequest_ReturnsSuccess()
    {
        var dto = new OAuthProviderCreateRequestDto { UserId = Guid.NewGuid(), ProviderId = "id", ProviderEmail = "a@b.com", AccessToken = "token" };
        var result = OAuthValidator.ValidateCreateRequest(dto);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateUpdateRequest_NullRequest_ReturnsFailure()
    {
        var result = OAuthValidator.ValidateUpdateRequest(null);
        Assert.False(result.IsValid);
        Assert.Contains("Request cannot be null", result.Errors);
    }

    [Fact]
    public void ValidateUpdateRequest_MissingAccessToken_ReturnsError()
    {
        var dto = new OAuthProviderUpdateRequestDto();
        var result = OAuthValidator.ValidateUpdateRequest(dto);
        Assert.False(result.IsValid);
        Assert.Contains("Access token is required", result.Errors);
    }

    [Fact]
    public void ValidateUpdateRequest_ValidRequest_ReturnsSuccess()
    {
        var dto = new OAuthProviderUpdateRequestDto { AccessToken = "token" };
        var result = OAuthValidator.ValidateUpdateRequest(dto);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateProviderCredentials_AllFieldsMissing_ReturnsAllErrors()
    {
        var result = OAuthValidator.ValidateProviderCredentials(OAuthProviderType.Google, null, null);
        Assert.False(result.IsValid);
        Assert.Contains("Provider ID is required", result.Errors);
        Assert.Contains("Provider email is required", result.Errors);
    }

    [Fact]
    public void ValidateProviderCredentials_InvalidEmail_ReturnsEmailFormatError()
    {
        var result = OAuthValidator.ValidateProviderCredentials(OAuthProviderType.Google, "id", "bademail");
        Assert.False(result.IsValid);
        Assert.Contains("Provider email format is invalid", result.Errors);
    }

    [Fact]
    public void ValidateProviderCredentials_Valid_ReturnsSuccess()
    {
        var result = OAuthValidator.ValidateProviderCredentials(OAuthProviderType.Google, "id", "a@b.com");
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateOAuthProviderId_EmptyGuid_ReturnsFailure()
    {
        var result = OAuthValidator.ValidateOAuthProviderId(Guid.Empty);
        Assert.False(result.IsValid);
        Assert.Contains("OAuth provider ID cannot be empty", result.Errors);
    }

    [Fact]
    public void ValidateOAuthProviderId_ValidGuid_ReturnsSuccess()
    {
        var result = OAuthValidator.ValidateOAuthProviderId(Guid.NewGuid());
        Assert.True(result.IsValid);
    }
}