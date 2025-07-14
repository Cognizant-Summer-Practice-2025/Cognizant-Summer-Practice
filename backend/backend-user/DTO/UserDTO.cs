using System.ComponentModel.DataAnnotations;

namespace backend_user.DTO
{
    // Request DTO
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

    public class UserUpdateRequestDto
    {
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

    // Response DTO
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfessionalTitle { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class UserSummaryDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfessionalTitle { get; set; }
        public string? Location { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfessionalTitle { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public string? AvatarUrl { get; set; }
    }
} 