using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend_portfolio.Models.Validation;

namespace backend_portfolio.Models
{
    [Table("bookmarks")]
    public class Bookmark
    {
        public Bookmark()
        {
            Id = Guid.NewGuid();
        }

        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [NotEmptyGuid]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [NotEmptyGuid]
        [Column("portfolio_id")]
        public Guid PortfolioId { get; set; }

        [StringLength(100)]
        [Column("collection_name")]
        public string? CollectionName { get; set; }

        [Column("notes", TypeName = "text")]
        public string? Notes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [System.Text.Json.Serialization.JsonIgnore]
        [ForeignKey("PortfolioId")]
        public virtual Portfolio Portfolio { get; set; } = null!;
    }
} 