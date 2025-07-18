using backend_user.Models;
using backend_user.DTO;

namespace backend_user.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserById(Guid id);
        Task<User?> GetUserByEmail(string email);
        Task<List<User>> GetAllUsers();
        Task<User> CreateUser(User user);
        Task<User?> UpdateUser(Guid id, UpdateUserRequest request);
    }
}
