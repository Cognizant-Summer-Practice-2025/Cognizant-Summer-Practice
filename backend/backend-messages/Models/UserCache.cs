using System.ComponentModel.DataAnnotations;

namespace backend_messages.Models
{
    public class UserCache
    {
        [Key]
        public Guid UserId { get; set; } // Primary key and external reference to User Service

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
} 