using BookingSystem.Data;
using BookingSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookingSystem.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByEmail(string email);
        Task<IEnumerable<User>> GetAllUsers();
        Task<User> CreateUser(User user);
    }

    public class UserService(ContextDatabase context) : IUserService
    {
        private readonly ContextDatabase _context = context;
        
        public async Task<User?> GetUserByEmail(string email)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> CreateUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }

}
