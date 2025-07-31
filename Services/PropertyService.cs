using BookingSystem.Data;
using BookingSystem.Models;
using BookingSystem.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookingSystem.Services
{
    /// <summary>
    /// Defines a service for managing and retrieving property-related data.
    /// </summary>
    public interface IPropertyService
    {
        Task<IEnumerable<Property>> GetAllProperties();
        Task<IEnumerable<PropertyDTO>> GetSearchProperties(string country, string town, int? guestNbr, double? price, PropertyType? type, DateTime? startDate, DateTime? EndDate);

        Task<PropertyDTO> CreateProperty(PropertyDTO propertyDTO, string userId);
        Task<Property?> GetPropertyById(int id);
        Task UpdateAsync(Property property);
        Task<bool> RatingProperty(int id, Rating rating);
    }

    /// <summary>
    /// The main class to create get or delete some properties from the db or the user 
    /// </summary>
    /// <param name="context">Get access to the db </param>
    /// <param name="httpAccessor">get access to the actual auser which is connect </param>
    public class PropertyService(ApplicationDbContext context, IHttpContextAccessor httpAccessor) : IPropertyService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IHttpContextAccessor _httpAccessor = httpAccessor;
        public async Task<IEnumerable<Property>> GetAllProperties()
        {
            return await _context.Properties.ToListAsync();
        }

        /// <summary>
        /// Method to sort all the properties and returns a collection of properties searched
        /// </summary>
        /// <param name="country">Country where are the properties</param>
        /// <param name="town">City where are the properties</param>
        /// <param name="guestNbr">Number of max guest of the properties</param>
        /// <param name="price">Price per night</param>
        /// <param name="type">what's the properties are</param>
        /// <param name="startDate">Date of the first day </param>
        /// <param name="endDate">Date of the departure</param>
        /// <returns>a collection of all the properties which match with all the arguments</returns>
        public async Task<IEnumerable<PropertyDTO>> GetSearchProperties(string? country, string? town, int? guestNbr, double? price, PropertyType? type, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Properties.Include(p => p.Bookings).AsQueryable();
            if (!string.IsNullOrWhiteSpace(country)) query = query.Where(p => p.Country == country);
            if (!string.IsNullOrWhiteSpace(town)) query = query.Where(p => p.Town == town); 
            if (guestNbr.HasValue && guestNbr.Value > 0) query = query.Where(p => p.GuestNbr >= guestNbr.Value);
            if (price.HasValue && price.Value > 0) query = query.Where(p => p.Price <= price.Value);
            if (type.HasValue) query = query.Where(p => p.Type == type.Value);
            if (startDate.HasValue && endDate.HasValue)
            {
                var reservedPropertyIds = await _context.Bookings
                    .Where(b => b.StartDate < endDate && b.EndDate > startDate)
                    .Select(b => b.PropertyId)
                    .Distinct()
                    .ToListAsync();
                query = query.Where(p => !reservedPropertyIds.Contains(p.Id));
            }
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

        /// <summary>
        /// Create and add a new property to the db 
        /// </summary>
        /// <param name="propertyDTO">All the informations about the properties that we want to create and add get by a form</param>
        /// <param name="userId">The id of the user who create the property</param>
        /// <returns>The property create</returns>
        /// <exception cref="UnauthorizedAccessException">User is not connected return an error </exception>
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

        /// <summary>
        /// Get one property by its Id
        /// </summary>
        /// <param name="id">Id of the property we want to get </param>
        /// <returns>The property </returns>
        public async Task<Property?> GetPropertyById(int id)
        {
            return await _context.Properties.FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Get one property DTO by its id
        /// </summary>
        /// <param name="id">Id of the property wer want</param>
        /// <returns>REturn a property DTO</returns>
        public async Task<PropertyDTO?> GetPropertyDTOById(int id)
        {
            return await _context.Properties
                .Where(p => p.Id == id)
                .Select(p => new PropertyDTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Country = p.Country,
                    Town = p.Town,
                    Price = p.Price,
                    GuestNbr = p.GuestNbr,
                    Description = p.Description,
                    Photo = p.Photo,
                    Type = p.Type
                })
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Update the database with the new property add in the dbcontext
        /// </summary>
        /// <param name="property">property to addd</param>
        /// <returns>/</returns>
        public async Task UpdateAsync(Property property)
        {
            _context.Properties.Update(property);
            await _context.SaveChangesAsync();
        }


        /// <summary>
        /// Delete one property from the database 
        /// </summary>
        /// <param name="id">The id of the property we want to delete</param>
        /// <returns>True or false if the property get delete or not </returns>
        public async Task<bool> DeleteProperty(int id)
        {
            var property = await _context.Properties.Include(p => p.Bookings).FirstOrDefaultAsync(p => p.Id == id);
            if (property is null) return false;
            var user = _httpAccessor.HttpContext?.User;
            string? userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if(userId == null) return false;
            if (userId != property.OwnerId || !user.IsInRole("Admin")) return false;

            _context.Bookings.RemoveRange(property.Bookings);
            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();
            return true;
        }


        /// <summary>
        /// Rate a property 
        /// </summary>
        /// <param name="id">id of the property to rate</param>
        /// <param name="rating">the rate for the property with rating between 1 and 5 and message </param>
        /// <returns>true or false if the rating is succesfully added</returns>
        public async Task<bool> RatingProperty(int id, Rating rating)
        {
            var property = await _context.Properties.Include(p => p.Ratings).FirstOrDefaultAsync(p => p.Id == id);
            if (property is null) return false;
            if (rating.Value < 1 || rating.Value > 5) return false;

            var hasAlreadyRated = property.Ratings.Any(r => r.UserId == rating.UserId);
            if (hasAlreadyRated) return false;

            var hasBooked = await _context.Bookings.AnyAsync(b => b.PropertyId == id && b.UserId == rating.UserId);
            if (!hasBooked) return false;

            property.Ratings.Add(rating);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
