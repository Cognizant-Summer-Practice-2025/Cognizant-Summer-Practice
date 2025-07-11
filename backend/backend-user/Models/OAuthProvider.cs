using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_user.Models
{
    public class OAuthProvider
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public Guid UserId { get; set; }

        [Required]
        public Provider Provider { get; set; }

        [Required]
        public string ProviderId { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string ProviderEmail { get; set; } = string.Empty;

        public string? AccessToken { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime? TokenExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
} 