using BookingSystem.Data;
using BookingSystem.Models;
using BookingSystem.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Areas.Identity.Pages.Account.Manage
{
    public class MyBookingsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyBookingsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public List<BookingDTO> Bookings { get; set; }
        public async Task<IActionResult> OnGet()
        {
            var userId = _userManager.GetUserId(User);
            Bookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Property)
                .Select(b => new BookingDTO
                {
                    Title = b.Property.Title,
                    Town = b.Property.Town,
                    Country = b.Property.Country,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate

                })
                .ToListAsync();
            return Page();
        }
    }
}
