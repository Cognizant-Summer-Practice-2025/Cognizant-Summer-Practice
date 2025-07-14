namespace backend_user.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserById(string id);
        Task<User> GetUserByEmail(string email);
        Task<List<User>> GetAllUsers();
        Task<User> CreateUser(User user);
    }
}
