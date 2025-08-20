using backend_user.DTO.User.Request;
using backend_user.DTO.User.Response;
using backend_user.Models;
using backend_user.Repositories;
using backend_user.Services.Abstractions;
using backend_user.Services.Mappers;
using backend_user.Services.Validators;
using Microsoft.Extensions.Logging;

namespace backend_user.Services
{
    /// <summary>
    /// Implementation of user service.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));

            return await _userRepository.GetUserByUsername(username);
        }

        public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<User>();  
            try
            {
                searchTerm = searchTerm.Trim().ToLowerInvariant();

                var users = await _userRepository.SearchUsers(searchTerm);
                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new List<User>();
            }
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

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var validation = UserValidator.ValidateUserId(id);
            if (!validation.IsValid)
                throw new ArgumentException(string.Join(", ", validation.Errors));

            try
            {
                _logger.LogInformation("Starting cascade deletion for user {UserId}", id);

                try
                {
                    var portfolioServiceUrl = Environment.GetEnvironmentVariable("PORTFOLIO_SERVICE_URL") ?? "http://localhost:5201";
                    using var httpClient = new HttpClient();
                    var portfolioResponse = await httpClient.DeleteAsync($"{portfolioServiceUrl}/api/Portfolio/admin/user/{id}");
                    
                    if (portfolioResponse.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Successfully deleted portfolio data for user {UserId}", id);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to delete portfolio data for user {UserId}: {StatusCode}", id, portfolioResponse.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calling portfolio service to delete data for user {UserId}", id);
                }

                try
                {
                    var messagesServiceUrl = Environment.GetEnvironmentVariable("MESSAGES_SERVICE_URL") ?? "http://localhost:5003";
                    using var httpClient = new HttpClient();
                    var messagesResponse = await httpClient.DeleteAsync($"{messagesServiceUrl}/api/messages/admin/user/{id}");
                    
                    if (messagesResponse.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Successfully deleted message data for user {UserId}", id);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to delete message data for user {UserId}: {StatusCode}", id, messagesResponse.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calling messages service to delete data for user {UserId}", id);
                }

                var result = await _userRepository.DeleteUserAsync(id);
                
                if (result)
                {
                    _logger.LogInformation("Successfully completed cascade deletion for user {UserId}", id);
                }
                else
                {
                    _logger.LogWarning("User {UserId} not found during deletion", id);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cascade deletion for user {UserId}", id);
                throw;
            }
        }
    }
}
