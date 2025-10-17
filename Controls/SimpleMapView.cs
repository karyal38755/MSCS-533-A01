using Microsoft.Maui.Controls;
using LocationTrackerApp.Models;
using LocationTrackerApp.Services;

namespace LocationTrackerApp.Controls
{
    /// <summary>
    /// Simple map view that displays location data as a list instead of a map.
    /// This is a fallback when the Maps package is not available.
    /// </summary>
    public class SimpleMapView : ContentView
    {
        private readonly LocationDataService _locationDataService;
        private ListView _locationListView;
        private Label _statusLabel;

        /// <summary>
        /// Bindable property for controlling heat map visibility.
        /// </summary>
        public static readonly BindableProperty ShowHeatMapProperty =
            BindableProperty.Create(nameof(ShowHeatMap), typeof(bool), 
                typeof(SimpleMapView), true);

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
        /// Default constructor for XAML.
        /// </summary>
        public SimpleMapView()
        {
            _locationDataService = null!;
            InitializeUI();
        }

        /// <summary>
        /// Initializes a new instance of the SimpleMapView.
        /// </summary>
        /// <param name="locationDataService">Service for accessing location data</param>
        public SimpleMapView(LocationDataService locationDataService)
        {
            _locationDataService = locationDataService ?? throw new ArgumentNullException(nameof(locationDataService));
            InitializeUI();
        }

        /// <summary>
        /// Initializes the UI components.
        /// </summary>
        private void InitializeUI()
        {
            _statusLabel = new Label
            {
                Text = "Location Tracker - Simple View",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(10)
            };

            _locationListView = new ListView
            {
                ItemTemplate = new DataTemplate(() =>
                {
                    var cell = new ViewCell();
                    var stackLayout = new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Padding = new Thickness(10)
                    };

                    var locationLabel = new Label
                    {
                        FontSize = 12,
                        VerticalOptions = LayoutOptions.Center
                    };
                    locationLabel.SetBinding(Label.TextProperty, new Binding(".", converter: new LocationDataConverter()));

                    stackLayout.Children.Add(locationLabel);
                    cell.View = stackLayout;
                    return cell;
                })
            };

            var stackLayout = new StackLayout
            {
                Children = { _statusLabel, _locationListView }
            };

            Content = stackLayout;
        }

        /// <summary>
        /// Loads location data from the database and updates the list.
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task LoadHeatMapDataAsync()
        {
            try
            {
                if (_locationDataService != null)
                {
                    var locationData = await _locationDataService.GetAllLocationDataAsync();
                    _locationListView.ItemsSource = locationData;
                    _statusLabel.Text = $"Location Tracker - {locationData.Count} locations loaded";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading location data: {ex.Message}");
                _statusLabel.Text = $"Error loading data: {ex.Message}";
            }
        }

        /// <summary>
        /// Clears the location data.
        /// </summary>
        public void ClearHeatMapData()
        {
            try
            {
                _locationListView.ItemsSource = null;
                _statusLabel.Text = "Location Tracker - No data";
                OnHeatMapUpdated();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing location data: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the current number of data points.
        /// </summary>
        /// <returns>Number of data points</returns>
        public int GetDataPointCount()
        {
            return _locationListView.ItemsSource?.Cast<object>().Count() ?? 0;
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
    /// Converter for displaying LocationData in the list view.
    /// </summary>
    public class LocationDataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is LocationData locationData)
            {
                return $"Lat: {locationData.Latitude:F6}, Lng: {locationData.Longitude:F6} - {locationData.Timestamp:yyyy-MM-dd HH:mm:ss}";
            }
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
