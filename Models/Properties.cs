﻿using System.Text.Json.Serialization;

namespace BookingSystem.Models
{
    public class Properties
    {
        internal int Id { get; set; }
        public string? Town { get; set; }
        public string? Country { get; set; }
        public int? GuestNbr { get; set; }
        public PropertiesType Type { get; set; }
        public string? Description { get; set; }
        public string? Title { get; set; }
        public Double Price { get; set; }
        internal string? Photo { get; set; }
        internal string? OwnerId { get; set; }
        [JsonIgnore]
        internal virtual ApplicationUser? Owner { get; set; }

    }
}
