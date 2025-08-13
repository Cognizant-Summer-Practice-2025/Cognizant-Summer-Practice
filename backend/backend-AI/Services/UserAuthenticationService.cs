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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserAuthenticationService> _logger;

        public UserAuthenticationService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<UserAuthenticationService> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
        {
            try
            {
                var userServiceUrl = _configuration["UserServiceUrl"] ?? Environment.GetEnvironmentVariable("USER_SERVICE_URL") ?? "http://localhost:5200";
                _logger.LogInformation("AuthService(AI): Validating token with user service at {UserServiceUrl}", userServiceUrl);

                var request = new HttpRequestMessage(HttpMethod.Get, $"{userServiceUrl}/api/oauth/me");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                _logger.LogInformation("AuthService(AI): Sending validation request to user service with token length: {TokenLength}", token.Length);

                using var httpClient = _httpClientFactory.CreateClient("UserService");
                var response = await httpClient.SendAsync(request);
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


