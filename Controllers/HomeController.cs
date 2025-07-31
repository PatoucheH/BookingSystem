using BookingSystem.Models;
using BookingSystem.Models.ViewModels;
using BookingSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookingSystem.Controllers
{
    /// <summary>
    /// Controller to display the main page 
    /// </summary>
    public class HomeController : Controller
    {

        private readonly PropertyService? _propertyService;
        public HomeController(IPropertyService propertyService)
        {
            _propertyService = (PropertyService?)propertyService;
        }

        /// <summary>
        /// Display all the properties by the search arguments
        /// </summary>
        /// <returns>The result of the action of displaying</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var properties = await _propertyService.GetSearchProperties(null, null, null, null, null, null, null);
            var viewModel = new PropertySearchViewModel
            {
                Results = (IEnumerable<Models.DTOs.PropertyDTO>)properties
            };
            return View(viewModel);
        }

        /// <summary>
        /// Sort all the propertiers to display
        /// </summary>
        /// <param name="model">Arguments for the searching</param>
        /// <returns>The result of the action of searching</returns>
        [HttpPost]
        public async Task<ActionResult> Index(PropertySearchViewModel model)
        {
            var results = await _propertyService.GetSearchProperties
            (
                model.Country,
                model.Town,
                model.GuestNbr,
                model.Price,
                model.Type,
                model.StartDate,
                model.EndDate
            );

            model.Results = results;

            return View(model);
        }
    }
}
