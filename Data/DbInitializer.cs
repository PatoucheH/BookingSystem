using BookingSystem.HubSignalR;
using BookingSystem.Models;
using BookingSystem.Models.StripeModels;
using BookingSystem.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace BookingSystem.Data
{
    /// <summary>
    /// Centralized initializer for database, services, and application configuration.
    /// </summary>
    public class DbInitializer
    {
        /// <summary>
        /// Configure all services for the application
        /// </summary>
        public static async Task ConfigureServices(WebApplicationBuilder builder)
        {
            // Configuration database
            var connectionString = GetConnectionString(builder);
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Configuration Identity with rôles
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddHttpContextAccessor();

            // Saving services
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IPropertyService, PropertyService>();
            builder.Services.AddTransient<IEmailSender, FakeEmailSender>();

            // Configuration cookies
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            });

            // MVC & Razor Pages
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            // SignalR
            builder.Services.AddSignalR();
            builder.Services.AddSession();

            // Configuration Stripe
            ConfigureStripe(builder);
        }

        /// <summary>
        /// Configure the application pipeline
        /// </summary>
        public static void ConfigurePipeline(WebApplication app)
        {
            // Middleware configuration
            if (!app.Environment.IsDevelopment())
                app.UseHsts();
            else
                app.UseMigrationsEndPoint();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();

            // Routage
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();
            app.MapHub<BookingHub>("/bookingHub");
        }

        /// <summary>
        /// Complete application initialization including database setup
        /// </summary>
        public static async Task InitializeApplication(WebApplication app)
        {
            var connectionString = GetConnectionString(app);

            if (Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT") == null)
                await CreateDatabaseIfNotExists(connectionString);

            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            var retry = 0;
            const int maxRetries = 15;

            while (retry < maxRetries)
            {
                try
                {
                    var canConnect = await context.Database.CanConnectAsync();

                    if (canConnect)
                    {
                        await Initialize(context, userManager, roleManager);
                        break;
                    }
                    else
                        throw new Exception("Cannot connect to database");
                }
                catch (Exception ex)
                {
                    retry++;
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }

                    if (retry >= maxRetries)
                        throw;

                    await Task.Delay(10000);
                }
            }
        }

        /// <summary>
        /// Initialize database with roles, users, and seed data
        /// </summary>
        public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // apply migrations
            await context.Database.MigrateAsync();

            // Create roles
            string[] roles = new[] { "Admin", "Owner", "Guest" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // create users if user table is empty
            if (!await userManager.Users.AnyAsync())
            {
                var user1 = new ApplicationUser
                {
                    UserName = "hugo@admin.com",
                    Email = "hugo@admin.com",
                    EmailConfirmed = true
                };
                var user2 = new ApplicationUser
                {
                    UserName = "martin.admin",
                    Email = "martin@admin.com",
                    EmailConfirmed = true
                };

                // Créer users and roles
                await userManager.CreateAsync(user1, "Hugo123!");
                await userManager.CreateAsync(user2, "Martin123!");

                await userManager.AddToRoleAsync(user1, "Admin");
                await userManager.AddToRoleAsync(user2, "Admin");

                Console.WriteLine("Default admin users created");

                // create test property
                var properties = new Property[]
                {
                    new Property
                    {
                        Town = "Bruxelles",
                        Country = "Belgium",
                        Type = PropertyType.Hotel,
                        Description = "blablabla",
                        Title = "Beautiful hotel",
                        Price = 150,
                        GuestNbr = 2,
                        Photo = "/css/assets/Patou_logo.png",
                        OwnerId = user1.Id
                    },
                    new Property
                    {
                        Town = "Charleroi",
                        Country = "Belgium",
                        Type = PropertyType.Hotel,
                        Description = "lololol",
                        Title = "Big hotel",
                        Price = 100,
                        GuestNbr = 4,
                        Photo = "/css/assets/Patou_logo.png",
                        OwnerId = user1.Id
                    }
                };

                await context.Properties.AddRangeAsync(properties);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Create database if it doesn't exist (pour local et Docker uniquement)
        /// </summary>
        private static async Task CreateDatabaseIfNotExists(string connectionString)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog;

                builder.InitialCatalog = "master";
                var masterConnectionString = builder.ConnectionString;

                var retryCount = 0;
                const int maxRetries = 10;

                while (retryCount < maxRetries)
                {
                    try
                    {
                        using var connection = new SqlConnection(masterConnectionString);
                        await connection.OpenAsync();

                        var checkCommand = new SqlCommand($"SELECT COUNT(*) FROM sys.databases WHERE name = @dbName", connection);
                        checkCommand.Parameters.AddWithValue("@dbName", databaseName);

                        var exists = (int)await checkCommand.ExecuteScalarAsync() > 0;

                        if (!exists)
                        {
                            var createCommand = new SqlCommand($"CREATE DATABASE [{databaseName}]", connection);
                            await createCommand.ExecuteNonQueryAsync();
                        }
                        break;
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        Console.WriteLine($"Failed to create database (attempt {retryCount}/{maxRetries}): {ex.Message}");

                        if (retryCount >= maxRetries)
                        {
                            throw new Exception($"Failed to create database after {maxRetries} attempts", ex);
                        }
                        await Task.Delay(5000);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Get the appropriate connection string based on environment
        /// </summary>
        private static string GetConnectionString(WebApplicationBuilder builder)
        {
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD")
                         ?? throw new Exception("La variable d'environnement DB_PASSWORD est manquante.");

            if (builder.Environment.EnvironmentName == "Docker")
            {
                return $"Server=db,1433;Database=BookingDB;User Id=sa;Password={dbPassword};TrustServerCertificate=True;Encrypt=True;";
            }
            else
            {
                var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? throw new Exception("DefaultConnection string is missing.");

                return rawConnectionString.Replace("__DB_PASSWORD__", dbPassword);
            }
        }

        /// <summary>
        /// Get connection string from built application
        /// </summary>
        private static string GetConnectionString(WebApplication app)
        {
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD")
                         ?? throw new Exception("La variable d'environnement DB_PASSWORD est manquante.");

            if (app.Environment.EnvironmentName == "Docker")
            {
                return $"Server=db,1433;Database=BookingDB;User Id=sa;Password={dbPassword};TrustServerCertificate=True;Encrypt=True;";
            }

            var rawConnectionString = app.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new Exception("DefaultConnection string is missing.");

            return rawConnectionString.Replace("__DB_PASSWORD__", dbPassword);
        }

        /// <summary>
        /// Configure Stripe settings
        /// </summary>
        private static void ConfigureStripe(WebApplicationBuilder builder)
        {
            var stripeSecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
            var stripePublishableKey = Environment.GetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY");

            if (string.IsNullOrEmpty(stripeSecretKey))
                throw new Exception("La variable d'environnement STRIPE_SECRET_KEY est manquante.");

            // Configuration StripeSettings for DI
            builder.Services.Configure<StripeSettings>(options =>
            {
                options.SecretKey = stripeSecretKey;
                options.PublishableKey = stripePublishableKey;
            });

            // Configuration stripe
            StripeConfiguration.ApiKey = stripeSecretKey;
        }
    }
}