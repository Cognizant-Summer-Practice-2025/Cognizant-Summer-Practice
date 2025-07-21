using backend_user.DTO;
using backend_user.Models;
using backend_user.Repositories;
using backend_user.Services.Abstractions;
using backend_user.Services.Mappers;
using backend_user.Services.Validators;

namespace backend_user.Services
{
    /// <summary>
    /// Implementation of user service.
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
            var validation = UserValidator.ValidateUserId(id);
            if (!validation.IsValid)
                throw new ArgumentException(string.Join(", ", validation.Errors));

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
            var validation = UserValidator.ValidateRegisterRequest(request);
            if (!validation.IsValid)
                throw new ArgumentException(string.Join(", ", validation.Errors));

            var existingUser = await _userRepository.GetUserByEmail(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User already exists");
            }

            var user = UserMapper.ToEntity(request);
            return await _userRepository.CreateUser(user);
        }

        public async Task<User?> UpdateUserAsync(Guid id, UpdateUserRequest request)
        {
            var userIdValidation = UserValidator.ValidateUserId(id);
            if (!userIdValidation.IsValid)
                throw new ArgumentException(string.Join(", ", userIdValidation.Errors));

            var requestValidation = UserValidator.ValidateUpdateRequest(request);
            if (!requestValidation.IsValid)
                throw new ArgumentException(string.Join(", ", requestValidation.Errors));

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

            return UserMapper.ToPortfolioInfo(user);
        }
    }
}
