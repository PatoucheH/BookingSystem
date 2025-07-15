using BookingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Data
{
    public class DbInitializer
    {
        public static async Task Initialize(ContextDatabase context)
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
                    Photo = "URl",
                    OwnerId = 1,
                    Owner = users[0]
                },
                new Properties
                {
                    Town ="Charlouz",
                    Country = "Belgium",
                    Type = PropertiesType.Hotel,
                    Description = "lololol",
                    Title = "awful hotel",
                    Price = 10,
                    Photo = "URl",
                    OwnerId = 1,
                    Owner = users[0]
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }
    }
}
