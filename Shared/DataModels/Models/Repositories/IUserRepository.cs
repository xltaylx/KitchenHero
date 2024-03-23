using DataModels.Models;
using DataModels.Models.DbContext;
using Microsoft.EntityFrameworkCore;

namespace DataModels.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByUsername(string username);
    }

    public class UserRepository : IUserRepository
    {
        private readonly KitchenHeroDbContext _context;

        public UserRepository(KitchenHeroDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserByUsername(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}
