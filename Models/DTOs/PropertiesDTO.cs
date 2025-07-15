namespace BookingSystem.Models.DTOs
{
    public class PropertiesDTO
    {
        public string? Town { get; set; }
        public string? Country { get; set; }
        public int? GuestNbr { get; set; }
        public PropertiesType Type { get; set; }
        public Double Price { get; set; }
        public string? Description { get; set; }
        public string? Title { get; set; }
        internal string? Photo { get; set; }
    }
}
