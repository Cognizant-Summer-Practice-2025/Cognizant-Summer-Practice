namespace backend_user.DTO.OAuth.Response
{
    /// <summary>
    /// Response DTO for token validation containing user information.
    /// </summary>
    public class ValidateTokenResponse
    {
        public Guid UserId { get; set; }
        public required string Email { get; set; }
        public required string Username { get; set; }
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
}