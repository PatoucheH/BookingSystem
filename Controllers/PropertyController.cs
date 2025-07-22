using BookingSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace BookingSystem.Controllers
{
    public class PropertyController : Controller
    {
        private readonly PropertyService _propertyService;

        public PropertyController (PropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        public async Task<IActionResult> Details(int id)
        {
            var property = await _propertyService.GetPropertyById(id);
            if (property == null) return NotFound();
            return View(property);
        }
    }
}
