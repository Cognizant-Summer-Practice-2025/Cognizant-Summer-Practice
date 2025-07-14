using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_user.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepository.GetAllUsers();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userRepository.GetUserById(id);
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(User user)
        {
            var newUser = await _userRepository.CreateUser(user);
            return Ok(newUser);
        }
        
    }
}
