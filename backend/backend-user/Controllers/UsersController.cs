using backend_user.Models;
using backend_user.Services.Abstractions;
using backend_user.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_user.Controllers
{
    /// <summary>
    /// UsersController for managing user-related operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IOAuthProviderService _oauthProviderService;
        private readonly IUserRegistrationService _userRegistrationService;
        private readonly ILoginService _loginService;

        public UsersController(
            IUserService userService,
            IOAuthProviderService oauthProviderService,
            IUserRegistrationService userRegistrationService,
            ILoginService loginService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _oauthProviderService = oauthProviderService ?? throw new ArgumentNullException(nameof(oauthProviderService));
            _userRegistrationService = userRegistrationService ?? throw new ArgumentNullException(nameof(userRegistrationService));
            _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
        }
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }
            return Ok(user);
        }

        [HttpGet("{id}/portfolio-info")]
        public async Task<IActionResult> GetUserPortfolioInfo(Guid id)
        {
            var portfolioInfo = await _userService.GetUserPortfolioInfoAsync(id);
            if (portfolioInfo == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            return Ok(portfolioInfo);
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            return Ok(user);
        }

        [HttpGet("check-email/{email}")]
        public async Task<IActionResult> CheckUserExistsByEmail(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            return Ok(new { exists = user != null, user = user });
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(User user)
        {
            // Convert User entity to RegisterUserRequest for service compatibility
            var registerRequest = new RegisterUserRequest
            {
                Email = user.Email,
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                ProfessionalTitle = user.ProfessionalTitle,
                Bio = user.Bio,
                Location = user.Location,
                ProfileImage = user.AvatarUrl
            };

            try
            {
                var newUser = await _userService.CreateUserAsync(registerRequest);
                return Ok(newUser);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request)
        {
            try
            {
                var newUser = await _userRegistrationService.RegisterUserAsync(request);
                return Ok(newUser);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login-oauth")]
        public async Task<IActionResult> LoginWithOAuth([FromBody] OAuthLoginRequestDto request)
        {
            try
            {
                var result = await _loginService.LoginWithOAuthAsync(request);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var updatedUser = await _userService.UpdateUserAsync(id, request);
                if (updatedUser == null)
                {
                    return NotFound($"User with ID {id} not found.");
                }

                var response = new UserResponseDto
                {
                    Id = updatedUser.Id,
                    Email = updatedUser.Email,
                    Username = updatedUser.Username,
                    FirstName = updatedUser.FirstName,
                    LastName = updatedUser.LastName,
                    ProfessionalTitle = updatedUser.ProfessionalTitle,
                    Bio = updatedUser.Bio,
                    Location = updatedUser.Location,
                    AvatarUrl = updatedUser.AvatarUrl,
                    IsActive = updatedUser.IsActive,
                    IsAdmin = updatedUser.IsAdmin,
                    LastLoginAt = updatedUser.LastLoginAt
                };

                return Ok(response);
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
            var response = await _oauthProviderService.GetUserOAuthProvidersAsync(userId);
            return Ok(response);
        }

        [HttpPost("oauth-providers")]
        public async Task<IActionResult> CreateOAuthProvider([FromBody] OAuthProviderCreateRequestDto request)
        {
            try
            {
                var response = await _oauthProviderService.CreateOAuthProviderAsync(request);
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
            var response = await _oauthProviderService.UpdateOAuthProviderAsync(id, request);
            if (response == null)
                return NotFound();
            return Ok(response);
        }

        [HttpDelete("oauth-providers/{id}")]
        public async Task<IActionResult> DeleteOAuthProvider(Guid id)
        {
            var result = await _oauthProviderService.DeleteOAuthProviderAsync(id);
            if (!result)
                return NotFound();
            return NoContent();
        }

        [HttpGet("oauth-providers/check")]
        public async Task<IActionResult> CheckOAuthProvider([FromQuery] OAuthProviderType provider, [FromQuery] string providerId)
        {
            var result = await _oauthProviderService.CheckOAuthProviderAsync(provider, providerId);
            return Ok(result);
        }

        [HttpGet("{userId}/oauth-providers/{provider}")]
        public async Task<IActionResult> GetUserOAuthProviderByType(Guid userId, OAuthProviderType provider)
        {
            var result = await _oauthProviderService.GetUserOAuthProviderByTypeAsync(userId, provider);
            return Ok(result);
        }

        [HttpPost("register-oauth")]
        public async Task<IActionResult> RegisterOAuthUser([FromBody] RegisterOAuthUserRequest request)
        {
            try
            {
                var result = await _userRegistrationService.RegisterOAuthUserAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
    }
}
