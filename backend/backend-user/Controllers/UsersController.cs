using backend_user.Models;
using backend_user.Repositories;
using backend_user.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_user.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUserRepository userRepository, IOAuthProviderRepository oauthProviderRepository) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await userRepository.GetAllUsers();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await userRepository.GetUserById(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }
            return Ok(user);
        }
        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await userRepository.GetUserByEmail(email);
            return Ok(user);
        }

        [HttpGet("check-email/{email}")]
        public async Task<IActionResult> CheckUserExistsByEmail(string email)
        {
            var user = await userRepository.GetUserByEmail(email);
            return Ok(new { exists = user != null, user = user });
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(User user)
        {
            var newUser = await userRepository.CreateUser(user);
            return Ok(newUser);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request)
        {
            try
            {
                var existingUser = await userRepository.GetUserByEmail(request.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "User already exists" });
                }

                var username = request.Email.Split('@')[0];
                
                var user = new User
                {
                    Email = request.Email,
                    Username = username,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    ProfessionalTitle = request.ProfessionalTitle,
                    Bio = request.Bio,
                    Location = request.Location,
                    AvatarUrl = request.ProfileImage
                };

                var newUser = await userRepository.CreateUser(user);
                return Ok(newUser);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // OAuth Provider endpoints
        [HttpGet("{userId}/oauth-providers")]
        public async Task<IActionResult> GetUserOAuthProviders(Guid userId)
        {
            var providers = await oauthProviderRepository.GetByUserIdAsync(userId);
            var response = providers.Select(p => new OAuthProviderSummaryDto
            {
                Id = p.Id,
                Provider = p.Provider,
                ProviderEmail = p.ProviderEmail,
                CreatedAt = p.CreatedAt
            });
            return Ok(response);
        }

        [HttpPost("oauth-providers")]
        public async Task<IActionResult> CreateOAuthProvider([FromBody] OAuthProviderCreateRequestDto request)
        {
            try
            {
                var provider = await oauthProviderRepository.CreateAsync(request);
                var response = new OAuthProviderResponseDto
                {
                    Id = provider.Id,
                    UserId = provider.UserId,
                    Provider = provider.Provider,
                    ProviderId = provider.ProviderId,
                    ProviderEmail = provider.ProviderEmail,
                    TokenExpiresAt = provider.TokenExpiresAt,
                    CreatedAt = provider.CreatedAt,
                    UpdatedAt = provider.UpdatedAt
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("oauth-providers/{id}")]
        public async Task<IActionResult> UpdateOAuthProvider(Guid id, [FromBody] OAuthProviderUpdateRequestDto request)
        {
            var provider = await oauthProviderRepository.UpdateAsync(id, request);
            if (provider == null)
                return NotFound();

            var response = new OAuthProviderResponseDto
            {
                Id = provider.Id,
                UserId = provider.UserId,
                Provider = provider.Provider,
                ProviderId = provider.ProviderId,
                ProviderEmail = provider.ProviderEmail,
                TokenExpiresAt = provider.TokenExpiresAt,
                CreatedAt = provider.CreatedAt,
                UpdatedAt = provider.UpdatedAt
            };
            return Ok(response);
        }

        [HttpDelete("oauth-providers/{id}")]
        public async Task<IActionResult> DeleteOAuthProvider(Guid id)
        {
            var result = await oauthProviderRepository.DeleteAsync(id);
            if (!result)
                return NotFound();
            return NoContent();
        }

        [HttpGet("oauth-providers/check")]
        public async Task<IActionResult> CheckOAuthProvider([FromQuery] OAuthProviderType provider, [FromQuery] string providerId)
        {
            var exists = await oauthProviderRepository.ExistsAsync(provider, providerId);
            var oauthProvider = await oauthProviderRepository.GetByProviderAndProviderIdAsync(provider, providerId);
            return Ok(new { exists = exists, provider = oauthProvider });
        }

        [HttpGet("{userId}/oauth-providers/{provider}")]
        public async Task<IActionResult> GetUserOAuthProviderByType(Guid userId, OAuthProviderType provider)
        {
            var oauthProvider = await oauthProviderRepository.GetByUserIdAndProviderAsync(userId, provider);
            if (oauthProvider == null)
            {
                return Ok(new { exists = false, provider = (OAuthProvider?)null });
            }

            var response = new OAuthProviderResponseDto
            {
                Id = oauthProvider.Id,
                UserId = oauthProvider.UserId,
                Provider = oauthProvider.Provider,
                ProviderId = oauthProvider.ProviderId,
                ProviderEmail = oauthProvider.ProviderEmail,
                TokenExpiresAt = oauthProvider.TokenExpiresAt,
                CreatedAt = oauthProvider.CreatedAt,
                UpdatedAt = oauthProvider.UpdatedAt
            };
            return Ok(new { exists = true, provider = response });
        }

        [HttpPost("register-oauth")]
        public async Task<IActionResult> RegisterOAuthUser([FromBody] RegisterOAuthUserRequest request)
        {
            try
            {
                // Check if OAuth provider already exists
                var existingProvider = await oauthProviderRepository.GetByProviderAndProviderIdAsync(
                    request.Provider, request.ProviderId);
                
                if (existingProvider != null)
                {
                    return BadRequest(new { message = "OAuth provider already linked to another user" });
                }

                // Check if user already exists by email
                var existingUser = await userRepository.GetUserByEmail(request.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "User already exists" });
                }

                var username = request.Email.Split('@')[0];

                // Create user first
                var user = new User
                {
                    Email = request.Email,
                    Username = username,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    ProfessionalTitle = request.ProfessionalTitle,
                    Bio = request.Bio,
                    Location = request.Location,
                    AvatarUrl = request.ProfileImage
                };

                var newUser = await userRepository.CreateUser(user);

                // create OAuth provider
                var oauthRequest = new OAuthProviderCreateRequestDto
                {
                    UserId = newUser.Id,
                    Provider = request.Provider,
                    ProviderId = request.ProviderId,
                    ProviderEmail = request.ProviderEmail,
                    AccessToken = request.AccessToken,
                    RefreshToken = request.RefreshToken,
                    TokenExpiresAt = request.TokenExpiresAt
                };

                var oauthProvider = await oauthProviderRepository.CreateAsync(oauthRequest);

                var userResponse = new UserResponseDto
                {
                    Id = newUser.Id,
                    Email = newUser.Email,
                    Username = newUser.Username,
                    FirstName = newUser.FirstName,
                    LastName = newUser.LastName,
                    ProfessionalTitle = newUser.ProfessionalTitle,
                    Bio = newUser.Bio,
                    Location = newUser.Location,
                    AvatarUrl = newUser.AvatarUrl,
                    IsActive = newUser.IsActive,
                    IsAdmin = newUser.IsAdmin,
                    LastLoginAt = newUser.LastLoginAt
                };

                var oauthResponse = new OAuthProviderResponseDto
                {
                    Id = oauthProvider.Id,
                    UserId = oauthProvider.UserId,
                    Provider = oauthProvider.Provider,
                    ProviderId = oauthProvider.ProviderId,
                    ProviderEmail = oauthProvider.ProviderEmail,
                    TokenExpiresAt = oauthProvider.TokenExpiresAt,
                    CreatedAt = oauthProvider.CreatedAt,
                    UpdatedAt = oauthProvider.UpdatedAt
                };

                return Ok(new { user = userResponse, oauthProvider = oauthResponse });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
    }
}
