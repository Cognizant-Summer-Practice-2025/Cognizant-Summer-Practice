using backend_user.Data;
using backend_user.Models;
using backend_user.DTO;
using Microsoft.EntityFrameworkCore;

namespace backend_user.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _context;
        public UserRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserById(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> CreateUser(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> UpdateUser(Guid id, UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return null;

            // Update only the fields that are provided
            if (request.FirstName != null)
                user.FirstName = request.FirstName;
            
            if (request.LastName != null)
                user.LastName = request.LastName;
            
            if (request.ProfessionalTitle != null)
                user.ProfessionalTitle = request.ProfessionalTitle;
            
            if (request.Bio != null)
                user.Bio = request.Bio;
            
            if (request.Location != null)
                user.Location = request.Location;
            
            if (request.ProfileImage != null)
                user.AvatarUrl = request.ProfileImage;

            await _context.SaveChangesAsync();
            return user;
        }
    }
}
