using BackendMessages.Models;

namespace BackendMessages.Services.Abstractions
{
    public interface IUserSearchService
    {
        Task<List<SearchUser>> SearchUsersAsync(string searchTerm);
        Task<SearchUser?> GetUserByIdAsync(Guid userId);
        Task<bool> IsUserOnlineAsync(Guid userId);
    }
} 