namespace BookingSystem.Models.DTOs
{
    public class BookingDTO
    {
        public string Title { get; set; }
        public string Town { get; set; }
        public string Country { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
