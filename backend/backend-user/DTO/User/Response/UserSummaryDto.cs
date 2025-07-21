namespace backend_user.DTO.User.Response
{
    /// <summary>
    /// Summarized user information for list views and brief representations.
    /// </summary>
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
}
