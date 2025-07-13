using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_user.Models
{
    [Table("user_reports")]
    public class UserReport
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey("Reporter")]
        public Guid ReporterId { get; set; }

        [ForeignKey("ResolvedByUser")]
        public Guid? ResolvedBy { get; set; }

        [Required]
        [StringLength(50)]
        public string ReportedService { get; set; } = string.Empty;

        [Required]
        public ReportedType ReportedType { get; set; }

        [Required]
        public Guid ReportedId { get; set; }

        [Required]
        public ReportType ReportType { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        public string? AdminNotes { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual User Reporter { get; set; } = null!;
        public virtual User? ResolvedByUser { get; set; }
    }
} 