using System.Security.Claims;
using System.Text.Json;
using backend_AI.Services.Abstractions;

namespace backend_AI.Services
{
    /// <summary>
    /// Service for validating user authentication tokens via the user service.
    /// </summary>
    public class UserAuthenticationService : IUserAuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserAuthenticationService> _logger;

        public UserAuthenticationService(HttpClient httpClient, IConfiguration configuration, ILogger<UserAuthenticationService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
        {
            try
            {
                var configuredBaseUrl = Environment.GetEnvironmentVariable("USER_SERVICE_URL")
                                        ?? Environment.GetEnvironmentVariable("AI_USER_SERVICE_URL")
                                        ?? _configuration["UserServiceUrl"]
                                        ?? _configuration["UserService:BaseUrl"]
                                        ?? "http://localhost:5200";
                if (string.IsNullOrEmpty(configuredBaseUrl))
                {
                    _logger.LogError("AuthService(AI): USER_SERVICE_URL not configured. Please set UserServiceUrl in appsettings.json or USER_SERVICE_URL environment variable.");
                    return null;
                }
                _logger.LogInformation("AuthService(AI): Validating token with user service at {UserServiceUrl}", configuredBaseUrl);

                var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress != null ? new Uri("/api/oauth/me", UriKind.Relative) : new Uri($"{configuredBaseUrl}/api/oauth/me", UriKind.Absolute));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                _logger.LogInformation("AuthService(AI): Sending validation request to user service with token length: {TokenLength}", token.Length);

                var response = await _httpClient.SendAsync(request);
                _logger.LogInformation("AuthService(AI): User service response status: {StatusCode}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("AuthService(AI): Token validation failed with status: {StatusCode}", response.StatusCode);
                    return null;
                }

                var userJson = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("AuthService(AI): User service response body: {ResponseBody}", userJson);

                var userInfo = JsonSerializer.Deserialize<UserInfo>(userJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (userInfo == null)
                {
                    _logger.LogWarning("AuthService(AI): Failed to deserialize user info from token validation");
                    return null;
                }

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, userInfo.UserId ?? string.Empty),
                    new(ClaimTypes.Email, userInfo.Email ?? string.Empty),
                    new(ClaimTypes.Name, userInfo.Username ?? string.Empty),
                    new("IsAdmin", userInfo.IsAdmin.ToString())
                };

                var identity = new ClaimsIdentity(claims, "OAuth2");
                return new ClaimsPrincipal(identity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AuthService(AI): Error validating token");
                return null;
            }
        }

        private class UserInfo
        {
            public string UserId { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public bool IsAdmin { get; set; }
        }
    }
}


