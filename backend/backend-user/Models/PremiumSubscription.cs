using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_user.Models
{
    [Table("premium_subscriptions")]
    public class PremiumSubscription
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [StringLength(255)]
        public string? StripeSubscriptionId { get; set; }

        [StringLength(255)]
        public string? StripeCustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "active";

        public DateTime? CurrentPeriodStart { get; set; }

        public DateTime? CurrentPeriodEnd { get; set; }

        [Required]
        public bool CancelAtPeriodEnd { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}
