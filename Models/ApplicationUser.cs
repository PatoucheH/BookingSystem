using Microsoft.AspNetCore.Identity;

namespace BookingSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public List<Property> Properties { get; set; } = [];
    }
}
