namespace backend_user.DTO.User.Response
{
    /// <summary>
    /// User profile information for portfolio display.
    /// </summary>
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
