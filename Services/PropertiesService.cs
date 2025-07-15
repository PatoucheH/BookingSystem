using BookingSystem.Data;
using BookingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Services
{

    public interface IPropertiesService
    {
        Task<IEnumerable<Properties>> GetAllProperties();
        Task<IEnumerable<Properties>> GetAllPropertiesByCountry(string country);
        Task<IEnumerable<Properties>> GetAllPropertiesByTown(string Town);
        Task<IEnumerable<Properties>> GetAllPropertiesByNbrGuest(int nbrGuest);
        Task<IEnumerable<Properties>> GetAllPropertiesByType(PropertiesType type);
    }
    public class PropertiesService(ContextDatabase context) : IPropertiesService
    {
        private readonly ContextDatabase _context = context;
        public async Task<IEnumerable<Properties>> GetAllProperties() 
        {
            return await _context.Properties.ToListAsync();
        }
        public async Task<IEnumerable<Properties>> GetAllPropertiesByCountry(string country)
        {
            return await _context.Properties.Where(p => p.Country == country).ToListAsync();
        }
        public async Task<IEnumerable<Properties>> GetAllPropertiesByTown(string town)
        {
            return await _context.Properties.Where(p => p.Town == town).ToListAsync();
        }
        public async Task<IEnumerable<Properties>> GetAllPropertiesByNbrGuest(int nbrGuest)
        {
            return await _context.Properties.Where(p => p.GuestNbr >= nbrGuest).ToListAsync();
        }
    }
}
