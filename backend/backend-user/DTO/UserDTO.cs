using System.ComponentModel.DataAnnotations;
using backend_user.Models;

namespace backend_user.DTO
{
    // Request DTO for regular user registration
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

    // Request DTO for OAuth user registration
    public class RegisterOAuthUserRequest
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

        // OAuth provider data
        [Required]
        public OAuthProviderType Provider { get; set; }

        [Required]
        public string ProviderId { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string ProviderEmail { get; set; } = string.Empty;

        [Required]
        public string AccessToken { get; set; } = string.Empty;

        public string? RefreshToken { get; set; }

        public DateTime? TokenExpiresAt { get; set; }
    }

    // Update request DTO
    public class UpdateUserRequest
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

        public string? ProfileImage { get; set; }
    }

    // Legacy DTO for backwards compatibility
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

    // Login DTOs
    public class OAuthLoginRequestDto
    {
        [Required]
        public OAuthProviderType Provider { get; set; }

        [Required]
        public string ProviderId { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string ProviderEmail { get; set; } = string.Empty;

        [Required]
        public string AccessToken { get; set; } = string.Empty;

        public string? RefreshToken { get; set; }

        public DateTime? TokenExpiresAt { get; set; }
    }

    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public UserResponseDto? User { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
} 