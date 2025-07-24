using BackendMessages.Services;
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