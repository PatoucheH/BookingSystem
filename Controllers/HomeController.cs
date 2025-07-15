using BookingSystem.Models.ViewModels;
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

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var properties = await _propertiesService.GetSearchProperties(null,null,null,null);
            var viewModel = new PropertiesSearchViewModel
            {
                Results = (IEnumerable<Models.DTOs.PropertiesSearchDTO>)properties
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> Index(PropertiesSearchViewModel model)
        {
            var results = await _propertiesService.GetSearchProperties
            (
                model.Country,
                model.Town,
                model.GuestNbr,
                model.Type
            );

            model.Results = results;

            return View(model);
        }
    }
}
