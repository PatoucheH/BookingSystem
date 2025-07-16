using BookingSystem.Models.DTOs;

namespace BookingSystem.Models.ViewModels
{
    public class PropertiesSearchViewModel
    {
        public string? Country { get; set; }
        public string? Town { get; set; }
        public int? GuestNbr { get; set; }
        public double? Price { get; set; }
        public PropertiesType? Type { get; set; }

        public IEnumerable<PropertiesDTO> Results { get; set; } = new List<PropertiesDTO>();
    }
}
