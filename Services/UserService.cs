using BookingSystem.Data;
using BookingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Services
{
    /// <summary>
    /// Defines methods for managing and retrieving user information within the application.
    /// </summary>
    public interface IUserService
    {
        Task<ApplicationUser?> GetUserByEmail(string email);
        Task<IEnumerable<ApplicationUser>> GetAllUsers();
        Task<ApplicationUser> CreateUser(ApplicationUser user, string password);
    }

    /// <summary>
    /// Provides methods for managing user accounts, including retrieving, creating, and listing users.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Retrieves a user associated with the specified email address.
        /// </summary>
        /// <param name="email">The email address of the user to retrieve. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user associated  with the
        /// specified email address, or <see langword="null"/> if no user is found.</returns>
        public async Task<ApplicationUser?> GetUserByEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        /// <summary>
        /// Retrieves all users from the underlying user store.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains an  IEnumerable{T} of
        /// ApplicationUser objects representing all users.</returns>
        public async Task<IEnumerable<ApplicationUser>> GetAllUsers()
        {
            return await _userManager.Users.ToListAsync();
        }

        /// <summary>
        /// Creates a new user in the system with the specified password.
        /// </summary>
        /// <param name="user">The <see cref="ApplicationUser"/> object representing the user to be created. This cannot be null.</param>
        /// <param name="password">The password to associate with the user. This must meet the password policy requirements.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created <see
        /// cref="ApplicationUser"/>.</returns>
        /// <exception cref="Exception">Thrown if the user creation fails. The exception message contains details about the failure.</exception>
        public async Task<ApplicationUser> CreateUser(ApplicationUser user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Échec de la création de l'utilisateur : {errors}");
            }
            return user;
        }
    }
}
