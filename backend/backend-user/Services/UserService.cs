using backend_user.DTO;
using backend_user.Models;
using backend_user.Repositories;

namespace backend_user.Services
{
    /// <summary>
    /// Implementation of user service.
    /// Follows Single Responsibility Principle by handling only user management operations.
    /// Follows Dependency Inversion Principle by depending on abstractions (interfaces).
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(id));

            return await _userRepository.GetUserById(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            return await _userRepository.GetUserByEmail(email);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsers();
        }

        public async Task<User> CreateUserAsync(RegisterUserRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Extracted logic from controller's register endpoint
            var existingUser = await _userRepository.GetUserByEmail(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User already exists");
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

            return await _userRepository.CreateUser(user);
        }

        public async Task<User?> UpdateUserAsync(Guid id, UpdateUserRequest request)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(id));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return await _userRepository.UpdateUser(id, request);
        }

        public async Task<bool> UserExistsByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var user = await _userRepository.GetUserByEmail(email);
            return user != null;
        }

        public async Task<object?> GetUserPortfolioInfoAsync(Guid id)
        {
            var user = await GetUserByIdAsync(id);
            if (user == null)
                return null;

            return new
            {
                userId = user.Id,
                username = user.Username,
                name = $"{user.FirstName} {user.LastName}".Trim(),
                professionalTitle = user.ProfessionalTitle ?? "Portfolio Creator",
                location = user.Location ?? "Location not specified",
                avatarUrl = user.AvatarUrl
            };
        }
    }
}
