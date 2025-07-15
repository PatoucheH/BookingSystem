using BookingSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly PropertiesService? _propertiesService;

        public HomeController(IPropertiesService propertiesService)
        {
            _propertiesService = (PropertiesService?)propertiesService;
        }

        public async Task<ActionResult> Index()
        {
            var properties = await _propertiesService.GetAllProperties();
            return View(properties);
        }
    }
}
