using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using backend_user.Repositories;
using backend_user.Models;
using backend_user.Data;
using backend_user.DTO.OAuthProvider.Request;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

public class OAuthProviderRepositoryTests : IDisposable
{
    private readonly UserDbContext _context;
    private readonly OAuthProviderRepository _repository;

    public OAuthProviderRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new UserDbContext(options);
        _repository = new OAuthProviderRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CreateAsync_AddsProvider()
    {
        var request = new OAuthProviderCreateRequestDto
        {
            UserId = Guid.NewGuid(),
            Provider = OAuthProviderType.Google,
            ProviderId = "pid",
            ProviderEmail = "a@b.com",
            AccessToken = "token",
            RefreshToken = "refresh",
            TokenExpiresAt = DateTime.UtcNow.AddHours(1)
        };
        var result = await _repository.CreateAsync(request);
        result.Id.Should().NotBe(Guid.Empty);
        result.Provider.Should().Be(OAuthProviderType.Google);
        (await _context.OAuthProviders.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsProviderOrNull()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var provider = new OAuthProvider { Id = Guid.NewGuid(), UserId = userId, Provider = OAuthProviderType.Google, ProviderId = "pid", ProviderEmail = "a@b.com", AccessToken = "token", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _context.Users.Add(user);
        _context.OAuthProviders.Add(provider);
        await _context.SaveChangesAsync();
        var found = await _repository.GetByIdAsync(provider.Id);
        found.Should().NotBeNull();
        (await _repository.GetByIdAsync(Guid.NewGuid())).Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsProviders()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _context.Users.Add(user);
        _context.OAuthProviders.Add(new OAuthProvider { Id = Guid.NewGuid(), UserId = userId, Provider = OAuthProviderType.Google, ProviderId = "pid", ProviderEmail = "a@b.com", AccessToken = "token", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();
        var list = (await _repository.GetByUserIdAsync(userId)).ToList();
        list.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByUserIdAndProviderAsync_ReturnsProviderOrNull()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var provider = new OAuthProvider { Id = Guid.NewGuid(), UserId = userId, Provider = OAuthProviderType.Google, ProviderId = "pid" };
        _context.Users.Add(user);
        _context.OAuthProviders.Add(provider);
        await _context.SaveChangesAsync();
        var found = await _repository.GetByUserIdAndProviderAsync(userId, OAuthProviderType.Google);
        found.Should().NotBeNull();
        (await _repository.GetByUserIdAndProviderAsync(userId, OAuthProviderType.Facebook)).Should().BeNull();
    }

    [Fact]
    public async Task GetByProviderAndProviderIdAsync_ReturnsProviderOrNull()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var provider = new OAuthProvider { Id = Guid.NewGuid(), UserId = userId, Provider = OAuthProviderType.Google, ProviderId = "pid" };
        _context.Users.Add(user);
        _context.OAuthProviders.Add(provider);
        await _context.SaveChangesAsync();
        var found = await _repository.GetByProviderAndProviderIdAsync(OAuthProviderType.Google, "pid");
        found.Should().NotBeNull();
        (await _repository.GetByProviderAndProviderIdAsync(OAuthProviderType.Facebook, "pid")).Should().BeNull();
    }

    [Fact]
    public async Task GetByProviderAndEmailAsync_ReturnsProviderOrNull()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var provider = new OAuthProvider { Id = Guid.NewGuid(), UserId = userId, Provider = OAuthProviderType.Google, ProviderEmail = "a@b.com" };
        _context.Users.Add(user);
        _context.OAuthProviders.Add(provider);
        await _context.SaveChangesAsync();
        var found = await _repository.GetByProviderAndEmailAsync(OAuthProviderType.Google, "a@b.com");
        found.Should().NotBeNull();
        (await _repository.GetByProviderAndEmailAsync(OAuthProviderType.Facebook, "a@b.com")).Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_UpdatesProvider()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var provider = new OAuthProvider { Id = Guid.NewGuid(), UserId = userId, AccessToken = "old", RefreshToken = "old", TokenExpiresAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _context.Users.Add(user);
        _context.OAuthProviders.Add(provider);
        await _context.SaveChangesAsync();
        var req = new OAuthProviderUpdateRequestDto { AccessToken = "new", RefreshToken = "new", TokenExpiresAt = DateTime.UtcNow.AddHours(1) };
        var updated = await _repository.UpdateAsync(provider.Id, req);
        updated.Should().NotBeNull();
        updated!.AccessToken.Should().Be("new");
        updated.RefreshToken.Should().Be("new");
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_IfNotFound()
    {
        var req = new OAuthProviderUpdateRequestDto { AccessToken = "new" };
        var updated = await _repository.UpdateAsync(Guid.NewGuid(), req);
        updated.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_RemovesProvider()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var provider = new OAuthProvider { Id = Guid.NewGuid(), UserId = userId };
        _context.Users.Add(user);
        _context.OAuthProviders.Add(provider);
        await _context.SaveChangesAsync();
        var result = await _repository.DeleteAsync(provider.Id);
        result.Should().BeTrue();
        (await _context.OAuthProviders.FindAsync(provider.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_IfNotFound()
    {
        var result = await _repository.DeleteAsync(Guid.NewGuid());
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrueOrFalse()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var provider = new OAuthProvider { Id = Guid.NewGuid(), UserId = userId, Provider = OAuthProviderType.Google, ProviderId = "pid" };
        _context.Users.Add(user);
        _context.OAuthProviders.Add(provider);
        await _context.SaveChangesAsync();
        (await _repository.ExistsAsync(OAuthProviderType.Google, "pid")).Should().BeTrue();
        (await _repository.ExistsAsync(OAuthProviderType.Facebook, "pid")).Should().BeFalse();
    }

    [Fact]
    public async Task GetByAccessTokenAsync_ReturnsProviderOrNull()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var provider = new OAuthProvider { Id = Guid.NewGuid(), UserId = userId, AccessToken = "token" };
        _context.Users.Add(user);
        _context.OAuthProviders.Add(provider);
        await _context.SaveChangesAsync();
        var found = await _repository.GetByAccessTokenAsync("token");
        found.Should().NotBeNull();
        (await _repository.GetByAccessTokenAsync("other")).Should().BeNull();
    }

    [Fact]
    public async Task GetByRefreshTokenAsync_ReturnsProviderOrNull()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var provider = new OAuthProvider { Id = Guid.NewGuid(), UserId = userId, RefreshToken = "refresh" };
        _context.Users.Add(user);
        _context.OAuthProviders.Add(provider);
        await _context.SaveChangesAsync();
        var found = await _repository.GetByRefreshTokenAsync("refresh");
        found.Should().NotBeNull();
        (await _repository.GetByRefreshTokenAsync("other")).Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WithEntity_UpdatesProvider()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var provider = new OAuthProvider { Id = Guid.NewGuid(), UserId = userId, AccessToken = "old", UpdatedAt = DateTime.UtcNow };
        _context.Users.Add(user);
        _context.OAuthProviders.Add(provider);
        await _context.SaveChangesAsync();
        provider.AccessToken = "new";
        var updated = await _repository.UpdateAsync(provider);
        updated.AccessToken.Should().Be("new");
    }
}
