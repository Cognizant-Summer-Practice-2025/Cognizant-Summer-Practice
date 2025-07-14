using backend_user.Models;
using backend_user.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_user.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUserRepository userRepository) : ControllerBase
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
            // Check if user already exists
            var existingUser = await userRepository.GetUserByEmail(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "User already exists" });
            }

            // Generate username from email if not provided
            var username = request.Username ?? request.Email.Split('@')[0];
            
            var user = new User
            {
                Email = request.Email,
                Username = username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                ProfessionalTitle = request.ProfessionalTitle,
                Bio = request.Bio,
                Location = request.Location,
                AvatarUrl = request.AvatarUrl
            };

            var newUser = await userRepository.CreateUser(user);
            return Ok(newUser);
        }
        
    }

    // DTO for registration
    public class RegisterUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfessionalTitle { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
