using BookingSystem.Data;
using BookingSystem.Models;
using BookingSystem.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Services
{

    public interface IPropertiesService
    {
        Task<IEnumerable<Properties>> GetAllProperties();
        Task<IEnumerable<PropertiesSearchDTO>> GetSearchProperties(string country, string town, int? guestNbr, PropertiesType? type);
    }
    public class PropertiesService(ContextDatabase context) : IPropertiesService
    { 
        private readonly ContextDatabase _context = context;
        public async Task<IEnumerable<Properties>> GetAllProperties() 
        {
            return await _context.Properties.ToListAsync();
        }

        public async Task<IEnumerable<PropertiesSearchDTO>> GetSearchProperties(string country, string town, int? guestNbr, PropertiesType? type)
        {
            var query = _context.Properties.AsQueryable();
            if (!string.IsNullOrWhiteSpace(country)) query = query.Where(p => p.Country == country);
            if (!string.IsNullOrWhiteSpace(town)) query = query.Where(p => p.Town == town);
            if (guestNbr.HasValue && guestNbr.Value > 0) query = query.Where(p => p.GuestNbr >= guestNbr.Value);
            if (type.HasValue) query = query.Where(p => p.Type == type.Value);

            var result =await query
                .Select(p => new PropertiesSearchDTO
                {
                    Price = p.Price,
                    Title = p.Title,
                    Country = p.Country,
                    Town = p.Town,
                    GuestNbr = p.GuestNbr,
                    Type = p.Type,
                    Description = p.Description,
                    Photo = p.Photo
                })
                .ToListAsync();
            return result;
        }
    }
}
