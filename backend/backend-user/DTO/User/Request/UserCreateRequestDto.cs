using System.ComponentModel.DataAnnotations;

namespace backend_user.DTO.User.Request
{
    /// <summary>
    /// Legacy DTO for backwards compatibility.
    /// </summary>
    public class UserCreateRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [StringLength(200)]
        public string? ProfessionalTitle { get; set; }

        public string? Bio { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        public string? AvatarUrl { get; set; }
    }
}
