using LocationTrackerApp.Models;
using LocationTrackerApp.Services;
using LocationTrackerApp.Controls;

namespace LocationTrackerApp;

/// <summary>
/// Main page of the Location Tracker application.
/// Provides UI for location tracking and heat map visualization.
/// </summary>
public partial class MainPage : ContentPage
{
    private readonly LocationTrackingService _locationTrackingService;
    private readonly LocationDataService _locationDataService;
    private readonly Controls.SimpleMapView _heatMapView;
    private bool _isTracking = false;

    /// <summary>
    /// Initializes a new instance of the MainPage.
    /// </summary>
	public MainPage()
	{
		InitializeComponent();
        
        // Initialize services with null checks to prevent crashes
        try
        {
            // Get services from dependency injection
            _locationDataService = Application.Current?.Handler?.MauiContext?.Services?.GetService<LocationDataService>()!;
            _locationTrackingService = Application.Current?.Handler?.MauiContext?.Services?.GetService<LocationTrackingService>()!;

            // If services are not available, create them manually
            if (_locationDataService == null)
            {
                var context = new Models.LocationDbContext();
                _locationDataService = new Services.LocationDataService(context);
            }

            if (_locationTrackingService == null)
            {
                _locationTrackingService = new Services.LocationTrackingService(_locationDataService);
            }

            // Subscribe to location tracking events
            _locationTrackingService.TrackingStatusChanged += OnTrackingStatusChanged;
            _locationTrackingService.LocationUpdated += OnLocationUpdated;
            _locationTrackingService.ErrorOccurred += OnErrorOccurred;

            // Initialize the SimpleMapView with the service
            _heatMapView = new Controls.SimpleMapView(_locationDataService);
            MapContainer.Children.Add(_heatMapView);

            // Subscribe to heat map events
            _heatMapView.HeatMapUpdated += OnHeatMapUpdated;

            // Initialize UI
            InitializeUI();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing MainPage: {ex.Message}");
            // Show error to user
            DisplayAlert("Error", $"Failed to initialize app: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Initializes the UI components and loads initial data.
    /// </summary>
    private async void InitializeUI()
    {
        try
        {
            // Ensure database is initialized first
            await InitializeDatabaseAsync();
            
            // Load existing location data
            await LoadLocationData();
            
            // Update UI state
            UpdateUI();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to initialize: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Initializes the database to ensure it's ready for use.
    /// </summary>
    private async Task InitializeDatabaseAsync()
    {
        try
        {
            using var context = new Models.LocationDbContext();
            await context.EnsureDatabaseCreatedAsync();
            System.Diagnostics.Debug.WriteLine("Database initialized in MainPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing database in MainPage: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Handles the Start Tracking button click event.
    /// </summary>
    private async void OnStartTrackingClicked(object sender, EventArgs e)
    {
        try
        {
            await _locationTrackingService.StartTrackingAsync();
            UpdateStatusLabel("Location tracking started");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to start tracking: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Handles the Stop Tracking button click event.
    /// </summary>
    private void OnStopTrackingClicked(object sender, EventArgs e)
    {
        try
        {
            _locationTrackingService.StopTracking();
            UpdateStatusLabel("Location tracking stopped");
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to stop tracking: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Handles the Get Current Location button click event.
    /// </summary>
    private async void OnGetCurrentLocationClicked(object sender, EventArgs e)
    {
        try
        {
            UpdateStatusLabel("Getting current location...");
            var location = await _locationTrackingService.GetCurrentLocationAsync();
            
            if (location != null)
            {
                UpdateStatusLabel($"Current location: {location.Latitude:F6}, {location.Longitude:F6}");
                await LoadLocationData(); // Refresh the heat map
            }
            else
            {
                UpdateStatusLabel("Could not get current location");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to get current location: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Handles the Load Data button click event.
    /// </summary>
    private async void OnLoadDataClicked(object sender, EventArgs e)
    {
        try
        {
            await LoadLocationData();
            UpdateStatusLabel("Location data loaded");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Handles the Clear Data button click event.
    /// </summary>
    private async void OnClearDataClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await DisplayAlert("Confirm", 
                "Are you sure you want to clear all location data? This action cannot be undone.", 
                "Yes", "No");
            
            if (result)
            {
                await _locationDataService.ClearAllLocationDataAsync();
                _heatMapView.ClearHeatMapData();
                UpdateUI();
                UpdateStatusLabel("All location data cleared");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to clear data: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Handles the Toggle Heat Map button click event.
    /// </summary>
    private void OnToggleHeatMapClicked(object sender, EventArgs e)
    {
        try
        {
            _heatMapView.ShowHeatMap = !_heatMapView.ShowHeatMap;
            ToggleHeatMapButton.Text = _heatMapView.ShowHeatMap ? "Hide Heat Map" : "Show Heat Map";
            UpdateStatusLabel(_heatMapView.ShowHeatMap ? "Heat map shown" : "Heat map hidden");
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to toggle heat map: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Handles the Intensity slider value changed event.
    /// </summary>
    private void OnIntensityChanged(object sender, ValueChangedEventArgs e)
    {
        try
        {
            // SimpleMapView doesn't have heat map properties, just update the label
            IntensityLabel.Text = e.NewValue.ToString("F1");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating intensity: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the Radius slider value changed event.
    /// </summary>
    private void OnRadiusChanged(object sender, ValueChangedEventArgs e)
    {
        try
        {
            // SimpleMapView doesn't have heat map properties, just update the label
            RadiusLabel.Text = $"{e.NewValue:F0}m";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating radius: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the Opacity slider value changed event.
    /// </summary>
    private void OnOpacityChanged(object sender, ValueChangedEventArgs e)
    {
        try
        {
            // SimpleMapView doesn't have heat map properties, just update the label
            OpacityLabel.Text = e.NewValue.ToString("F1");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating opacity: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads location data from the database and updates the heat map.
    /// </summary>
    private async Task LoadLocationData()
    {
        try
        {
            await _heatMapView.LoadHeatMapDataAsync();
            UpdateUI();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading location data: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Updates the UI elements based on current state.
    /// </summary>
    private void UpdateUI()
    {
        try
        {
            var dataPointCount = _heatMapView.GetDataPointCount();
            DataPointCountLabel.Text = $"Data Points: {dataPointCount}";
            
            TrackingStatusLabel.Text = _isTracking ? "Status: Tracking" : "Status: Stopped";
            
            StartTrackingButton.IsEnabled = !_isTracking;
            StopTrackingButton.IsEnabled = _isTracking;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating UI: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the status label with a message.
    /// </summary>
    /// <param name="message">Status message to display</param>
    private void UpdateStatusLabel(string message)
    {
        try
        {
            StatusLabel.Text = message;
            System.Diagnostics.Debug.WriteLine($"Status: {message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating status label: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles location tracking status changes.
    /// </summary>
    private void OnTrackingStatusChanged(object? sender, bool isTracking)
    {
        try
        {
            _isTracking = isTracking;
            UpdateUI();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling tracking status change: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles location updates from the tracking service.
    /// </summary>
    private async void OnLocationUpdated(object? sender, LocationData locationData)
    {
        try
        {
            // Update the heat map with new data
            await LoadLocationData();
            
            UpdateStatusLabel($"Location updated: {locationData.Latitude:F6}, {locationData.Longitude:F6}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling location update: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles errors from the location tracking service.
    /// </summary>
    private void OnErrorOccurred(object? sender, string errorMessage)
    {
        try
        {
            UpdateStatusLabel($"Error: {errorMessage}");
            System.Diagnostics.Debug.WriteLine($"Location tracking error: {errorMessage}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling error event: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles heat map updates.
    /// </summary>
    private void OnHeatMapUpdated(object? sender, EventArgs e)
    {
        try
        {
            UpdateUI();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling heat map update: {ex.Message}");
        }
    }

    /// <summary>
    /// Cleanup when the page is disposed.
    /// </summary>
    protected override void OnDisappearing()
    {
        try
        {
            // Unsubscribe from events
            _locationTrackingService.TrackingStatusChanged -= OnTrackingStatusChanged;
            _locationTrackingService.LocationUpdated -= OnLocationUpdated;
            _locationTrackingService.ErrorOccurred -= OnErrorOccurred;
            _heatMapView.HeatMapUpdated -= OnHeatMapUpdated;
            
            // Stop tracking if active
            if (_isTracking)
            {
                _locationTrackingService.StopTracking();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during cleanup: {ex.Message}");
        }
        
        base.OnDisappearing();
	}
}

