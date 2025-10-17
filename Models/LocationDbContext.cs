using Microsoft.EntityFrameworkCore;
using System.IO;

namespace LocationTrackerApp.Models
{
    /// <summary>
    /// Entity Framework DbContext for managing location data in SQLite database.
    /// This class handles database operations and entity configurations.
    /// </summary>
    public class LocationDbContext : DbContext
    {
        /// <summary>
        /// Database set for location data records.
        /// Provides access to CRUD operations on location data.
        /// </summary>
        public DbSet<LocationData> LocationData { get; set; }

        /// <summary>
        /// Default constructor for dependency injection.
        /// </summary>
        public LocationDbContext()
        {
        }

        /// <summary>
        /// Constructor with DbContextOptions for configuration.
        /// </summary>
        /// <param name="options">Database context options</param>
        public LocationDbContext(DbContextOptions<LocationDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Configures the database connection and entity relationships.
        /// </summary>
        /// <param name="optionsBuilder">Options builder for database configuration</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Get the local application data path for SQLite database
                var databasePath = Path.Combine(FileSystem.AppDataDirectory, "location_tracker.db");
                
                // Configure SQLite connection
                optionsBuilder.UseSqlite($"Data Source={databasePath}");
            }
        }

        /// <summary>
        /// Configures entity relationships and constraints.
        /// </summary>
        /// <param name="modelBuilder">Model builder for entity configuration</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure LocationData entity
            modelBuilder.Entity<LocationData>(entity =>
            {
                // Set primary key
                entity.HasKey(e => e.Id);

                // Configure Id as auto-increment
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                // Configure required fields
                entity.Property(e => e.Latitude)
                    .IsRequired();

                entity.Property(e => e.Longitude)
                    .IsRequired();

                entity.Property(e => e.Timestamp)
                    .IsRequired();

                // Add constraints for latitude and longitude ranges
                entity.Property(e => e.Latitude)
                    .HasAnnotation("Range", new { Min = -90.0, Max = 90.0 });

                entity.Property(e => e.Longitude)
                    .HasAnnotation("Range", new { Min = -180.0, Max = 180.0 });

                // Create index on timestamp for efficient querying
                entity.HasIndex(e => e.Timestamp)
                    .HasDatabaseName("IX_LocationData_Timestamp");

                // Create composite index on coordinates for spatial queries
                entity.HasIndex(e => new { e.Latitude, e.Longitude })
                    .HasDatabaseName("IX_LocationData_Coordinates");
            });
        }

        /// <summary>
        /// Ensures the database is created and up to date.
        /// This method should be called during application startup.
        /// </summary>
        public async Task EnsureDatabaseCreatedAsync()
        {
            try
            {
                // Ensure the database is created
                await Database.EnsureCreatedAsync();
                
                // Verify the LocationData table exists
                var connection = Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }
                
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='LocationData'";
                var result = await command.ExecuteScalarAsync();
                
                if (result == null)
                {
                    System.Diagnostics.Debug.WriteLine("LocationData table not found, creating...");
                    // Force table creation by adding a dummy record and removing it
                    var dummyLocation = new LocationData(0, 0);
                    LocationData.Add(dummyLocation);
                    await SaveChangesAsync();
                    LocationData.Remove(dummyLocation);
                    await SaveChangesAsync();
                }
                
                System.Diagnostics.Debug.WriteLine("Database and tables verified successfully");
            }
            catch (Exception ex)
            {
                // Log the exception (in a real app, use proper logging)
                System.Diagnostics.Debug.WriteLine($"Error creating database: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Clears all location data from the database.
        /// Use with caution as this operation cannot be undone.
        /// </summary>
        public async Task ClearAllLocationDataAsync()
        {
            try
            {
                await Database.ExecuteSqlRawAsync("DELETE FROM LocationData");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing location data: {ex.Message}");
                throw;
            }
        }
    }
}
