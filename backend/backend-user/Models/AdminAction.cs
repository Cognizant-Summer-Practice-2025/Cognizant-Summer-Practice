using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_user.Models
{
    public class AdminAction
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("Admin")]
        public Guid AdminId { get; set; }

        [Required]
        public string TargetService { get; set; } = string.Empty;

        [Required]
        public TargetType TargetType { get; set; }

        [Required]
        public Guid TargetId { get; set; }

        [Required]
        public ActionType ActionType { get; set; }

        public string? Reason { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual User Admin { get; set; } = null!;
    }
} 