# Android Screen Capture .NET Library

A .NET 8.0 class library for capturing screenshots from Android devices using the **scrcpy protocol and components**. This library extracts and utilizes the core screenshot functionality from **QtScrcpy**, leveraging the same underlying technologies:

- **ADB (Android Debug Bridge)** for device communication
- **scrcpy-server** JAR file for high-performance screen capture
- **FFmpeg libraries** for video processing (future enhancement)
- **QtScrcpy's proven architecture** adapted for .NET

## Features

- ✅ **Dual capture methods**: scrcpy protocol (high performance) + traditional screencap (fallback)
- ✅ **Uses actual QtScrcpy components**: ADB, scrcpy-server, and native libraries
- ✅ **Auto-detection**: Automatically finds ADB and scrcpy-server in application directory
- ✅ **Multiple device support** with device management
- ✅ **Multiple image formats** (PNG, JPEG, BMP, GIF)
- ✅ **Async/await pattern** for non-blocking operations
- ✅ **Factory methods** for simple usage
- ✅ **Based on proven QtScrcpy technology**
- ✅ Simple factory methods for quick usage
- ✅ Device management and detection
- ✅ Cross-platform .NET 8.0 compatibility (Windows focused)

## Prerequisites

- .NET 8.0 or later
- Android Debug Bridge (ADB) installed and accessible
- Android device with USB debugging enabled
- Device connected via USB or network

## Installation

1. Build the project:

   ```bash
   dotnet build
   ```

2. Reference the generated DLL in your project, or add as a project reference:

   ```xml
   <ProjectReference Include="path\to\AndroidScreenCapture.csproj" />
   ```

## Quick Start

### Simple Usage with Factory Methods

```csharp
using AndroidScreenCapture;
using System.Drawing;

// Capture from first available device
Bitmap? screenshot = await ScreenCaptureFactory.CaptureScreenshotFromFirstDeviceAsync();
if (screenshot != null)
{
    screenshot.Save("screenshot.png");
    screenshot.Dispose();
}

// Save directly to file
bool success = await ScreenCaptureFactory.SaveScreenshotFromFirstDeviceAsync("screenshot.png");
```

### Advanced Usage with Device Management

```csharp
using AndroidScreenCapture;

// Create device manager
IDeviceManager deviceManager = ScreenCaptureFactory.CreateDeviceManager();

// Check if ADB is available
if (!await deviceManager.IsAdbAvailableAsync())
{
    Console.WriteLine("ADB is not available!");
    return;
}

// Get connected devices
var devices = await deviceManager.GetConnectedDevicesAsync();
Console.WriteLine($"Found {devices.Count} connected devices");

foreach (var deviceSerial in devices)
{
    var device = deviceManager.GetDevice(deviceSerial);
    if (device != null && device.IsConnected)
    {
        Console.WriteLine($"Device: {device.Name} ({device.Serial})");
        
        // Capture screenshot
        using var screenshot = await device.CaptureScreenshotAsync();
        if (screenshot != null)
        {
            await device.CaptureScreenshotToFileAsync($"screenshot_{device.Serial}.png");
            Console.WriteLine($"Screenshot saved for {device.Name}");
        }
    }
}
```

### Custom ADB Path

```csharp
// If ADB is not in PATH, specify custom location
string adbPath = @"C:\path\to\platform-tools\adb.exe";
var deviceManager = ScreenCaptureFactory.CreateDeviceManager(adbPath);
```

## API Reference

### IDeviceManager Interface

- `GetConnectedDevicesAsync()` - Get list of connected device serial numbers
- `GetDevice(string serial)` - Get device instance by serial number
- `IsAdbAvailableAsync()` - Check if ADB is available

### IAndroidDevice Interface

- `Serial` - Device serial number
- `Name` - Device name/model
- `IsConnected` - Connection status
- `CaptureScreenshotAsync()` - Capture screenshot as Bitmap
- `CaptureScreenshotToFileAsync(string filePath)` - Save screenshot to file

### ScreenCaptureFactory Static Methods

- `CreateDeviceManager(string? adbPath = null)` - Create device manager
- `CaptureScreenshotFromFirstDeviceAsync(string? adbPath = null)` - Quick capture from first device
- `CaptureScreenshotFromDeviceAsync(string deviceSerial, string? adbPath = null)` - Quick capture from specific device
- `SaveScreenshotFromFirstDeviceAsync(string filePath, string? adbPath = null)` - Quick save from first device

## Supported Image Formats

The library automatically detects the output format based on file extension:

- `.png` - PNG format (default)
- `.jpg`, `.jpeg` - JPEG format
- `.bmp` - BMP format
- `.gif` - GIF format

## Error Handling

The library uses exceptions for error conditions:

- `InvalidOperationException` - For ADB/device communication errors
- `ArgumentException` - For invalid parameters
- Standard .NET exceptions for file operations

## Requirements for Android Device

1. Enable Developer Options:
   - Go to Settings > About Phone
   - Tap "Build Number" 7 times

2. Enable USB Debugging:
   - Go to Settings > Developer Options
   - Turn on "USB Debugging"

3. Connect device and authorize computer when prompted

## Building

```bash
# Restore packages
dotnet restore

# Build project
dotnet build

# Build release version
dotnet build -c Release
```

## Notes

- This library is designed primarily for Windows environments
- Requires ADB to be installed and accessible
- Screenshots are captured using Android's `screencap` command
- Temporary files are created and cleaned up automatically
- Memory usage is optimized with proper disposal patterns

## Based on QtScrcpy

This library extracts and adapts the screenshot functionality from the QtScrcpy project, focusing only on the core screen capture capabilities needed for .NET applications.
