using Xunit;
using backend_user.DTO.Newsletter.Request;
using backend_user.DTO.Newsletter.Response;
using System;

public class NewsletterDTOTests
{
    [Fact]
    public void NewsletterCreateRequestDto_Defaults_AreCorrect()
    {
        var dto = new NewsletterCreateRequestDto();
        Assert.Equal(Guid.Empty, dto.UserId);
        Assert.Equal(string.Empty, dto.Type);
        Assert.True(dto.IsActive);
    }

    [Fact]
    public void NewsletterCreateRequestDto_SetProperties_Works()
    {
        var dto = new NewsletterCreateRequestDto
        {
            UserId = Guid.NewGuid(),
            Type = "weekly",
            IsActive = false
        };
        Assert.Equal("weekly", dto.Type);
        Assert.False(dto.IsActive);
    }

    [Fact]
    public void NewsletterUpdateRequestDto_SetProperties_Works()
    {
        var dto = new NewsletterUpdateRequestDto { IsActive = false };
        Assert.False(dto.IsActive);
        dto.IsActive = true;
        Assert.True(dto.IsActive);
    }

    [Fact]
    public void NewsletterResponseDto_SetProperties_Works()
    {
        var now = DateTime.UtcNow;
        var dto = new NewsletterResponseDto
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Type = "monthly",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        Assert.Equal("monthly", dto.Type);
        Assert.True(dto.IsActive);
        Assert.Equal(now, dto.CreatedAt);
        Assert.Equal(now, dto.UpdatedAt);
    }

    [Fact]
    public void NewsletterSummaryDto_SetProperties_Works()
    {
        var dto = new NewsletterSummaryDto
        {
            Id = Guid.NewGuid(),
            Type = "special",
            IsActive = false
        };
        Assert.Equal("special", dto.Type);
        Assert.False(dto.IsActive);
    }
}