using backend_user.DTO.User.Response;

namespace backend_user.DTO.Authentication.Response
{
    /// <summary>
    /// Response DTO for login operations.
    /// </summary>
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public UserResponseDto? User { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}
