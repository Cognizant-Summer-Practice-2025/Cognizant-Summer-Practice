using BackendMessages.Models;
using System.Text.Json;

namespace BackendMessages.Services
{
    public interface IUserSearchService
    {
        Task<List<SearchUser>> SearchUsersAsync(string searchTerm);
    }

    public class UserSearchService : IUserSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserSearchService> _logger;
        private readonly string _userServiceBaseUrl;

        public UserSearchService(HttpClient httpClient, ILogger<UserSearchService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _userServiceBaseUrl = configuration["UserService:BaseUrl"] ?? "http://localhost:5200";
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

                var response = await _httpClient.GetAsync(requestUrl);

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
    }
} 