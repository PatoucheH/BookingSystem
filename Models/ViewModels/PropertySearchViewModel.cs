using BookingSystem.Models.DTOs;

namespace BookingSystem.Models.ViewModels
{
    /// <summary>
    /// Represents the view model for property search functionality, containing search criteria, results, and related
    /// bookings.
    /// </summary>
    public class PropertySearchViewModel
    {
        public int Id { get; set; }
        public string? Country { get; set; }
        public string? Town { get; set; }
        public int? GuestNbr { get; set; }
        public double? Price { get; set; }
        public PropertyType? Type { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public IEnumerable<PropertyDTO>? Results { get; set; } = new List<PropertyDTO>();
        public List<BookingDTO>? Bookings { get; set; } = new List<BookingDTO>();
    }
}
