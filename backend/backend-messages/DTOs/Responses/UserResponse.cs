using backend_messages.Models;

namespace backend_messages.DTOs.Responses
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public string? ProfessionalTitle { get; set; }
    }
}