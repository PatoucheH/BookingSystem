using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Models
{
    public class Rating
    {
        public int Id { get; set; }
        [Range(1,5)]
        public int Value { get;set; }
        public string UserId { get; set; }
        public DateTime CreateAt { get; set; }
        public string Message { get; set; }
        public int PropertyId { get; set; }
        public Property Property { get; set; }
    }
}
