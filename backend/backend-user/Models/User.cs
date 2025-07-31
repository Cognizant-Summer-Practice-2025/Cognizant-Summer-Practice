using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_user.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [StringLength(200)]
        public string? ProfessionalTitle { get; set; }

        public string? Bio { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        public string? AvatarUrl { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public bool IsAdmin { get; set; } = false;

        public DateTime? LastLoginAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<OAuthProvider> OAuthProviders { get; set; } = new List<OAuthProvider>();
        public virtual ICollection<Newsletter> Newsletters { get; set; } = new List<Newsletter>();
        public virtual ICollection<UserAnalytics> UserAnalytics { get; set; } = new List<UserAnalytics>();
        public virtual ICollection<UserReport> ReportsCreated { get; set; } = new List<UserReport>();
        public virtual ICollection<UserReport> ReportsResolved { get; set; } = new List<UserReport>();
        public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    }
}
