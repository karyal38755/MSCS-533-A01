using Microsoft.Maui.Controls.Maps;
using LocationTrackerApp.Models;
using LocationTrackerApp.Services;

namespace LocationTrackerApp.Controls
{
    /// <summary>
    /// Custom map view that displays location data as pins.
    /// This is a simplified version that shows location data as map pins.
    /// </summary>
    public class HeatMapView : Microsoft.Maui.Controls.Maps.Map
    {
        private readonly LocationDataService _locationDataService;

        /// <summary>
        /// Bindable property for controlling heat map visibility.
        /// </summary>
        public static readonly BindableProperty ShowHeatMapProperty =
            BindableProperty.Create(nameof(ShowHeatMap), typeof(bool), 
                typeof(HeatMapView), true, propertyChanged: OnShowHeatMapChanged);

        /// <summary>
        /// Bindable property for controlling heat map intensity.
        /// </summary>
        public static readonly BindableProperty HeatMapIntensityProperty =
            BindableProperty.Create(nameof(HeatMapIntensity), typeof(double), 
                typeof(HeatMapView), 1.0);

        /// <summary>
        /// Bindable property for controlling heat map radius.
        /// </summary>
        public static readonly BindableProperty HeatMapRadiusProperty =
            BindableProperty.Create(nameof(HeatMapRadius), typeof(double), 
                typeof(HeatMapView), 100.0);

        /// <summary>
        /// Bindable property for controlling heat map opacity.
        /// </summary>
        public static readonly BindableProperty HeatMapOpacityProperty =
            BindableProperty.Create(nameof(HeatMapOpacity), typeof(double), 
                typeof(HeatMapView), 0.6);

        /// <summary>
        /// Bindable property for controlling heat map color scheme.
        /// </summary>
        public static readonly BindableProperty HeatMapColorSchemeProperty =
            BindableProperty.Create(nameof(HeatMapColorScheme), typeof(HeatMapColorScheme), 
                typeof(HeatMapView), HeatMapColorScheme.RedToBlue);

        /// <summary>
        /// Event fired when the heat map data is updated.
        /// </summary>
        public event EventHandler? HeatMapUpdated;

        /// <summary>
        /// Gets or sets whether the heat map overlay is visible.
        /// </summary>
        public bool ShowHeatMap
        {
            get => (bool)GetValue(ShowHeatMapProperty);
            set => SetValue(ShowHeatMapProperty, value);
        }

        /// <summary>
        /// Gets or sets the intensity of the heat map visualization.
        /// </summary>
        public double HeatMapIntensity
        {
            get => (double)GetValue(HeatMapIntensityProperty);
            set => SetValue(HeatMapIntensityProperty, value);
        }

        /// <summary>
        /// Gets or sets the radius of each heat point in meters.
        /// </summary>
        public double HeatMapRadius
        {
            get => (double)GetValue(HeatMapRadiusProperty);
            set => SetValue(HeatMapRadiusProperty, value);
        }

        /// <summary>
        /// Gets or sets the opacity of the heat map overlay.
        /// </summary>
        public double HeatMapOpacity
        {
            get => (double)GetValue(HeatMapOpacityProperty);
            set => SetValue(HeatMapOpacityProperty, value);
        }

        /// <summary>
        /// Gets or sets the color scheme for the heat map.
        /// </summary>
        public HeatMapColorScheme HeatMapColorScheme
        {
            get => (HeatMapColorScheme)GetValue(HeatMapColorSchemeProperty);
            set => SetValue(HeatMapColorSchemeProperty, value);
        }

        /// <summary>
        /// Default constructor for XAML.
        /// </summary>
        public HeatMapView()
        {
            // This will be set via dependency injection or property setting
            _locationDataService = null!;
        }

        /// <summary>
        /// Initializes a new instance of the HeatMapView.
        /// </summary>
        /// <param name="locationDataService">Service for accessing location data</param>
        public HeatMapView(LocationDataService locationDataService)
        {
            _locationDataService = locationDataService ?? throw new ArgumentNullException(nameof(locationDataService));
        }

        /// <summary>
        /// Loads location data from the database and updates the map pins.
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task LoadHeatMapDataAsync()
        {
            try
            {
                var locationData = await _locationDataService.GetAllLocationDataAsync();
                UpdateMapPins(locationData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading heat map data: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads location data within a specific date range and updates the map pins.
        /// </summary>
        /// <param name="startDate">Start date for the data range</param>
        /// <param name="endDate">End date for the data range</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task LoadHeatMapDataByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var locationData = await _locationDataService.GetLocationDataByDateRangeAsync(startDate, endDate);
                UpdateMapPins(locationData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading heat map data by date range: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the map with new location data as pins.
        /// </summary>
        /// <param name="locationData">Collection of location data points</param>
        public void UpdateMapPins(IEnumerable<LocationData> locationData)
        {
            try
            {
                // Clear existing pins
                Pins.Clear();

                if (ShowHeatMap && locationData != null)
                {
                    var locations = locationData.ToList();
                    
                    // Add pins for each location
                    foreach (var location in locations)
                    {
                        var pin = new Pin
                        {
                            Label = $"Location {location.Id}",
                            Address = $"Recorded: {location.Timestamp:yyyy-MM-dd HH:mm:ss}",
                            Location = new Microsoft.Maui.Devices.Sensors.Location(location.Latitude, location.Longitude),
                            Type = PinType.Place
                        };
                        Pins.Add(pin);
                    }

                    // Auto-fit the map to show all pins
                    if (locations.Any())
                    {
                        var minLat = locations.Min(loc => loc.Latitude);
                        var maxLat = locations.Max(loc => loc.Latitude);
                        var minLng = locations.Min(loc => loc.Longitude);
                        var maxLng = locations.Max(loc => loc.Longitude);

                        var centerLat = (minLat + maxLat) / 2;
                        var centerLng = (minLng + maxLng) / 2;
                        var latDelta = Math.Max(maxLat - minLat, 0.01); // Minimum zoom level
                        var lngDelta = Math.Max(maxLng - minLng, 0.01);

                        var mapSpan = new Microsoft.Maui.Maps.MapSpan(
                            new Microsoft.Maui.Devices.Sensors.Location(centerLat, centerLng), 
                            latDelta, lngDelta);
                        
                        MoveToRegion(mapSpan);
                    }
                }

                OnHeatMapUpdated();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating map pins: {ex.Message}");
            }
        }

        /// <summary>
        /// Clears the heat map data.
        /// </summary>
        public void ClearHeatMapData()
        {
            try
            {
                Pins.Clear();
                OnHeatMapUpdated();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing heat map data: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the current number of data points in the heat map.
        /// </summary>
        /// <returns>Number of data points</returns>
        public int GetDataPointCount()
        {
            return Pins.Count;
        }

        /// <summary>
        /// Handles changes to the ShowHeatMap property.
        /// </summary>
        private static void OnShowHeatMapChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is HeatMapView heatMapView)
            {
                // Refresh the map when visibility changes
                heatMapView.OnHeatMapUpdated();
            }
        }

        /// <summary>
        /// Raises the HeatMapUpdated event.
        /// </summary>
        private void OnHeatMapUpdated()
        {
            HeatMapUpdated?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Enumeration of available color schemes for the heat map.
    /// </summary>
    public enum HeatMapColorScheme
    {
        /// <summary>
        /// Red to blue color scheme (hot to cold).
        /// </summary>
        RedToBlue,

        /// <summary>
        /// Red to yellow color scheme (hot to warm).
        /// </summary>
        RedToYellow,

        /// <summary>
        /// Blue to red color scheme (cold to hot).
        /// </summary>
        BlueToRed,

        /// <summary>
        /// Green to red color scheme (safe to danger).
        /// </summary>
        GreenToRed,

        /// <summary>
        /// Purple to orange color scheme.
        /// </summary>
        PurpleToOrange
    }
}