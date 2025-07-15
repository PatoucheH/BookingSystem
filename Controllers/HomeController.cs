using BookingSystem.Models;
using BookingSystem.Models.ViewModels;
using BookingSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly PropertiesService? _propertiesService;
        public HomeController(ILogger<HomeController> logger, IPropertiesService propertiesService)
        {
            _logger = logger;
            _propertiesService = (PropertiesService?)propertiesService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var properties = await _propertiesService.GetSearchProperties(null, null, null, null);
            var viewModel = new PropertiesSearchViewModel
            {
                Results = (IEnumerable<Models.DTOs.PropertiesDTO>)properties
            };
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
