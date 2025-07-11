using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_user.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsAdmin { get; set; } = false;

        public bool EmailVerified { get; set; } = false;

        public DateTime? LastLoginAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public virtual ICollection<OAuthProvider> OAuthProviders { get; set; } = new List<OAuthProvider>();
        public virtual UserSettings? UserSettings { get; set; }
        public virtual ICollection<Newsletter> Newsletters { get; set; } = new List<Newsletter>();
        public virtual ICollection<UserAnalytics> UserAnalytics { get; set; } = new List<UserAnalytics>();
        public virtual ICollection<AdminAction> AdminActions { get; set; } = new List<AdminAction>();
        public virtual ICollection<UserReport> ReportsCreated { get; set; } = new List<UserReport>();
        public virtual ICollection<UserReport> ReportsResolved { get; set; } = new List<UserReport>();
    }
}
