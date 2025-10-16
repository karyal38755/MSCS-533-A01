using LocationTrackerApp.Models;

namespace LocationTrackerApp;

/// <summary>
/// Main application class for the Location Tracker app.
/// Handles application lifecycle and database initialization.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes a new instance of the App class.
    /// </summary>
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }

    /// <summary>
    /// Called when the application starts.
    /// Initializes the database and sets up the main page.
    /// </summary>
    protected override async void OnStart()
    {
        try
        {
            // Initialize the database
            await InitializeDatabaseAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing database: {ex.Message}");
        }
    }

    /// <summary>
    /// Initializes the SQLite database and ensures it's created.
    /// </summary>
    private async Task InitializeDatabaseAsync()
    {
        try
        {
            // Create database context directly
            using var context = new LocationDbContext();
            
            // Ensure the database is created and migrations are applied
            await context.Database.EnsureCreatedAsync();
            
            System.Diagnostics.Debug.WriteLine("Database initialized successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing database: {ex.Message}");
            // Don't throw - let the app continue
        }
    }
}
