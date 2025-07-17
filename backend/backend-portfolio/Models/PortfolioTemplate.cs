using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace backend_portfolio.Models
{
    [Table("portfolio_templates")]
    public class PortfolioTemplate
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description", TypeName = "text")]
        public string? Description { get; set; }



        [Column("preview_image_url", TypeName = "text")]
        public string? PreviewImageUrl { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Portfolio> Portfolios { get; set; } = new List<Portfolio>();
    }
} 