namespace BackendMessages.Models
{
    public class SearchUser
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? ProfessionalTitle { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; }
    }
} 