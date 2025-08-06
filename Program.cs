using BookingSystem.Data;
using BookingSystem.HubSignalR;
using BookingSystem.Models;
using BookingSystem.Models.StripeModels;
using BookingSystem.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace BookingSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Loading .env only in local
            if (builder.Environment.IsDevelopment() && !builder.Environment.EnvironmentName.Equals("Docker"))
            {
                DotNetEnv.Env.Load();
            }

            // Configuration centralisée
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");

            // Configuration services via DbInitializer
            await DbInitializer.ConfigureServices(builder);

            var app = builder.Build();

            // Configuration du pipeline via DbInitializer
            DbInitializer.ConfigurePipeline(app);
             
            // Initialisation database via DbInitializer 
            await DbInitializer.InitializeApplication(app);

            if (app.Environment.IsDevelopment())
                app.Run();
            else
                app.Run("http://0.0.0.0:80");
        }
    }
}