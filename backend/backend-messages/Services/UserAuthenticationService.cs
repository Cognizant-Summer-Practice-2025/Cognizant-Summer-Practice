using BackendMessages.Services.Abstractions;
using System.Security.Claims;
using System.Text.Json;

namespace BackendMessages.Services
{
    /// <summary>
    /// Service for validating user authentication tokens via the user service.
    /// </summary>
    public class UserAuthenticationService : IUserAuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserAuthenticationService> _logger;

        public UserAuthenticationService(
            HttpClient httpClient, 
            IConfiguration configuration,
            ILogger<UserAuthenticationService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Validates an access token by calling the user service.
        /// </summary>
        /// <param name="token">The access token to validate.</param>
        /// <returns>The user claims if token is valid, null otherwise.</returns>
        public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
        {
            try
            {
                var configuredBaseUrl = Environment.GetEnvironmentVariable("USER_SERVICE_URL")
                                        ?? _configuration["UserService:BaseUrl"]
                                        ?? _configuration["UserServiceUrl"]
                                        ?? "http://localhost:5200";
                
                // Call the user service to validate the token and get user info
                var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress != null ? new Uri("/api/oauth/me", UriKind.Relative) : new Uri($"{configuredBaseUrl}/api/oauth/me", UriKind.Absolute));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("🔐 AuthService: Token validation failed with status: {StatusCode}", response.StatusCode);
                    return null;
                }

                var userJson = await response.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<UserInfo>(userJson, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (userInfo == null)
                {
                    _logger.LogWarning("🔐 AuthService: Failed to deserialize user info from token validation");
                    return null;
                }
                
                 // Create claims from user info
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, userInfo.UserId.ToString()),
                    new(ClaimTypes.Email, userInfo.Email),
                    new(ClaimTypes.Name, userInfo.Username),
                    new("IsAdmin", userInfo.IsAdmin.ToString())
                };

                var identity = new ClaimsIdentity(claims, "OAuth2");
                return new ClaimsPrincipal(identity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔐 AuthService: Error validating token");
                return null;
            }
        }

        /// <summary>
        /// User info DTO for token validation response.
        /// </summary>
        private class UserInfo
        {
            public Guid UserId { get; set; }
            public string Email { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public bool IsAdmin { get; set; }
        }
    }
}
