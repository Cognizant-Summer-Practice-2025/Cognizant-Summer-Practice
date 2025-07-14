using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_user.Models
{
    [Table("oauth_providers")]
    public class OAuthProvider
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey("User")]
        public Guid UserId { get; set; }

        [Required]
        public OAuthProviderType Provider { get; set; }

        [Required]
        [StringLength(255)]
        public string ProviderId { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string ProviderEmail { get; set; } = string.Empty;

        [Required]
        public string AccessToken { get; set; } = string.Empty;

        public string? RefreshToken { get; set; }

        public DateTime? TokenExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
} 