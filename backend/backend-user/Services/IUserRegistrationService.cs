using backend_user.DTO;
using backend_user.Models;

namespace backend_user.Services
{
    /// <summary>
    /// Interface for user registration operations.
    /// Follows Single Responsibility Principle by focusing only on registration logic.
    /// </summary>
    public interface IUserRegistrationService
    {
        /// <summary>
        /// Registers a new user with standard registration.
        /// </summary>
        /// <param name="request">The user registration request</param>
        /// <returns>The created user</returns>
        Task<User> RegisterUserAsync(RegisterUserRequest request);
        
        /// <summary>
        /// Registers a new user with OAuth authentication.
        /// </summary>
        /// <param name="request">The OAuth user registration request</param>
        /// <returns>An object containing the created user and OAuth provider</returns>
        Task<object> RegisterOAuthUserAsync(RegisterOAuthUserRequest request);
        
        /// <summary>
        /// Validates if a user can be registered (email not taken, etc.).
        /// </summary>
        /// <param name="email">The email to validate</param>
        /// <returns>True if the user can be registered, false otherwise</returns>
        Task<bool> CanRegisterUserAsync(string email);
        
        /// <summary>
        /// Validates if an OAuth user can be registered.
        /// </summary>
        /// <param name="email">The email to validate</param>
        /// <param name="provider">The OAuth provider type</param>
        /// <param name="providerId">The provider-specific user ID</param>
        /// <returns>True if the user can be registered, false otherwise</returns>
        Task<bool> CanRegisterOAuthUserAsync(string email, OAuthProviderType provider, string providerId);
    }
}
