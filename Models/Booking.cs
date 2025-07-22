namespace BookingSystem.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public int PropertyId { get; set; }
        public Property Property { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate{ get; set; }
        public DateTime CreateAt{ get; set; } = DateTime.Now;
    }
}
