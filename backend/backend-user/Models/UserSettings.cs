using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_user.Models
{
    public class UserSettings
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public Guid UserId { get; set; }

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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
} 