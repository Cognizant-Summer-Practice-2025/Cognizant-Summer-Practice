using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Required]
        [StringLength(100)]
        [Column("component_name")]
        public string ComponentName { get; set; } = string.Empty;

        [Column("preview_image_url", TypeName = "text")]
        public string? PreviewImageUrl { get; set; }

        [Column("default_config", TypeName = "jsonb")]
        public string? DefaultConfig { get; set; }

        [Column("default_sections", TypeName = "jsonb")]
        public string? DefaultSections { get; set; }

        [Column("customizable_options", TypeName = "jsonb")]
        public string? CustomizableOptions { get; set; }

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