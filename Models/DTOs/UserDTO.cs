﻿namespace BookingSystem.Models.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string? Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
