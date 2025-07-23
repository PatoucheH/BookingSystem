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
    [Authorize(Roles = "Admin,Owner")]
    public class FormController : Controller
    {
        private readonly PropertyService? _propertyService;

        public FormController(IPropertyService? propertyService)
        {
            _propertyService = (PropertyService?)propertyService;
        }

        
        [HttpGet]
        public IActionResult Index()
        {
            return View(new PropertyDTO());
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromForm] PropertyDTO property)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            if(property.PhotoFile is not null && property.PhotoFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(property.PhotoFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using(var stream = new FileStream(filePath, FileMode.Create))
                {
                    await property.PhotoFile.CopyToAsync(stream);
                }
                property.Photo = "/uploads/" + uniqueFileName;
            }

            await _propertyService.CreateProperty(property, userId);
            return RedirectToAction("Index", "Home");
        }
    }
}
