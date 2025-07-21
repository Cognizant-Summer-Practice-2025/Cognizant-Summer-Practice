using System.ComponentModel.DataAnnotations;

namespace backend_user.DTO.User.Request
{
    /// <summary>
    /// Legacy DTO for backwards compatibility with user updates.
    /// </summary>
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
}
