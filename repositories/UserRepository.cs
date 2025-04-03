using GameAssetStorage.Data;
using GameAssetStorage.Models;
using Microsoft.EntityFrameworkCore;

namespace GameAssetStorage.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(
            ApplicationDbContext context,
            ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.username.ToLower() == username.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by username");
                throw;
            }
        }

        public async Task AddUser(User user)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.username == user.username))
                    throw new ArgumentException("Username already exists");

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error adding user");
                throw new Exception("Registration failed due to database error");
            }
        }
    }
}