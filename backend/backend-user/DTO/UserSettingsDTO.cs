using backend_user.Models;

namespace backend_user.DTO;

// UserSettings Response DTO
public class UserSettingsResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public bool EmailNotifications { get; set; }
    public bool BrowserNotifications { get; set; }
    public bool MarketingEmails { get; set; }
    public ProfileVisibility ProfileVisibility { get; set; }
    public bool ShowEmail { get; set; }
    public bool ShowPhone { get; set; }
    public bool AllowMessages { get; set; }
    public string Language { get; set; } = "en";
    public string Timezone { get; set; } = "UTC";
    public Theme Theme { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// UserSettings Request DTO
public class UserSettingsRequestDto
{
    public bool EmailNotifications { get; set; } = true;
    public bool BrowserNotifications { get; set; } = true;
    public bool MarketingEmails { get; set; } = false;
    public ProfileVisibility ProfileVisibility { get; set; } = ProfileVisibility.Public;
    public bool ShowEmail { get; set; } = false;
    public bool ShowPhone { get; set; } = false;
    public bool AllowMessages { get; set; } = true;
    public string Language { get; set; } = "en";
    public string Timezone { get; set; } = "UTC";
    public Theme Theme { get; set; } = Theme.System;
}