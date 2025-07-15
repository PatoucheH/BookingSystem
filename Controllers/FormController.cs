using BookingSystem.Models;
using BookingSystem.Services;
using BookingSystem.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

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
            await _propertyService.CreateProperties(properties);

            return RedirectToAction("Index", "Home");
        }
    }
}
