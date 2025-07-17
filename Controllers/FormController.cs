using BookingSystem.Models;
using BookingSystem.Models.DTOs;
using BookingSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Security.Claims;

namespace BookingSystem.Controllers
{
    public class FormController : Controller
    {
        private readonly PropertiesService? _propertyService;

        public FormController(IPropertiesService? propertyService)
        {
            _propertyService = (PropertiesService?)propertyService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new PropertiesDTO());
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromForm] PropertiesDTO properties)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            await _propertyService.CreateProperties(properties, userId);
            return RedirectToAction("Index", "Home");
        }
    }
}
