using BookingSystem.Data;
using BookingSystem.Models;
using BookingSystem.Models.DTOs;
using BookingSystem.Models.ViewModels;
using BookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var property = await _propertyService.GetPropertyDTOById(id);
            if (property == null) return NotFound();
            var bookings = await _context.Bookings
                .Where(b => b.PropertyId == id)
                .ToListAsync();
            var ratings = await _context.Ratings
                .Where(r => r.PropertyId == id)
                .Select(r => r.Value)
                .ToListAsync();

            property.AverageRating = ratings.Any() ? ratings.Average() : 0;
            var viewModel = new PropertyDetailsViewModel
            {
                Id = id,
                Property = property,
                Bookings = bookings
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Book(int propertyId, DateTime startDate, DateTime endDate)
        {
            var StartDateWithHours = startDate.Date.AddHours(16);
            var EndDateWithHours = endDate.Date.AddHours(11);
            var overlaping = await _context.Bookings
                .Where(b => b.PropertyId == propertyId &&
                        ((StartDateWithHours >= b.StartDate && StartDateWithHours < b.EndDate) ||
                        (EndDateWithHours > b.StartDate && EndDateWithHours <= b.EndDate)))
                .AnyAsync();

            if(overlaping)
            {
                TempData["Error"] = "The Selected Date are not available ";
                return RedirectToAction("Details", new { Id = propertyId });
            }

            var booking = new Booking
            {
                PropertyId = propertyId,
                StartDate = StartDateWithHours,
                EndDate = EndDateWithHours,
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
            var property = await _propertyService.GetPropertyDTOById(id);
            if (property == null) return NotFound();
            if (!User.IsInRole("Admin") && property.OwnerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                TempData["Error"] = "You are not authorized to edit this property.";
                return RedirectToAction("Details", "Property", new {id}); 
            }

            return View(property);
        }

        [Authorize(Roles = "Admin, Owner")]
        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] PropertyDTO propertyDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(propertyDTO);
            }

            var existingProperty = await _propertyService.GetPropertyById(propertyDTO.Id);
            if (existingProperty == null)
            {
                return NotFound();
            }

            existingProperty.Title = propertyDTO.Title;
            existingProperty.Description = propertyDTO.Description;
            existingProperty.Price = propertyDTO.Price;
            existingProperty.GuestNbr = propertyDTO.GuestNbr;

            await _propertyService.UpdateAsync(existingProperty);

            return RedirectToAction("Details","Property", new { id = existingProperty.Id });
        }

        [Authorize(Roles = "Admin, Owner")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _propertyService.DeleteProperty(id);
            if (!success)
            {
                TempData["Error"] = "Impossible to delete the property (maybe it's not yours)";
                return RedirectToAction("Details", "Property", new { id });
            }
            else
            {
                TempData["Success"] = "Property succesfully deleted !!";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Rate(int propertyId, int value, string message)
        {
            var rating = new Rating
            {
                Value = value,
                Message = message,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            var success = await _propertyService.RatingProperty(propertyId, rating);
            if (!success)
            {
                TempData["Error"] = "Impossible to add a rating ! (You maybe already add one or just never Book this property)";
            }
            else
            {
                TempData["Success"] = "Thanks for your rate !!";
            }

                return RedirectToAction("Details", new { id = propertyId });
        }
    }
}
