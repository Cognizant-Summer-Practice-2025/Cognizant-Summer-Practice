using BackendMessages.Models;
using BackendMessages.Services.Abstractions;
using System.Text.Json;

namespace BackendMessages.Services
{
    public class UserSearchService : IUserSearchService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UserSearchService> _logger;
        private readonly string _userServiceBaseUrl;

        public UserSearchService(IHttpClientFactory httpClientFactory, ILogger<UserSearchService> logger, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _userServiceBaseUrl = Environment.GetEnvironmentVariable("USER_SERVICE_URL")
                                   ?? configuration["UserServiceUrl"]
                                   ?? configuration["UserService:BaseUrl"]
                                   ?? "http://localhost:5200";
            
            _logger.LogInformation("🔍 UserSearchService: Using user service URL: {UserServiceUrl}", _userServiceBaseUrl);
        }

        public async Task<List<SearchUser>> SearchUsersAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new List<SearchUser>();
                }

                var encodedSearchTerm = Uri.EscapeDataString(searchTerm);
                var requestUrl = $"{_userServiceBaseUrl}/api/users/search?q={encodedSearchTerm}";

                _logger.LogInformation("Searching users with term: {SearchTerm}", searchTerm);

                using var httpClient = _httpClientFactory.CreateClient("UserService");
                var response = await httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var users = JsonSerializer.Deserialize<List<SearchUser>>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return users ?? new List<SearchUser>();
                }
                else
                {
                    _logger.LogWarning("User service returned error: {StatusCode}", response.StatusCode);
                    return new List<SearchUser>();
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error occurred while searching users");
                return new List<SearchUser>();
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout while searching users");
                return new List<SearchUser>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error while parsing user search response");
                return new List<SearchUser>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while searching users");
                return new List<SearchUser>();
            }
        }

        public async Task<SearchUser?> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var requestUrl = $"{_userServiceBaseUrl}/api/users/{userId}";
                _logger.LogInformation("🔍 UserSearchService: Requesting user {UserId} from {RequestUrl}", userId, requestUrl);
                
                using var httpClient = _httpClientFactory.CreateClient("UserService");
                var response = await httpClient.GetAsync(requestUrl);
                
                _logger.LogInformation("🔍 UserSearchService: Response status for user {UserId}: {StatusCode}", userId, response.StatusCode);
                
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _logger.LogWarning("🔍 UserSearchService: User {UserId} not found (404)", userId);
                        return null;
                    }
                    
                    _logger.LogWarning("🔍 UserSearchService: Failed to get user {UserId}. Status: {StatusCode}", userId, response.StatusCode);
                    return null;
                }

                var jsonContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("🔍 UserSearchService: Raw JSON response for user {UserId}: {JsonContent}", userId, jsonContent);
                
                var user = JsonSerializer.Deserialize<SearchUser>(jsonContent, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (user != null)
                {
                    _logger.LogInformation("🔍 UserSearchService: Successfully deserialized user {UserId} - FullName: '{FullName}', Username: '{Username}', Email: '{Email}'", 
                        userId, user.FullName ?? "NULL", user.Username ?? "NULL", user.Email ?? "NULL");
                }
                else
                {
                    _logger.LogWarning("🔍 UserSearchService: Failed to deserialize user {UserId} from JSON", userId);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔍 UserSearchService: Error getting user by id: {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> IsUserOnlineAsync(Guid userId)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient("UserService");
                var response = await httpClient.GetAsync($"{_userServiceBaseUrl}/api/users/{userId}/online-status");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get online status for user {UserId}. Status: {StatusCode}", userId, response.StatusCode);
                    return false;
                }

                var jsonContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (result != null && result.TryGetValue("isOnline", out var isOnlineValue))
                {
                    return Convert.ToBoolean(isOnlineValue);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking online status for user: {UserId}", userId);
                return false;
            }
        }
    }
} 