using Microsoft.AspNetCore.Mvc;
using backend_user.Services.Abstractions;
using backend_user.DTO.OAuth.Request;
using backend_user.DTO.OAuth.Response;
using backend_user.Repositories;
using backend_user.Models;

namespace backend_user.Controllers
{
    /// <summary>
    /// OAuth 2.0 endpoints for authorization and token management.
    /// </summary>
    [Route("api/oauth")]
    [ApiController]
    public class OAuth2Controller : ControllerBase
    {
        private readonly IOAuth2Service _oauth2Service;
        private readonly IUserService _userService;
        private readonly IOAuthProviderRepository _oauthProviderRepository;

        public OAuth2Controller(IOAuth2Service oauth2Service, IUserService userService, IOAuthProviderRepository oauthProviderRepository)
        {
            _oauth2Service = oauth2Service ?? throw new ArgumentNullException(nameof(oauth2Service));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _oauthProviderRepository = oauthProviderRepository ?? throw new ArgumentNullException(nameof(oauthProviderRepository));
        }

        /// <summary>
        /// Validates an access token and returns user information.
        /// </summary>
        /// <param name="request">Token validation request.</param>
        /// <returns>User information if token is valid.</returns>
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request)
        {
            try
            {
                var user = await _oauth2Service.GetUserByAccessTokenAsync(request.AccessToken);
                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid or expired access token" });
                }

                var response = new ValidateTokenResponse
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    IsAdmin = user.IsAdmin
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Refreshes an access token using a refresh token.
        /// </summary>
        /// <param name="request">Token refresh request.</param>
        /// <returns>Success or failure of token refresh.</returns>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var success = await _oauth2Service.RefreshAccessTokenAsync(request.RefreshToken);
                if (!success)
                {
                    return BadRequest(new { 
                        message = "Invalid or expired refresh token. Note: GitHub OAuth apps don't support refresh tokens. Only Google with proper configuration provides refresh tokens.", 
                        timestamp = DateTime.UtcNow 
                    });
                }

                return Ok(new { 
                    message = "Token refreshed successfully", 
                    timestamp = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets the status of refresh tokens for the current user.
        /// </summary>
        /// <returns>Token status information for debugging.</returns>
        [HttpGet("token-status")]
        public async Task<IActionResult> GetTokenStatus()
        {
            try
            {
                // Extract token from Authorization header
                var authHeader = Request.Headers.Authorization.FirstOrDefault();
                if (authHeader == null || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized(new { message = "Missing or invalid Authorization header" });
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();
                var user = await _oauth2Service.GetUserByAccessTokenAsync(token);
                
                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid access token" });
                }

                // Get all OAuth providers for this user to check refresh token status
                var oauthProviders = await _oauthProviderRepository.GetByUserIdAsync(user.Id);
                
                var tokenStatus = oauthProviders.Select(provider => new
                {
                    Provider = provider.Provider.ToString(),
                    HasRefreshToken = !string.IsNullOrEmpty(provider.RefreshToken),
                    TokenExpiresAt = provider.TokenExpiresAt,
                    IsExpired = provider.TokenExpiresAt.HasValue && provider.TokenExpiresAt.Value < DateTime.UtcNow,
                    SupportsRefresh = provider.Provider == OAuthProviderType.Google, // Only Google supports refresh tokens in this implementation
                    Note = provider.Provider == OAuthProviderType.GitHub ? "GitHub OAuth apps don't support refresh tokens" : null
                }).ToList();

                return Ok(new
                {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    Providers = tokenStatus,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets user profile information using access token from Authorization header.
        /// </summary>
        /// <returns>User profile information.</returns>
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                // Extract token from Authorization header
                var authHeader = Request.Headers.Authorization.FirstOrDefault();
                if (authHeader == null || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized(new { message = "Missing or invalid Authorization header" });
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();
                var user = await _oauth2Service.GetUserByAccessTokenAsync(token);
                
                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid or expired access token" });
                }

                var response = new ValidateTokenResponse
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ProfessionalTitle = user.ProfessionalTitle,
                    Bio = user.Bio,
                    Location = user.Location,
                    AvatarUrl = user.AvatarUrl,
                    IsActive = user.IsActive,
                    IsAdmin = user.IsAdmin,
                    LastLoginAt = user.LastLoginAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}