using Microsoft.Extensions.Logging;
using LocationTrackerApp.Models;
using LocationTrackerApp.Services;
using Microsoft.EntityFrameworkCore;

namespace LocationTrackerApp;

/// <summary>
/// Main entry point for configuring the MAUI application.
/// Sets up dependency injection, services, and application configuration.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Creates and configures the MAUI application.
    /// </summary>
    /// <returns>Configured MAUI application</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
        // Configure the main app
        builder
            .UseMauiApp<App>()
            .UseMauiMaps() // Enable maps functionality
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Configure logging
#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Configure Entity Framework and database services
        builder.Services.AddDbContext<LocationDbContext>(options =>
        {
            // Database path will be configured in the DbContext
            options.UseSqlite();
        });

        // Register application services
        builder.Services.AddScoped<LocationDataService>();
        builder.Services.AddScoped<LocationTrackingService>();

        // Register the main page
        builder.Services.AddTransient<MainPage>();

        return builder.Build();
    }
}
