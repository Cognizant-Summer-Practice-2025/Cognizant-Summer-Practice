using backend_portfolio.Services.Abstractions;
using System.Text.Json;

namespace backend_portfolio.Services.External
{
    public class ExternalUserService : IExternalUserService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExternalUserService> _logger;
        private readonly string _userServiceBaseUrl;

        public ExternalUserService(HttpClient httpClient, ILogger<ExternalUserService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _userServiceBaseUrl = configuration["ExternalServices:UserService:BaseUrl"] ?? "http://localhost:5200";
        }

        public async Task<UserInformation?> GetUserInformationAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_userServiceBaseUrl}/api/users/{userId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch user information for user {UserId}. Status: {StatusCode}", 
                        userId, response.StatusCode);
                    return null;
                }

                var jsonContent = await response.Content.ReadAsStringAsync();
                var userJson = JsonSerializer.Deserialize<JsonElement>(jsonContent);

                var firstName = GetJsonProperty(userJson, "firstName") ?? "";
                var lastName = GetJsonProperty(userJson, "lastName") ?? "";
                var fullName = $"{firstName} {lastName}".Trim();
                if (string.IsNullOrEmpty(fullName)) fullName = "Anonymous User";
                
                var jobTitle = GetJsonProperty(userJson, "professionalTitle") ?? "Professional";
                var location = GetJsonProperty(userJson, "location") ?? "Remote";
                var profilePictureUrl = GetJsonProperty(userJson, "avatarUrl") ?? "/default-avatar.png";

                return new UserInformation(fullName, jobTitle, location, profilePictureUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching user information for user {UserId}", userId);
                return null;
            }
        }

        private static string? GetJsonProperty(JsonElement element, string propertyName)
        {
            try
            {
                return element.TryGetProperty(propertyName, out var property) ? property.GetString() : null;
            }
            catch
            {
                return null;
            }
        }
    }
} 