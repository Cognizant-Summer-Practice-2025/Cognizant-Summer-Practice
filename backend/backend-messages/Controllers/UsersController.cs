using BackendMessages.Services;
using BackendMessages.Services.Abstractions;
using BackendMessages.Hubs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendMessages.Controllers
{
    [ApiController]
    [Route("api/users")]  // Explicit lowercase route
    public class UsersController : ControllerBase
    {
        private readonly IUserSearchService _userSearchService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserSearchService userSearchService, ILogger<UsersController> logger)
        {
            _userSearchService = userSearchService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the authenticated user ID from the current context.
        /// </summary>
        /// <returns>The authenticated user ID, or null if not authenticated.</returns>
        private Guid? GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }

        /// <summary>
        /// Simple test endpoint
        /// </summary>
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "Messages service is working!", timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Check if a user is currently online
        /// </summary>
        /// <param name="userId">The user ID to check</param>
        /// <returns>Online status of the user</returns>
        [HttpGet("{userId}/online-status")]
        public IActionResult GetUserOnlineStatus(string userId)
        {
            try
            {
                // Validate authenticated user
                var authenticatedUserId = GetAuthenticatedUserId();
                if (authenticatedUserId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest("User ID cannot be empty");
                }

                var isOnline = MessageHub.IsUserOnline(userId);
                
                _logger.LogInformation("Checked online status for user {UserId}: {IsOnline}", userId, isOnline);

                return Ok(new { 
                    userId = userId,
                    isOnline = isOnline,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking online status for user {UserId}", userId);
                return StatusCode(500, "An error occurred while checking user online status");
            }
        }



        /// <summary>
        /// Search for users by username, name, or professional title
        /// </summary>
        /// <param name="q">The search query term</param>
        /// <returns>List of matching users</returns>
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string q)
        {
            try
            {
                // Validate authenticated user
                var authenticatedUserId = GetAuthenticatedUserId();
                if (authenticatedUserId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                if (string.IsNullOrWhiteSpace(q))
                {
                    return BadRequest("Search query cannot be empty");
                }

                _logger.LogInformation("Received user search request: {Query}", q);

                var users = await _userSearchService.SearchUsersAsync(q);
                
                _logger.LogInformation("Found {Count} users for query: {Query}", users.Count, q);

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching users with query: {Query}", q);
                return StatusCode(500, "An error occurred while searching users");
            }
        }
    }
} 