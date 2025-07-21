using System.Text.Json;

namespace backend_messages.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserService> _logger;
        private readonly string _userApiBaseUrl;

        public UserService(HttpClient httpClient, ILogger<UserService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _userApiBaseUrl = configuration.GetValue<string>("UserApiBaseUrl") ?? "http://localhost:5200";
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_userApiBaseUrl}/api/users");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get all users: {StatusCode}", response.StatusCode);
                    return new List<UserDto>();
                }

                var json = await response.Content.ReadAsStringAsync();
                var usersResponse = JsonSerializer.Deserialize<List<UserApiResponse>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return usersResponse?.Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = $"{u.FirstName} {u.LastName}".Trim(),
                    Avatar = u.AvatarUrl,
                    ProfessionalTitle = u.ProfessionalTitle
                }).ToList() ?? new List<UserDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return new List<UserDto>();
            }
        }

        private class UserApiResponse
        {
            public Guid Id { get; set; }
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string? ProfessionalTitle { get; set; }
            public string? AvatarUrl { get; set; }
        }
    }
} 