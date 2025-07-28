using BackendMessages.Services;
using BackendMessages.Hubs;
using Microsoft.AspNetCore.Mvc;

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
        /// Simple test endpoint
        /// </summary>
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "Messages service is working!", timestamp = DateTime.Now });
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
        /// Debug endpoint to see all tracked connections (remove in production)
        /// </summary>
        [HttpGet("debug/connections")]
        public IActionResult GetDebugConnections()
        {
            try
            {
                var onlineUsers = MessageHub.GetOnlineUsers().ToList();
                
                _logger.LogInformation("Debug: Retrieved {Count} tracked users", onlineUsers.Count);

                return Ok(new {
                    trackedUsers = onlineUsers,
                    count = onlineUsers.Count,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving debug connections");
                return StatusCode(500, "An error occurred while retrieving debug connections");
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