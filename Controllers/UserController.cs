using BookingSystem.Data;
using BookingSystem.Models;
using BookingSystem.Models.DTOs;
using BookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;

namespace BookingSystem.Controllers
{
    /// <summary>
    /// Controller to display all the users
    /// </summary>
    public class UserController: Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Display all the user with their role, email and username
        /// </summary>
        /// <returns>The result of the action of displaying</returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var userRolesViewModel = new List<UserDTO>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRolesViewModel.Add(new UserDTO
                {
                    Username = user.UserName,
                    Email = user.Email,
                    Roles = roles
                });
            }
            return View(userRolesViewModel);
        }
    }
}
