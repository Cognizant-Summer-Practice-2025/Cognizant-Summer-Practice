using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_user.Models
{
    [Table("bookmarks")]
    public class Bookmark
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(255)]
        public string PortfolioId { get; set; } = string.Empty;

        [StringLength(255)]
        public string? PortfolioTitle { get; set; }

        [StringLength(255)]
        public string? PortfolioOwnerName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
} 