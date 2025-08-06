using BookingSystem.HubSignalR;
using BookingSystem.Models;
using BookingSystem.Models.StripeModels;
using BookingSystem.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;
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
            var connectionString = GetConnectionString(builder.Configuration, builder.Environment);
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

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
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<DbInitializer>>();

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                await WaitForDatabase(context, logger);
                await Initialize(context, userManager, roleManager);

                logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database initialization");
                throw;
            }
        }

        /// <summary>
        /// Wait for database to be ready with retry logic
        /// </summary>
        private static async Task WaitForDatabase(ApplicationDbContext context, ILogger logger)
        {
            var retry = 0;
            const int maxRetries = 30; 
            const int delayMs = 5000;

            while (retry < maxRetries)
            {
                try
                {
                    logger.LogInformation($"Attempting to connect to database (attempt {retry + 1}/{maxRetries})");

                    var canConnect = await context.Database.CanConnectAsync();
                    if (canConnect)
                    {
                        logger.LogInformation("Successfully connected to database");
                        return;
                    }
                    else
                        throw new Exception("Cannot connect to database - CanConnectAsync returned false");
                }
                catch (Exception ex)
                {
                    retry++;
                    logger.LogWarning($"Database connection attempt {retry} failed: {ex.Message}");

                    if (ex.InnerException != null)
                        logger.LogWarning($"Inner exception: {ex.InnerException.Message}");

                    if (retry >= maxRetries)
                    {
                        logger.LogError($"Failed to connect to database after {maxRetries} attempts");
                        throw new Exception($"Could not connect to database after {maxRetries} attempts. Last error: {ex.Message}", ex);
                    }

                    logger.LogInformation($"Waiting {delayMs}ms before retry...");
                    await Task.Delay(delayMs);
                }
            }
        }

        /// <summary>
        /// Initialize database with roles, users, and seed data
        /// </summary>
        public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            try
            {
                // Apply migrations
                Console.WriteLine("Applying database migrations...");
                await context.Database.MigrateAsync();
                Console.WriteLine("Migrations applied successfully");

                // Create roles
                string[] roles = new[] { "Admin", "Owner", "Guest" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        var result = await roleManager.CreateAsync(new IdentityRole(role));
                        if (result.Succeeded)
                            Console.WriteLine($"Role '{role}' created successfully");
                        else
                            Console.WriteLine($"Failed to create role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                    else
                        Console.WriteLine($"Role '{role}' already exists");
                }

                // Create users if user table is empty
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

                    // Create users
                    var user1Result = await userManager.CreateAsync(user1, "Hugo123!");
                    var user2Result = await userManager.CreateAsync(user2, "Martin123!");

                    if (user1Result.Succeeded && user2Result.Succeeded)
                    {
                        // Add roles to users
                        await userManager.AddToRoleAsync(user1, "Admin");
                        await userManager.AddToRoleAsync(user2, "Admin");

                        Console.WriteLine("Default admin users created successfully");

                        // Create test properties
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

                        Console.WriteLine("Test properties created successfully");
                    }
                    else
                        Console.WriteLine($"Failed to create users: {string.Join(", ", user1Result.Errors.Concat(user2Result.Errors).Select(e => e.Description))}");
                }
                else
                    Console.WriteLine("Users already exist, skipping user creation");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during database initialization: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get the appropriate connection string based on environment
        /// </summary>
        private static string GetConnectionString(IConfiguration configuration, IHostEnvironment environment)
        {
            try
            {
                var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD")
                    ?? throw new Exception("La variable d'environnement DB_PASSWORD est manquante.");

                var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
                var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
                var user = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
                var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "BookingDB";

                var connectionString = $"Host={host};Port={port};Database={dbName};Username={user};Password={dbPassword};Include Error Detail=true;";

                return connectionString;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error building connection string: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Configure Stripe settings
        /// </summary>
        private static void ConfigureStripe(WebApplicationBuilder builder)
        {
            var stripeSecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
            var stripePublishableKey = Environment.GetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY");

            if (string.IsNullOrEmpty(stripeSecretKey))
                throw new Exception("env variable STRIPE_SECRET_KEY is missing.");

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