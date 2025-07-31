using BookingSystem.Data;
using BookingSystem.HubSignalR;
using BookingSystem.Models;
using BookingSystem.Models.DTOs;
using BookingSystem.Models.ViewModels;
using BookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookingSystem.Controllers
{

    /// <summary>
    /// Controller which manage all about the properties 
    /// </summary>
    public class PropertyController : Controller
    {
        private readonly PropertyService _propertyService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<BookingHub> _hubContext;

        public PropertyController (PropertyService propertyService, ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHubContext<BookingHub> hubContext)
        {
            _propertyService = propertyService;
            _userManager = userManager;
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Retrieves the details of a property, including its bookings and average rating, and displays them in a view.
        /// </summary>
        /// <param name="id">The unique identifier of the property to retrieve.</param>
        /// <returns>An <see cref="IActionResult"/> that renders the property details view if the property is found; otherwise, a
        /// <see cref="NotFoundResult"/> if the property does not exist.</returns>
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


    /// <summary>
    /// Attempts to book a property for the specified date range.
    /// </summary>
    /// <param name="propertyId">The unique identifier of the property to be booked.</param>
    /// <param name="startDate">The start date of the booking. The time portion is adjusted to 4:00 PM.</param>
    /// <param name="endDate">The end date of the booking. The time portion is adjusted to 11:00 AM.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the booking operation.  If the booking is successful,
    /// redirects to the property details page with a success message.  If the booking fails due to overlapping dates,
    /// redirects to the property details page with an error message.</returns>
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

            await _hubContext.Clients.All.SendAsync("ReceiveNewBooking", new
            {
                propertyId = propertyId,
                start = StartDateWithHours.ToString("yyyy-MM-dd"),
                end = EndDateWithHours.ToString("yyyy-MM-dd")
            });

            TempData["Success"] = "Booking confirmed";
            return RedirectToAction("Details", new { id = propertyId });
        }


        /// <summary>
        /// Displays the edit view for a property with the specified ID.
        /// </summary>
        /// <param name="id">The unique identifier of the property to edit.</param>
        /// <returns>An <see cref="IActionResult"/> that renders the edit view if the property exists and the user is authorized;
        /// otherwise, a redirection to the property details page or a 404 Not Found result.</returns>
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


        /// <summary>
        /// Updates an existing property with the provided details.
        /// </summary>
        /// <param name="propertyDTO">An object containing the updated property details. The <see cref="PropertyDTO.Id"/> must correspond to an
        /// existing property.</param>
        /// <returns>An <see cref="IActionResult"/> that represents the result of the operation.  Returns a view with the
        /// provided <paramref name="propertyDTO"/> if the model state is invalid,  <see cref="NotFoundResult"/> if the
        /// property does not exist, or a redirect to the property details page upon successful update.</returns>
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


        /// <summary>
        /// Deletes a property with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the property to delete.</param>
        /// <returns>An <see cref="IActionResult"/> that redirects to the appropriate view based on the success of the operation.
        /// If the deletion is successful, redirects to the home page. Otherwise, redirects to the property details page
        /// with an error message.</returns>
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


        /// <summary>
        /// Submits a rating for a specified property.
        /// </summary>
        /// <param name="propertyId">The unique identifier of the property to be rated.</param>
        /// <param name="value">The rating value to assign to the property. Must be within the valid range defined by the system.</param>
        /// <param name="message">An optional message accompanying the rating, providing additional feedback.</param>
        /// <returns>An <see cref="IActionResult"/> that redirects to the property details page.  If the rating submission fails,
        /// an error message is added to <see cref="TempData"/>.</returns>
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
