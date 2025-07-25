using System.ComponentModel.DataAnnotations.Schema;

namespace BookingSystem.Models.DTOs
{
    public class PropertyDTO
    {
        public int Id { get; set; }
        public string? Town { get; set; }
        public string? Country { get; set; }
        public int? GuestNbr { get; set; }
        public PropertyType Type { get; set; }
        public Double Price { get; set; }
        public string? Description { get; set; }
        public string? Title { get; set; }
        public string? Photo { get; set; }
        public IFormFile? PhotoFile { get; set; }
        internal string? OwnerId { get; set; }
        public List<Rating> Ratings { get; set; } = new();
        [NotMapped]
        public double AverageRating { get; set; }
    }
}
