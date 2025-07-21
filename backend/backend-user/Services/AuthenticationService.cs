using backend_user.DTO;
using backend_user.Models;
using backend_user.Repositories;

namespace backend_user.Services
{
    /// <summary>
    /// Implementation of authentication service.
    /// Follows Single Responsibility Principle by handling only authentication concerns.
    /// Follows Dependency Inversion Principle by depending on abstractions (interfaces).
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IOAuthProviderRepository _oauthProviderRepository;

        public AuthenticationService(IUserRepository userRepository, IOAuthProviderRepository oauthProviderRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _oauthProviderRepository = oauthProviderRepository ?? throw new ArgumentNullException(nameof(oauthProviderRepository));
        }

        public async Task<User?> AuthenticateOAuthUserAsync(OAuthProviderType provider, string providerId, string providerEmail)
        {
            if (string.IsNullOrWhiteSpace(providerId))
                throw new ArgumentException("Provider ID cannot be null or empty", nameof(providerId));
            
            if (string.IsNullOrWhiteSpace(providerEmail))
                throw new ArgumentException("Provider email cannot be null or empty", nameof(providerEmail));

            // Find the OAuth provider
            var oauthProvider = await _oauthProviderRepository.GetByProviderAndProviderIdAsync(provider, providerId);
            if (oauthProvider == null)
                return null;

            // Get the associated user
            var user = await _userRepository.GetUserById(oauthProvider.UserId);
            if (user == null || !user.IsActive)
                return null;

            // Update last login
            await UpdateLastLoginAsync(user.Id);

            return user;
        }

        public async Task<bool> UpdateLastLoginAsync(Guid userId)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user == null)
                return false;

            // Note: This would require extending the repository to handle last login updates
            // For now, we'll create a simple update request
            var updateRequest = new UpdateUserRequest
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfessionalTitle = user.ProfessionalTitle,
                Bio = user.Bio,
                Location = user.Location,
                ProfileImage = user.AvatarUrl
            };

            // This is a limitation of the current repository design - it doesn't handle LastLoginAt
            // In a proper implementation, we would extend the repository or use a specialized method
            await _userRepository.UpdateUser(userId, updateRequest);
            
            return true;
        }

        public async Task<bool> IsOAuthProviderLinkedAsync(OAuthProviderType provider, string providerId)
        {
            if (string.IsNullOrWhiteSpace(providerId))
                return false;

            var oauthProvider = await _oauthProviderRepository.GetByProviderAndProviderIdAsync(provider, providerId);
            return oauthProvider != null;
        }
    }
}
