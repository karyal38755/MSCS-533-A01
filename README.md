# Location Tracker App

A .NET MAUI application that tracks user location and displays it as a heat map visualization using SQLite database storage.

## Features

- **Location Tracking**: Continuously tracks user location with configurable accuracy and intervals
- **SQLite Database**: Stores location data locally with Entity Framework Core
- **Heat Map Visualization**: Displays location data as map pins with customizable settings
- **Real-time Updates**: Live location tracking with automatic map updates
- **Data Management**: Load, clear, and manage location data
- **Cross-platform**: Runs on Android, iOS, macOS, and Windows

## Architecture

### Project Structure

```
LocationTrackerApp/
├── Models/
│   ├── LocationData.cs          # Location data model
│   └── LocationDbContext.cs     # Entity Framework database context
├── Services/
│   ├── LocationDataService.cs   # Database operations service
│   └── LocationTrackingService.cs # GPS location tracking service
├── Controls/
│   └── HeatMapView.cs           # Custom map view with heat map functionality
├── Platforms/
│   ├── Android/
│   │   └── AndroidManifest.xml  # Android permissions
│   └── iOS/
│       └── Info.plist           # iOS permissions
├── MainPage.xaml                # Main UI layout
├── MainPage.xaml.cs             # Main page code-behind
├── MauiProgram.cs               # Application configuration and DI
└── App.xaml.cs                  # Application lifecycle
```

### Key Components

#### 1. LocationData Model
- Represents a location data point with coordinates, timestamp, and metadata
- Uses Entity Framework annotations for database mapping
- Includes validation for coordinate ranges

#### 2. LocationDbContext
- Entity Framework DbContext for SQLite database operations
- Configures entity relationships and constraints
- Provides database initialization and cleanup methods

#### 3. LocationDataService
- Service layer for database operations
- Provides CRUD operations for location data
- Includes batch operations and data filtering methods

#### 4. LocationTrackingService
- Handles GPS location tracking and permission management
- Configurable tracking parameters (accuracy, intervals, thresholds)
- Event-driven architecture for location updates

#### 5. HeatMapView
- Custom map control extending .NET MAUI Maps
- Displays location data as map pins
- Configurable visualization settings

## Permissions

### Android (AndroidManifest.xml)
```xml
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_BACKGROUND_LOCATION" />
<uses-permission android:name="android.permission.FOREGROUND_SERVICE" />
<uses-permission android:name="android.permission.FOREGROUND_SERVICE_LOCATION" />
```

### iOS (Info.plist)
```xml
<key>NSLocationWhenInUseUsageDescription</key>
<string>This app needs location access to track your position and create a heat map of your movements.</string>
<key>NSLocationAlwaysAndWhenInUseUsageDescription</key>
<string>This app needs location access to track your position and create a heat map of your movements.</string>
<key>NSLocationAlwaysUsageDescription</key>
<string>This app needs location access to track your position and create a heat map of your movements.</string>
```

## Dependencies

- **Microsoft.EntityFrameworkCore.Sqlite** (9.0.10) - SQLite database provider
- **Microsoft.Maui.Controls.Maps** (9.0.120) - Maps functionality
- **Microsoft.Maui.Controls** - Core MAUI controls
- **Microsoft.Extensions.Logging.Debug** - Debug logging

## Usage

### Starting Location Tracking
1. Launch the application
2. Grant location permissions when prompted
3. Tap "Start Tracking" to begin continuous location tracking
4. The app will automatically save location data to the SQLite database

### Viewing Location Data
1. Tap "Load Data" to display all stored location data on the map
2. Location points appear as pins on the map
3. The map automatically zooms to show all data points

### Managing Data
- **Get Current Location**: Records a single location point
- **Clear Data**: Removes all stored location data (with confirmation)
- **Toggle Heat Map**: Shows/hides location pins on the map

### Customizing Visualization
- **Intensity**: Adjusts the visual intensity of the heat map
- **Radius**: Controls the size of each heat point
- **Opacity**: Sets the transparency of the heat map overlay

## Database Schema

### LocationData Table
```sql
CREATE TABLE LocationData (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Latitude REAL NOT NULL,
    Longitude REAL NOT NULL,
    Accuracy REAL,
    Altitude REAL,
    Speed REAL,
    Timestamp TEXT NOT NULL,
    Notes TEXT
);
```

### Indexes
- `IX_LocationData_Timestamp` - For efficient time-based queries
- `IX_LocationData_Coordinates` - For spatial queries

## Best Practices Implemented

### Code Quality
- **Naming Conventions**: PascalCase for classes/methods, camelCase for variables
- **Documentation**: Comprehensive XML documentation for all public members
- **Error Handling**: Try-catch blocks with proper error logging
- **Resource Management**: Proper disposal of services and event handlers

### Architecture
- **Dependency Injection**: Services registered in MauiProgram.cs
- **Separation of Concerns**: Clear separation between UI, services, and data layers
- **Event-Driven**: Asynchronous operations with event notifications
- **Configuration**: Centralized configuration in MauiProgram.cs

### Performance
- **Batch Operations**: Efficient database operations for multiple records
- **Lazy Loading**: Database context created on demand
- **Memory Management**: Proper disposal of resources
- **Background Operations**: Non-blocking UI operations

## Building and Running

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or Visual Studio Code
- Platform-specific development tools (Android SDK, Xcode for iOS)

### Build Commands
```bash
# Restore packages
dotnet restore

# Build for specific platform
dotnet build --framework net8.0-android
dotnet build --framework net8.0-ios
dotnet build --framework net8.0-maccatalyst
dotnet build --framework net8.0-windows10.0.19041.0

# Run on specific platform
dotnet run --framework net8.0-maccatalyst
```

### Development Notes
- The app requires location permissions to function properly
- SQLite database is created automatically in the app's data directory
- Location tracking respects device battery optimization settings
- The app handles permission denials gracefully

## Troubleshooting

### Common Issues
1. **Location not updating**: Check location permissions and GPS settings
2. **Build errors**: Ensure all NuGet packages are restored
3. **Map not displaying**: Verify Maps package is properly installed
4. **Database errors**: Check file system permissions for app data directory

### Debug Information
- Enable debug logging in MauiProgram.cs
- Check device logs for location service errors
- Verify database file creation in app data directory

## Future Enhancements

- True heat map visualization with color gradients
- Export location data to various formats
- Location clustering for better performance
- Offline map support
- Location sharing and synchronization
- Advanced filtering and search capabilities

