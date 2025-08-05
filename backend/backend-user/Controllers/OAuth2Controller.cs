using Microsoft.AspNetCore.Mvc;
using backend_user.Services.Abstractions;
using backend_user.DTO.OAuth.Request;
using backend_user.DTO.OAuth.Response;

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

        public OAuth2Controller(IOAuth2Service oauth2Service, IUserService userService)
        {
            _oauth2Service = oauth2Service ?? throw new ArgumentNullException(nameof(oauth2Service));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
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
                    return BadRequest(new { message = "Invalid or expired refresh token" });
                }

                return Ok(new { message = "Token refreshed successfully" });
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