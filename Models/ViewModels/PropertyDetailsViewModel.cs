using BookingSystem.Models.DTOs;
using BookingSystem.Models;

namespace BookingSystem.Models.ViewModels
{
    public class PropertyDetailsViewModel
    {
        public PropertyDTO Property { get; set; }
        public List<Booking> Bookings { get; set; } = new();
    }
}