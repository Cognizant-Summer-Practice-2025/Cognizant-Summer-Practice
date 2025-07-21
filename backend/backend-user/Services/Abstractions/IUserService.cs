using backend_user.DTO.User.Request;
using backend_user.DTO.User.Response;
using backend_user.Models;

namespace backend_user.Services.Abstractions
{
    /// <summary>
    /// Interface for user management operations.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The user ID</param>
        /// <returns>The user or null if not found</returns>
        Task<User?> GetUserByIdAsync(Guid id);
        
        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The email address</param>
        /// <returns>The user or null if not found</returns>
        Task<User?> GetUserByEmailAsync(string email);
        
        /// <summary>
        /// Retrieves all users in the system.
        /// </summary>
        /// <returns>A list of all users</returns>
        Task<IEnumerable<User>> GetAllUsersAsync();
        
        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="request">The user creation request</param>
        /// <returns>The created user</returns>
        Task<User> CreateUserAsync(RegisterUserRequest request);
        
        /// <summary>
        /// Updates an existing user.
        /// </summary>
        /// <param name="id">The user ID</param>
        /// <param name="request">The user update request</param>
        /// <returns>The updated user or null if not found</returns>
        Task<User?> UpdateUserAsync(Guid id, UpdateUserRequest request);
        
        /// <summary>
        /// Checks if a user exists by email.
        /// </summary>
        /// <param name="email">The email address</param>
        /// <returns>True if user exists, false otherwise</returns>
        Task<bool> UserExistsByEmailAsync(string email);
        
        /// <summary>
        /// Gets user portfolio information.
        /// </summary>
        /// <param name="id">The user ID</param>
        /// <returns>Portfolio information object or null if user not found</returns>
        Task<object?> GetUserPortfolioInfoAsync(Guid id);
    }
}
