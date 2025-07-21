namespace backend_messages.Services
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public string? ProfessionalTitle { get; set; }
    }

    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();
    }
} 