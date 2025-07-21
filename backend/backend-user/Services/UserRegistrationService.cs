using backend_user.DTO;
using backend_user.Models;
using backend_user.Repositories;
using backend_user.Services.Abstractions;
using backend_user.Services.Mappers;
using backend_user.Services.Validators;

namespace backend_user.Services
{
    /// <summary>
    /// Implementation of user registration service.
    /// </summary>
    public class UserRegistrationService : IUserRegistrationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IOAuthProviderRepository _oauthProviderRepository;

        public UserRegistrationService(IUserRepository userRepository, IOAuthProviderRepository oauthProviderRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _oauthProviderRepository = oauthProviderRepository ?? throw new ArgumentNullException(nameof(oauthProviderRepository));
        }

        public async Task<User> RegisterUserAsync(RegisterUserRequest request)
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

        public async Task<object> RegisterOAuthUserAsync(RegisterOAuthUserRequest request)
        {
            var validation = UserValidator.ValidateOAuthRegisterRequest(request);
            if (!validation.IsValid)
                throw new ArgumentException(string.Join(", ", validation.Errors));

            // Check if OAuth provider already exists
            var existingProvider = await _oauthProviderRepository.GetByProviderAndProviderIdAsync(
                request.Provider, request.ProviderId);
            
            if (existingProvider != null)
            {
                throw new InvalidOperationException("OAuth provider already linked to another user");
            }

            // Check if user already exists by email
            var existingUser = await _userRepository.GetUserByEmail(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User already exists");
            }

            // Create user first
            var user = UserMapper.ToEntity(request);
            var newUser = await _userRepository.CreateUser(user);

            // create OAuth provider
            var oauthRequest = OAuthProviderMapper.ToCreateRequest(request, newUser.Id);
            var oauthProvider = await _oauthProviderRepository.CreateAsync(oauthRequest);

            var userResponse = UserMapper.ToResponseDto(newUser);
            var oauthResponse = OAuthProviderMapper.ToResponseDto(oauthProvider);

            return new { user = userResponse, oauthProvider = oauthResponse };
        }

        public async Task<bool> CanRegisterUserAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var existingUser = await _userRepository.GetUserByEmail(email);
            return existingUser == null;
        }

        public async Task<bool> CanRegisterOAuthUserAsync(string email, OAuthProviderType provider, string providerId)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(providerId))
                return false;

            // Check if user already exists
            var existingUser = await _userRepository.GetUserByEmail(email);
            if (existingUser != null)
                return false;

            // Check if OAuth provider already exists
            var existingProvider = await _oauthProviderRepository.GetByProviderAndProviderIdAsync(provider, providerId);
            return existingProvider == null;
        }
    }
}
