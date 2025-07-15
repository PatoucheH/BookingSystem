using BookingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Data
{
    public class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context)
        {
            await context.Database.EnsureCreatedAsync();

            if (await context.Users.AnyAsync())
                return;

            var users = new User[]
            {
                new User { Username = "Hugo", Email = "Hugo@example.com", Role = "Owner" },
                new User { Username = "Martin", Email = "Martin@example.com", Role = "Guest" },
                new User { Username = "Stephan", Email = "Stephan@example.com", Role = "Guest" },
                new User { Username = "Antoine", Email = "Antoine@example.com", Role = "Guest" },
                new User { Username = "Jordan", Email = "Jordan@example.com", Role = "Guest" }
            };
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            var properties = new Properties[]
            {
                new Properties
                {
                    Town ="Bx",
                    Country = "Belgium",
                    Type = PropertiesType.Hotel,
                    Description = "blablabla",
                    Title = "beautifull hotel",
                    Price = 150,
                    GuestNbr = 2,
                    Photo = "URl",
                    OwnerId = users[0].Id,
                    //Owner = users[0]
                },
                new Properties
                {
                    Town ="Charlouz",
                    Country = "Belgium",
                    Type = PropertiesType.Hotel,
                    Description = "lololol",
                    Title = "awful hotel",
                    Price = 10,
                    GuestNbr = 4,
                    Photo = "URl",
                    OwnerId = users[0].Id,
                    //Owner = users[0]
                }
            };

            await context.Properties.AddRangeAsync(properties);
            await context.SaveChangesAsync();
        }
    }
}
