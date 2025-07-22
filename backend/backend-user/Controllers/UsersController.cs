using backend_user.Models;
using backend_user.Services.Abstractions;
using backend_user.DTO.User.Request;
using backend_user.DTO.User.Response;
using backend_user.DTO.Authentication.Request;
using backend_user.DTO.Authentication.Response;
using backend_user.DTO.OAuthProvider.Request;
using backend_user.DTO.OAuthProvider.Response;
using backend_user.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_user.Controllers
{
    /// <summary>
    /// UsersController for managing user-related operations.
    /// </summary>
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IOAuthProviderService _oauthProviderService;
        private readonly IUserRegistrationService _userRegistrationService;
        private readonly ILoginService _loginService;
        private readonly IBookmarkService _bookmarkService;

        public UsersController(
            IUserService userService,
            IOAuthProviderService oauthProviderService,
            IUserRegistrationService userRegistrationService,
            ILoginService loginService,
            IBookmarkService bookmarkService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _oauthProviderService = oauthProviderService ?? throw new ArgumentNullException(nameof(oauthProviderService));
            _userRegistrationService = userRegistrationService ?? throw new ArgumentNullException(nameof(userRegistrationService));
            _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
            _bookmarkService = bookmarkService ?? throw new ArgumentNullException(nameof(bookmarkService));
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

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(new { message = "Search query parameter 'q' is required" });
            }

            try
            {
                var users = await _userService.SearchUsersAsync(q);
                var response = users.Select(u => new
                {
                    id = u.Id,
                    username = u.Username,
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    fullName = $"{u.FirstName} {u.LastName}".Trim(),
                    professionalTitle = u.ProfessionalTitle,
                    avatarUrl = u.AvatarUrl,
                    isActive = u.IsActive
                }).Where(u => u.isActive);

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the detailed error for debugging
                Console.WriteLine($"Error in SearchUsers: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                return BadRequest(new { 
                    message = ex.Message,
                    details = ex.InnerException?.Message,
                    type = ex.GetType().Name
                });
            }
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

        // Bookmark endpoints
        [HttpPost("{userId}/bookmarks")]
        public async Task<IActionResult> AddBookmark(Guid userId, [FromBody] AddBookmarkRequest request)
        {
            try
            {
                var response = await _bookmarkService.AddBookmarkAsync(userId, request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{userId}/bookmarks/{portfolioId}")]
        public async Task<IActionResult> RemoveBookmark(Guid userId, string portfolioId)
        {
            try
            {
                await _bookmarkService.RemoveBookmarkAsync(userId, portfolioId);
                return Ok(new { message = "Bookmark removed successfully." });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{userId}/bookmarks")]
        public async Task<IActionResult> GetUserBookmarks(Guid userId)
        {
            try
            {
                var response = await _bookmarkService.GetUserBookmarksAsync(userId);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{userId}/bookmarks/{portfolioId}/status")]
        public async Task<IActionResult> GetBookmarkStatus(Guid userId, string portfolioId)
        {
            try
            {
                var response = await _bookmarkService.GetBookmarkStatusAsync(userId, portfolioId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
    }
}
