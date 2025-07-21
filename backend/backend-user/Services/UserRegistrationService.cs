using backend_user.DTO;
using backend_user.Models;
using backend_user.Repositories;

namespace backend_user.Services
{
    /// <summary>
    /// Implementation of user registration service.
    /// Follows Single Responsibility Principle by handling only registration operations.
    /// Follows Dependency Inversion Principle by depending on abstractions (interfaces).
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

        public async Task<object> RegisterOAuthUserAsync(RegisterOAuthUserRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Extracted logic from controller's register-oauth endpoint
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

            var username = request.Email.Split('@')[0];

            // Create user first
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

            var newUser = await _userRepository.CreateUser(user);

            // create OAuth provider
            var oauthRequest = new OAuthProviderCreateRequestDto
            {
                UserId = newUser.Id,
                Provider = request.Provider,
                ProviderId = request.ProviderId,
                ProviderEmail = request.ProviderEmail,
                AccessToken = request.AccessToken,
                RefreshToken = request.RefreshToken,
                TokenExpiresAt = request.TokenExpiresAt
            };

            var oauthProvider = await _oauthProviderRepository.CreateAsync(oauthRequest);

            var userResponse = new UserResponseDto
            {
                Id = newUser.Id,
                Email = newUser.Email,
                Username = newUser.Username,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                ProfessionalTitle = newUser.ProfessionalTitle,
                Bio = newUser.Bio,
                Location = newUser.Location,
                AvatarUrl = newUser.AvatarUrl,
                IsActive = newUser.IsActive,
                IsAdmin = newUser.IsAdmin,
                LastLoginAt = newUser.LastLoginAt
            };

            var oauthResponse = new OAuthProviderResponseDto
            {
                Id = oauthProvider.Id,
                UserId = oauthProvider.UserId,
                Provider = oauthProvider.Provider,
                ProviderId = oauthProvider.ProviderId,
                ProviderEmail = oauthProvider.ProviderEmail,
                TokenExpiresAt = oauthProvider.TokenExpiresAt,
                CreatedAt = oauthProvider.CreatedAt,
                UpdatedAt = oauthProvider.UpdatedAt
            };

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
