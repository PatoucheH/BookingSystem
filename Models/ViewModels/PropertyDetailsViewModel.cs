using BookingSystem.Models.DTOs;
using BookingSystem.Models;

namespace BookingSystem.Models.ViewModels
{
    /// <summary>
    /// Represents the view model for displaying property details, including property information and associated bookings.
    /// </summary>
    public class PropertyDetailsViewModel
    {
        public int Id { get; set; }
        public required PropertyDTO Property { get; set; }
        public required List<Booking> Bookings { get; set; } = new();
    }
}