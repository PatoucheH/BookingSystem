namespace BookingSystem.Models.DTOs
{
    public class UserDTO
    {
        public string? Username { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
    }
}
