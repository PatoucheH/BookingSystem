using BookingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Data
{
    public class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager )
        {
            await context.Database.MigrateAsync();

            Console.WriteLine("📦 Migration terminée");

            string[] roles = new[] { "Admin", "Owner", "Guest" };

            foreach(var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role)) await roleManager.CreateAsync(new IdentityRole(role));
            }

            if(!await userManager.Users.AnyAsync())
            {
                Console.WriteLine("👤 Création des utilisateurs...");
                var user1 = new ApplicationUser
                {
                    UserName = "hugo.admin",
                    Email = "hugo@admin.com",
                    EmailConfirmed = true
                };
                var user2 = new ApplicationUser
                {
                    UserName = "martin.admin",
                    Email = "martin@admin.com",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(user1, "Hugo123!");
                await userManager.CreateAsync(user2, "Martin123!");

                await userManager.AddToRoleAsync(user1, "Admin");
                await userManager.AddToRoleAsync(user2, "Admin");

                var properties = new Properties[]
                {
                     new Properties
                    {
                        Town = "Bx",
                        Country = "Belgium",
                        Type = PropertiesType.Hotel,
                        Description = "blablabla",
                        Title = "Beautiful hotel",
                        Price = 150,
                        GuestNbr = 2,
                        Photo = "/css/assets/Patou_logo.png",
                        OwnerId = user1.Id
                    },
                    new Properties
                    {
                        Town = "Charlouz",
                        Country = "Belgium",
                        Type = PropertiesType.Hotel,
                        Description = "lololol",
                        Title = "Awful hotel",
                        Price = 10,
                        GuestNbr = 4,
                        Photo = "/css/assets/Patou_logo.png",
                        OwnerId = user1.Id
                    }
                };
                await context.Properties.AddRangeAsync(properties);
                await context.SaveChangesAsync();
            }
        }
    }
}
