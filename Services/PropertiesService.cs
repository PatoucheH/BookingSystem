﻿using BookingSystem.Data;
using BookingSystem.Models;
using BookingSystem.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Services
{

    public interface IPropertiesService
    {
        Task<IEnumerable<Properties>> GetAllProperties();
        Task<IEnumerable<PropertiesDTO>> GetSearchProperties(string country, string town, int? guestNbr, double? price, PropertiesType? type);

        Task<PropertiesDTO> CreateProperties(PropertiesDTO propertiesDTO);
    }
    public class PropertiesService(ApplicationDbContext context) : IPropertiesService
    { 
        private readonly ApplicationDbContext _context = context;
        public async Task<IEnumerable<Properties>> GetAllProperties() 
        {
            return await _context.Properties.ToListAsync();
        }

        public async Task<IEnumerable<PropertiesDTO>> GetSearchProperties(string country, string town, int? guestNbr, double? price, PropertiesType? type)
        {
            var query = _context.Properties.AsQueryable();
            if (!string.IsNullOrWhiteSpace(country)) query = query.Where(p => p.Country == country);
            if (!string.IsNullOrWhiteSpace(town)) query = query.Where(p => p.Town == town);
            if (guestNbr.HasValue && guestNbr.Value > 0) query = query.Where(p => p.GuestNbr >= guestNbr.Value);
            if (price.HasValue && price.Value > 0) query = query.Where(p => p.Price >= price.Value);
            if (type.HasValue) query = query.Where(p => p.Type == type.Value);

            var result = await query
                .Select(p => new PropertiesDTO
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

        public async Task<PropertiesDTO> CreateProperties(PropertiesDTO propertiesDTO)
        {
            var properties = new Properties
            {
                Town = propertiesDTO.Town,
                Country = propertiesDTO.Country,
                GuestNbr = propertiesDTO.GuestNbr,
                Type = propertiesDTO.Type,
                Description = propertiesDTO.Description,
                Title = propertiesDTO.Title,
                Price = propertiesDTO.Price,
                Photo = propertiesDTO.Photo,
                OwnerId = "1"
            };

            _context.Properties.Add(properties);
            await _context.SaveChangesAsync();
            return propertiesDTO;
        }
    }
}
