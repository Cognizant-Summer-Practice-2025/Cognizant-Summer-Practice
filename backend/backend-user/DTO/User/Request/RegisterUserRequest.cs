using System.ComponentModel.DataAnnotations;

namespace backend_user.DTO.User.Request
{
    /// <summary>
    /// Request DTO for regular user registration.
    /// </summary>
    public class RegisterUserRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? ProfessionalTitle { get; set; }

        public string? Bio { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        public string? ProfileImage { get; set; }
    }
}
