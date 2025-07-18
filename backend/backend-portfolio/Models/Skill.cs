using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_portfolio.Models
{
    [Table("skills")]
    public class Skill
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("portfolio_id")]
        public Guid PortfolioId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        [Column("category_type")]
        public string? CategoryType { get; set; }

        [StringLength(100)]
        [Column("subcategory")]
        public string? Subcategory { get; set; }

        [StringLength(255)]
        [Column("category")]
        public string? Category { get; set; }

        [Column("proficiency_level")]
        public int? ProficiencyLevel { get; set; }

        [Column("display_order")]
        public int? DisplayOrder { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("PortfolioId")]
        public virtual Portfolio Portfolio { get; set; } = null!;
    }
} 