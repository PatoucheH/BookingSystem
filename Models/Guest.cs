namespace BookingSystem.Models
{
    public class Guest
    {
        internal int Id { get; set; }
        internal string? Username { get; set; } = string.Empty;
        internal string? Password { get; set; } = string.Empty;
        internal string? Email { get; set; } = string.Empty;
    }
}
