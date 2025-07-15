using BookingSystem.Models;
using BookingSystem.Services;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Controllers
{
        [ApiController]
        [Route("api/[controller]")]
        public class AuthController(IUserService userService, IAuthService authService) : ControllerBase
        {
            [HttpPost("register/guest")]
            public async Task<ActionResult> Register(LoginRequest request)
            {
            var existingUser = await userService.GetAllUsers();
            if (existingUser.Any(u => u.Email == request.Email)) return BadRequest("User already exists");

                var user = new User
                {
                    Email = request.Email,
                    Username = request.Email.Split('@')[0],
                    Password = authService.HashPassword(request.Password)
                };

            await userService.CreateUser(user);
            return Ok("User registered");
            }


            [HttpPost("login")]
            public async Task<ActionResult> Login(LoginRequest request)
            {
                var users = await userService.GetAllUsers();
                var user = users.FirstOrDefault(u => u.Email == request.Email);
                if (user is null) return Unauthorized("Invalid Credentials");
                if(!authService.VerifyPassword(user, request.Password)) return Unauthorized("Invalid Credentials ! !");

                var token = authService.GenerateJwtToken(user);
                return Ok(new { Token = token });

            }
        }
}
