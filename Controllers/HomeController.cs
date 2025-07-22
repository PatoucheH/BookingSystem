using BookingSystem.Models;
using BookingSystem.Models.ViewModels;
using BookingSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookingSystem.Controllers
{
    public class HomeController : Controller
    {

        private readonly PropertyService? _propertyService;
        public HomeController(IPropertyService propertyService)
        {
            _propertyService = (PropertyService?)propertyService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var properties = await _propertyService.GetSearchProperties(null, null, null, null, null);
            var viewModel = new PropertySearchViewModel
            {
                Results = (IEnumerable<Models.DTOs.PropertyDTO>)properties
            };
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }



        [HttpPost]
        public async Task<ActionResult> Index(PropertySearchViewModel model)
        {
            var results = await _propertyService.GetSearchProperties
            (
                model.Country,
                model.Town,
                model.GuestNbr,
                model.Price,
                model.Type
            );

            model.Results = results;

            return View(model);
        }
    }
}
