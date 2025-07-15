using BookingSystem.Data;
using BookingSystem.Services;
using BookingSystem.Models;
using BookingSystem.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;

namespace BookingSystem.Controllers
{
    public class UserController: Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<ActionResult> Index()
        {
            var users = await _userService.GetAllUsers();
            return View(users);
        }


    }
}
