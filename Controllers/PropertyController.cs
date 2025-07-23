using BookingSystem.Data;
using BookingSystem.Models;
using BookingSystem.Models.ViewModels;
using BookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using SQLitePCL;

namespace BookingSystem.Controllers
{
    public class PropertyController : Controller
    {
        private readonly PropertyService _propertyService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public PropertyController (PropertyService propertyService, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _propertyService = propertyService;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Details(int id)
        {
            var property = await _propertyService.GetPropertyDTOById(id);
            if (property == null) return NotFound();
            var bookings = await _context.Bookings
                .Where(b => b.PropertyId == id)
                .ToListAsync();
            var viewModel = new PropertyDetailsViewModel
            {
                Property = property,
                Bookings = bookings
            };
            return View(viewModel);
        }

        [Authorize(Roles ="Admin,Owner")]
        [HttpPost]
        public async Task<IActionResult> Book(int propertyId, DateTime startDate, DateTime endDate)
        {
            var overlaping = await _context.Bookings
                .Where(b => b.PropertyId == propertyId &&
                        ((startDate >= b.StartDate && startDate < b.EndDate) ||
                        (endDate > b.StartDate && endDate <= b.EndDate)))
                .AnyAsync();

            if(overlaping)
            {
                TempData["Error"] = "The Selected Date are not available ";
                return RedirectToAction("Details", new { Id = propertyId });
            }

            var booking = new Booking
            {
                PropertyId = propertyId,
                StartDate = startDate,
                EndDate = endDate,
                UserId = _userManager.GetUserId(User)
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking confirmed";
            return RedirectToAction("Details", new { id = propertyId });
        }

        [Authorize(Roles = "Admin, Owner")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var property = await _propertyService.GetPropertyById(id);
            if (property == null) return NotFound();

            Console.WriteLine($"Edit GET => Property ID: {property.Id}");

            return View(property);
        }

        [Authorize(Roles = "Admin, Owner")]
        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] Property property)
        {
            if (!ModelState.IsValid)
            {
                return View(property);
            }

            var existingProperty = await _propertyService.GetPropertyById(property.Id);
            if (existingProperty == null)
            {
                return NotFound();
            }

            existingProperty.Title = property.Title;
            existingProperty.Description = property.Description;
            existingProperty.Price = property.Price;
            existingProperty.GuestNbr = property.GuestNbr;

            await _propertyService.UpdateAsync(existingProperty);

            return RedirectToAction("Details","Property", new { id = existingProperty.Id });
        }
    }
}
