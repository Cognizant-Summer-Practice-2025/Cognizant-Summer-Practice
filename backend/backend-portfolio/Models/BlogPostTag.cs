using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_portfolio.Models
{
    public class BlogPostTag
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("BlogPost")]
        public Guid BlogPostId { get; set; }

        [Required]
        public string TagName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual BlogPost BlogPost { get; set; } = null!;
    }
} 