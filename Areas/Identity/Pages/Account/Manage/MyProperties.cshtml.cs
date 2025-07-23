using BookingSystem.Models.DTOs;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BookingSystem.Data; 

namespace BookingSystem.Areas.Identity.Pages.Account.Manage
{
    public class MyPropertiesModel : PageModel
    {
        private readonly ApplicationDbContext _context;  

        public MyPropertiesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<PropertyDTO> Properties { get; set; } = new List<PropertyDTO>();

        public async Task OnGetAsync()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                Properties = new List<PropertyDTO>();
                return;
            }

            Properties = await _context.Properties
                .Where(p => p.OwnerId == userId)
                .Select(p => new PropertyDTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Price = p.Price,
                    Town = p.Town,
                    Country = p.Country,
                    GuestNbr = p.GuestNbr
                })
                .ToListAsync();
        }
    }
}
