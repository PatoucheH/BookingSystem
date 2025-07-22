using BookingSystem.Models;
using BookingSystem.Models.DTOs;
using BookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Security.Claims;

namespace BookingSystem.Controllers
{
    public class FormController : Controller
    {
        private readonly PropertyService? _propertyService;

        public FormController(IPropertyService? propertyService)
        {
            _propertyService = (PropertyService?)propertyService;
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpGet]
        public IActionResult Index()
        {
            return View(new PropertyDTO());
        }

        [Authorize(Roles = "Admin, Owner")]
        [HttpPost]
        public async Task<IActionResult> Index([FromForm] PropertyDTO property)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            await _propertyService.CreateProperty(property, userId);
            return RedirectToAction("Index", "Home");
        }
    }
}
