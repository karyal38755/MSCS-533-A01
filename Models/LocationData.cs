using System.ComponentModel.DataAnnotations;

namespace LocationTrackerApp.Models
{
    /// <summary>
    /// Represents a location data point stored in the SQLite database.
    /// This model follows Entity Framework conventions for database mapping.
    /// </summary>
    public class LocationData
    {
        /// <summary>
        /// Unique identifier for the location record.
        /// Primary key in the database.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Latitude coordinate of the location.
        /// Range: -90 to 90 degrees.
        /// </summary>
        [Required]
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude coordinate of the location.
        /// Range: -180 to 180 degrees.
        /// </summary>
        [Required]
        public double Longitude { get; set; }

        /// <summary>
        /// Accuracy of the location measurement in meters.
        /// Lower values indicate higher accuracy.
        /// </summary>
        public double? Accuracy { get; set; }

        /// <summary>
        /// Altitude of the location in meters above sea level.
        /// Can be null if altitude is not available.
        /// </summary>
        public double? Altitude { get; set; }

        /// <summary>
        /// Speed of movement in meters per second.
        /// Can be null if speed is not available.
        /// </summary>
        public double? Speed { get; set; }

        /// <summary>
        /// Timestamp when the location was recorded.
        /// Stored in UTC format for consistency across time zones.
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Additional metadata or notes about the location.
        /// Optional field for user annotations.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Default constructor for Entity Framework.
        /// </summary>
        public LocationData()
        {
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Constructor with required parameters.
        /// </summary>
        /// <param name="latitude">Latitude coordinate</param>
        /// <param name="longitude">Longitude coordinate</param>
        /// <param name="accuracy">Location accuracy in meters</param>
        public LocationData(double latitude, double longitude, double? accuracy = null)
        {
            Latitude = latitude;
            Longitude = longitude;
            Accuracy = accuracy;
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Returns a string representation of the location data.
        /// </summary>
        /// <returns>Formatted string with location coordinates and timestamp</returns>
        public override string ToString()
        {
            return $"Location: {Latitude:F6}, {Longitude:F6} at {Timestamp:yyyy-MM-dd HH:mm:ss} UTC";
        }
    }
}
