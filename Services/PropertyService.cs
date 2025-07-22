using BookingSystem.Data;
using BookingSystem.Models;
using BookingSystem.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Services
{

    public interface IPropertyService
    {
        Task<IEnumerable<Property>> GetAllProperties();
        Task<IEnumerable<PropertyDTO>> GetSearchProperties(string country, string town, int? guestNbr, double? price, PropertyType? type);

        Task<PropertyDTO> CreateProperty(PropertyDTO propertyDTO, string userId);
        Task<Property> GetPropertyById(int id);
    }
    public class PropertyService(ApplicationDbContext context) : IPropertyService
    {
        private readonly ApplicationDbContext _context = context;
        public async Task<IEnumerable<Property>> GetAllProperties()
        {
            return await _context.Properties.ToListAsync();
        }

        public async Task<IEnumerable<PropertyDTO>> GetSearchProperties(string country, string town, int? guestNbr, double? price, PropertyType? type)
        {
            var query = _context.Properties.AsQueryable();
            if (!string.IsNullOrWhiteSpace(country)) query = query.Where(p => p.Country == country);
            if (!string.IsNullOrWhiteSpace(town)) query = query.Where(p => p.Town == town);
            if (guestNbr.HasValue && guestNbr.Value > 0) query = query.Where(p => p.GuestNbr >= guestNbr.Value);
            if (price.HasValue && price.Value > 0) query = query.Where(p => p.Price <= price.Value);
            if (type.HasValue) query = query.Where(p => p.Type == type.Value);

            var result = await query
                .Select(p => new PropertyDTO
                {
                    Id = p.Id,
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

        public async Task<PropertyDTO> CreateProperty(PropertyDTO propertyDTO, string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException("User not connected !");
            var property = new Property
            {
                Town = propertyDTO.Town,
                Country = propertyDTO.Country,
                GuestNbr = propertyDTO.GuestNbr,
                Type = propertyDTO.Type,
                Description = propertyDTO.Description,
                Title = propertyDTO.Title,
                Price = propertyDTO.Price,
                Photo = propertyDTO.Photo,
                OwnerId = userId
            };

            _context.Properties.Add(property);
            await _context.SaveChangesAsync();
            return propertyDTO;
        }

        public async Task<Property> GetPropertyById(int id)
        {
            return await _context.Properties.FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
