# AndroidScreenCapture Library User Guide

## Table of Contents
1. [Introduction](#introduction)
2. [System Requirements](#system-requirements)
3. [Build Project](#build-project)
4. [Copy Library to Other Projects](#copy-library-to-other-projects)
5. [Installation and Configuration](#installation-and-configuration)
6. [Detailed Usage Guide](#detailed-usage-guide)
7. [Real-world Examples](#real-world-examples)
8. [Error Handling](#error-handling)
9. [FAQ](#faq)

---

## Introduction

AndroidScreenCapture is a .NET library that allows you to easily capture screenshots from Android devices. The library supports two methods:

- **Standard method**: Capture individual screenshots
- **Persistent connection method**: Maintain connection with scrcpy server for faster repeated captures

## System Requirements

- .NET 8.0 or higher
- Windows (due to System.Drawing.Common usage)
- Android device with USB Debugging enabled
- ADB (Android Debug Bridge) - integrated in the library

## Build Project

### Step 1: Clone or Download source code

```bash
# If you have git
git clone [repository-url]

# Or download ZIP and extract
```

### Step 2: Build the library

```bash
# Open PowerShell/Command Prompt in the root directory
cd "c:\Users\user\Desktop\VSCode\.Net"

# Build project
dotnet build --configuration Release

# Or build specifically
dotnet build AndroidScreenCapture.csproj --configuration Release
```

### Step 3: Check build output

After successful build, files will be created at:

```
bin/Release/net8.0/
├── AndroidScreenCapture.dll      # Main library file
├── AndroidScreenCapture.xml      # Documentation XML
├── AndroidScreenCapture.pdb      # Debug symbols
├── dependencies/                 # Required dependencies
│   ├── adb.exe                   # ADB executable
│   ├── AdbWinApi.dll            # ADB Windows API
│   ├── AdbWinUsbApi.dll         # ADB USB API
│   └── scrcpy-server            # Scrcpy server file
└── [other dependencies]
```

## Copy Library to Other Projects

### Option 1: Manual Copy

#### Step 1: Create libs folder in target project

```
YourProject/
├── libs/
│   ├── AndroidScreenCapture.dll
│   ├── AndroidScreenCapture.xml
│   ├── adb.exe
│   ├── AdbWinApi.dll
│   ├── AdbWinUsbApi.dll
│   └── scrcpy-server
├── Program.cs
└── YourProject.csproj
```

#### Step 2: Copy required files

```bash
# Copy main DLL
copy "bin\Release\net8.0\AndroidScreenCapture.dll" "YourProject\libs\"

# Copy XML documentation (optional)
copy "bin\Release\net8.0\AndroidScreenCapture.xml" "YourProject\libs\"

# Copy required dependency files
copy "bin\Release\net8.0\dependencies\adb.exe" "YourProject\libs\"
copy "bin\Release\net8.0\dependencies\AdbWinApi.dll" "YourProject\libs\"
copy "bin\Release\net8.0\dependencies\AdbWinUsbApi.dll" "YourProject\libs\"
copy "bin\Release\net8.0\dependencies\scrcpy-server" "YourProject\libs\"
```

#### Step 3: Configure project file (.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <!-- Reference library -->
  <ItemGroup>
    <Reference Include="AndroidScreenCapture">
      <HintPath>libs\AndroidScreenCapture.dll</HintPath>
    </Reference>
  </ItemGroup>

  <!-- Copy required files to output -->
  <ItemGroup>
    <None Include="libs\adb.exe">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Include="libs\AdbWinApi.dll">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Include="libs\AdbWinUsbApi.dll">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Include="libs\scrcpy-server">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

  <!-- Dependencies -->
  <ItemGroup>
    <PackageReference Include="System.Drawing.Common" Version="8.0.0" />
    <PackageReference Include="FFMpegCore" Version="5.0.2" />
  </ItemGroup>

</Project>
```

### Option 2: Create NuGet Package (Advanced)

#### Step 1: Configure .csproj for NuGet

```xml
<PropertyGroup>
  <PackageId>AndroidScreenCapture</PackageId>
  <Version>1.0.0</Version>
  <Authors>Your Name</Authors>
  <Description>Android Screen Capture Library using QtScrcpy</Description>
  <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
</PropertyGroup>
```

#### Step 2: Build NuGet package

```bash
dotnet pack --configuration Release
```

#### Step 3: Install in other projects

```bash
dotnet add package AndroidScreenCapture --source "path/to/nupkg"
```

## Installation and Configuration

### 1. Add using statements

```csharp
using AndroidScreenCapture;
using System.Drawing;
```

### 2. Initialize the library

```csharp
// Create device manager
var deviceManager = ScreenCaptureFactory.CreateDeviceManager();

// Get list of devices
var devices = await deviceManager.GetConnectedDevicesAsync();

// Select first device
if (devices.Any())
{
    var device = deviceManager.GetDevice(devices.First());
    // Use device...
}
```

## Detailed Usage Guide

### 1. IDeviceManager Interface

#### `GetConnectedDevicesAsync()`

**Purpose**: Get list of all connected Android devices

```csharp
// Syntax
Task<IEnumerable<string>> GetConnectedDevicesAsync()

// Usage example
var deviceManager = ScreenCaptureFactory.CreateDeviceManager();
var devices = await deviceManager.GetConnectedDevicesAsync();

foreach (var deviceSerial in devices)
{
    Console.WriteLine($"Device found: {deviceSerial}");
}

// Sample output:
// Device found: emulator-5554
// Device found: ABC123DEF456
```

#### `GetDevice(string serial)`

**Purpose**: Get instance of specific device

```csharp
// Syntax
IAndroidDevice? GetDevice(string serial)

// Usage example
var device = deviceManager.GetDevice("emulator-5554");
if (device != null)
{
    Console.WriteLine($"Connected to device: {device.Name}");
}
else
{
    Console.WriteLine("Device not found");
}
```

### 2. IAndroidDevice Interface

#### Properties

##### `Serial` (string)

**Purpose**: Get device serial number

```csharp
string serial = device.Serial;
Console.WriteLine($"Serial: {serial}"); // Example: emulator-5554
```

##### `Name` (string)

**Purpose**: Get device display name

```csharp
string name = device.Name;
Console.WriteLine($"Device name: {name}"); // Example: Samsung Galaxy S21
```

##### `IsConnected` (bool)

**Purpose**: Check if device is connected via ADB

```csharp
if (device.IsConnected)
{
    Console.WriteLine("Device is connected");
}
else
{
    Console.WriteLine("Device is not connected");
}
```

##### `IsScrcpyConnected` (bool)

**Purpose**: Check if persistently connected to scrcpy server

```csharp
if (device.IsScrcpyConnected)
{
    Console.WriteLine("Connected to scrcpy server");
}
else
{
    Console.WriteLine("Not connected to scrcpy server");
}
```

#### Methods - Basic Screenshot Capture

##### `CaptureScreenshotAsync()`

**Purpose**: Capture a single screenshot (standard method)

```csharp
// Syntax
Task<Bitmap?> CaptureScreenshotAsync()

// Usage example
try
{
    using var screenshot = await device.CaptureScreenshotAsync();
    if (screenshot != null)
    {
        Console.WriteLine($"Screenshot captured: {screenshot.Width}x{screenshot.Height}");
        
        // Save image
        screenshot.Save("screenshot.png");
    }
    else
    {
        Console.WriteLine("Screenshot failed");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

##### `CaptureScreenshotToFileAsync(string filename)`

**Purpose**: Capture screenshot and save directly to file

```csharp
// Syntax
Task CaptureScreenshotToFileAsync(string filename)

// Usage example
try
{
    await device.CaptureScreenshotToFileAsync("my_screenshot.png");
    Console.WriteLine("Screenshot saved to my_screenshot.png");
}
catch (Exception ex)
{
    Console.WriteLine($"Error saving screenshot: {ex.Message}");
}

// Supported formats
await device.CaptureScreenshotToFileAsync("screenshot.png");  // PNG
await device.CaptureScreenshotToFileAsync("screenshot.jpg");  // JPEG
await device.CaptureScreenshotToFileAsync("screenshot.bmp");  // BMP
await device.CaptureScreenshotToFileAsync("screenshot.gif");  // GIF
```

#### Methods - Persistent Connection

##### `ConnectToScrcpyServerAsync()`

**Purpose**: Establish persistent connection to scrcpy server for faster captures

```csharp
// Syntax
Task<bool> ConnectToScrcpyServerAsync()

// Usage example
try
{
    bool connected = await device.ConnectToScrcpyServerAsync();
    if (connected)
    {
        Console.WriteLine("Connected to scrcpy server");
        Console.WriteLine($"Connection status: {device.IsScrcpyConnected}");
    }
    else
    {
        Console.WriteLine("Connection failed");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Connection error: {ex.Message}");
}
```

##### `CaptureScreenshotFromConnectedServerAsync()`

**Purpose**: Capture screenshot from persistent connection (faster for multiple consecutive captures)

```csharp
// Syntax
Task<Bitmap?> CaptureScreenshotFromConnectedServerAsync()

// Usage example
// Note: Must call ConnectToScrcpyServerAsync() first
if (device.IsScrcpyConnected)
{
    try
    {
        using var screenshot = await device.CaptureScreenshotFromConnectedServerAsync();
        if (screenshot != null)
        {
            Console.WriteLine($"Fast capture successful: {screenshot.Width}x{screenshot.Height}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fast capture error: {ex.Message}");
    }
}
else
{
    Console.WriteLine("Not connected to server. Call ConnectToScrcpyServerAsync() first");
}
```

##### `DisconnectFromScrcpyServerAsync()`

**Purpose**: Disconnect from scrcpy server

```csharp
// Syntax
Task DisconnectFromScrcpyServerAsync()

// Usage example
try
{
    await device.DisconnectFromScrcpyServerAsync();
    Console.WriteLine("Disconnected from server");
    Console.WriteLine($"Status: {device.IsScrcpyConnected}"); // False
}
catch (Exception ex)
{
    Console.WriteLine($"Disconnect error: {ex.Message}");
}
```

##### `Dispose()`

**Purpose**: Release resources (should be called when finished)

```csharp
// Usage example
device.Dispose(); // Or use using statement

// Better with using
using var device = deviceManager.GetDevice(deviceSerial);
// Automatically calls Dispose() when going out of scope
```

## Real-world Examples

### Example 1: Simple Screenshot

```csharp
using AndroidScreenCapture;
using System.Drawing;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Android Screenshot Capture ===");
        
        try
        {
            // Step 1: Create device manager
            var deviceManager = ScreenCaptureFactory.CreateDeviceManager();
            
            // Step 2: Get device list
            var devices = await deviceManager.GetConnectedDevicesAsync();
            
            if (!devices.Any())
            {
                Console.WriteLine("No Android devices found");
                return;
            }
            
            // Step 3: Select first device
            using var device = deviceManager.GetDevice(devices.First());
            if (device == null)
            {
                Console.WriteLine("Cannot connect to device");
                return;
            }
            
            Console.WriteLine($"Using device: {device.Name} ({device.Serial})");
            
            // Step 4: Capture screenshot
            using var screenshot = await device.CaptureScreenshotAsync();
            if (screenshot != null)
            {
                var filename = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                screenshot.Save(filename);
                Console.WriteLine($"Screenshot saved: {filename}");
                Console.WriteLine($"Resolution: {screenshot.Width}x{screenshot.Height}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
```

### Example 2: Multiple Screenshots with Persistent Connection

```csharp
using AndroidScreenCapture;
using System.Drawing;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Continuous Screenshot Capture ===");
        
        try
        {
            var deviceManager = ScreenCaptureFactory.CreateDeviceManager();
            var devices = await deviceManager.GetConnectedDevicesAsync();
            
            if (!devices.Any())
            {
                Console.WriteLine("No devices found");
                return;
            }
            
            using var device = deviceManager.GetDevice(devices.First());
            Console.WriteLine($"Device: {device.Name}");
            
            // Step 1: Connect to scrcpy server
            Console.WriteLine("Connecting to scrcpy server...");
            bool connected = await device.ConnectToScrcpyServerAsync();
            
            if (!connected)
            {
                Console.WriteLine("Connection failed, using standard method");
                return;
            }
            
            Console.WriteLine("Connection successful!");
            
            // Step 2: Capture multiple screenshots
            for (int i = 1; i <= 5; i++)
            {
                Console.WriteLine($"Capturing screenshot {i}/5...");
                
                try
                {
                    using var screenshot = await device.CaptureScreenshotFromConnectedServerAsync();
                    if (screenshot != null)
                    {
                        var filename = $"screenshot_{i:D2}_{DateTime.Now:HHmmss}.png";
                        screenshot.Save(filename);
                        Console.WriteLine($"  ✓ Saved: {filename}");
                    }
                    else
                    {
                        Console.WriteLine($"  ✗ Screenshot {i} failed");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ✗ Error screenshot {i}: {ex.Message}");
                }
                
                // Wait 1 second before next capture
                if (i < 5) await Task.Delay(1000);
            }
            
            // Step 3: Disconnect
            Console.WriteLine("Disconnecting server...");
            await device.DisconnectFromScrcpyServerAsync();
            Console.WriteLine("Complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        
        Console.ReadKey();
    }
}
```

### Example 3: Full Console Application

```csharp
using AndroidScreenCapture;
using System.Drawing;

class ScreenCaptureApp
{
    private IDeviceManager? _deviceManager;
    private IAndroidDevice? _currentDevice;
    
    static async Task Main(string[] args)
    {
        var app = new ScreenCaptureApp();
        await app.RunAsync();
    }
    
    async Task RunAsync()
    {
        Console.WriteLine("=== AndroidScreenCapture Console App ===");
        
        // Initialize
        _deviceManager = ScreenCaptureFactory.CreateDeviceManager();
        
        while (true)
        {
            try
            {
                await ShowMainMenuAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
            }
        }
    }
    
    async Task ShowMainMenuAsync()
    {
        Console.Clear();
        Console.WriteLine("=== MAIN MENU ===");
        Console.WriteLine("1. Scan devices");
        Console.WriteLine("2. Select device");
        Console.WriteLine("3. Single screenshot");
        Console.WriteLine("4. Multiple screenshots");
        Console.WriteLine("5. Connect scrcpy server");
        Console.WriteLine("6. Disconnect server");
        Console.WriteLine("7. Device info");
        Console.WriteLine("0. Exit");
        
        if (_currentDevice != null)
        {
            Console.WriteLine($"\nCurrent device: {_currentDevice.Name}");
            Console.WriteLine($"ADB connected: {_currentDevice.IsConnected}");
            Console.WriteLine($"Scrcpy connected: {_currentDevice.IsScrcpyConnected}");
        }
        
        Console.Write("\nSelect: ");
        var choice = Console.ReadLine();
        
        switch (choice)
        {
            case "1": await ScanDevicesAsync(); break;
            case "2": await SelectDeviceAsync(); break;
            case "3": await CaptureScreenshotAsync(); break;
            case "4": await CaptureMultipleAsync(); break;
            case "5": await ConnectServerAsync(); break;
            case "6": await DisconnectServerAsync(); break;
            case "7": ShowDeviceInfo(); break;
            case "0": Environment.Exit(0); break;
            default: 
                Console.WriteLine("Invalid choice");
                await Task.Delay(1000);
                break;
        }
    }
    
    async Task ScanDevicesAsync()
    {
        Console.WriteLine("Scanning devices...");
        var devices = await _deviceManager!.GetConnectedDevicesAsync();
        
        if (!devices.Any())
        {
            Console.WriteLine("No devices found");
        }
        else
        {
            Console.WriteLine($"Found {devices.Count()} device(s):");
            foreach (var device in devices)
            {
                Console.WriteLine($"  - {device}");
            }
        }
        
        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }
    
    async Task SelectDeviceAsync()
    {
        var devices = await _deviceManager!.GetConnectedDevicesAsync();
        
        if (!devices.Any())
        {
            Console.WriteLine("No devices available. Please connect a device first.");
            Console.ReadLine();
            return;
        }
        
        Console.WriteLine("Select device:");
        var deviceList = devices.ToList();
        for (int i = 0; i < deviceList.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {deviceList[i]}");
        }
        
        Console.Write("Enter number: ");
        if (int.TryParse(Console.ReadLine(), out int index) && 
            index > 0 && index <= deviceList.Count)
        {
            _currentDevice?.Dispose();
            _currentDevice = _deviceManager.GetDevice(deviceList[index - 1]);
            Console.WriteLine($"Selected: {_currentDevice?.Name}");
        }
        else
        {
            Console.WriteLine("Invalid choice");
        }
        
        Console.ReadLine();
    }
    
    async Task CaptureScreenshotAsync()
    {
        if (_currentDevice == null)
        {
            Console.WriteLine("Please select a device first");
            Console.ReadLine();
            return;
        }
        
        Console.WriteLine("Capturing screenshot...");
        
        var filename = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        await _currentDevice.CaptureScreenshotToFileAsync(filename);
        Console.WriteLine($"Saved: {filename}");
        
        Console.ReadLine();
    }
    
    async Task CaptureMultipleAsync()
    {
        if (_currentDevice == null)
        {
            Console.WriteLine("Please select a device first");
            Console.ReadLine();
            return;
        }
        
        Console.Write("Number of screenshots: ");
        if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
        {
            Console.WriteLine("Invalid number");
            Console.ReadLine();
            return;
        }
        
        Console.Write("Interval between screenshots (seconds): ");
        if (!int.TryParse(Console.ReadLine(), out int interval) || interval < 0)
        {
            interval = 1;
        }
        
        for (int i = 1; i <= count; i++)
        {
            Console.WriteLine($"Capturing screenshot {i}/{count}...");
            
            try
            {
                var filename = $"batch_{i:D3}_{DateTime.Now:HHmmss}.png";
                
                if (_currentDevice.IsScrcpyConnected)
                {
                    using var screenshot = await _currentDevice.CaptureScreenshotFromConnectedServerAsync();
                    screenshot?.Save(filename);
                }
                else
                {
                    await _currentDevice.CaptureScreenshotToFileAsync(filename);
                }
                
                Console.WriteLine($"  ✓ {filename}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ Error: {ex.Message}");
            }
            
            if (i < count && interval > 0)
            {
                await Task.Delay(interval * 1000);
            }
        }
        
        Console.WriteLine("Complete!");
        Console.ReadLine();
    }
    
    async Task ConnectServerAsync()
    {
        if (_currentDevice == null)
        {
            Console.WriteLine("Please select a device first");
            Console.ReadLine();
            return;
        }
        
        Console.WriteLine("Connecting to scrcpy server...");
        bool success = await _currentDevice.ConnectToScrcpyServerAsync();
        
        if (success)
        {
            Console.WriteLine("Connection successful!");
        }
        else
        {
            Console.WriteLine("Connection failed");
        }
        
        Console.ReadLine();
    }
    
    async Task DisconnectServerAsync()
    {
        if (_currentDevice == null)
        {
            Console.WriteLine("Please select a device first");
            Console.ReadLine();
            return;
        }
        
        Console.WriteLine("Disconnecting...");
        await _currentDevice.DisconnectFromScrcpyServerAsync();
        Console.WriteLine("Disconnected");
        Console.ReadLine();
    }
    
    void ShowDeviceInfo()
    {
        if (_currentDevice == null)
        {
            Console.WriteLine("No device selected");
        }
        else
        {
            Console.WriteLine("=== DEVICE INFO ===");
            Console.WriteLine($"Name: {_currentDevice.Name}");
            Console.WriteLine($"Serial: {_currentDevice.Serial}");
            Console.WriteLine($"ADB connected: {_currentDevice.IsConnected}");
            Console.WriteLine($"Scrcpy connected: {_currentDevice.IsScrcpyConnected}");
        }
        
        Console.ReadLine();
    }
}
```

## Error Handling

### Common Errors and Solutions

#### 1. "No devices found"

```csharp
// Causes and solutions
var devices = await deviceManager.GetConnectedDevicesAsync();
if (!devices.Any())
{
    Console.WriteLine("Check:");
    Console.WriteLine("1. Is device connected via USB?");
    Console.WriteLine("2. Is USB Debugging enabled?");
    Console.WriteLine("3. Are ADB drivers installed?");
    Console.WriteLine("4. Try 'adb devices' command in terminal");
}
```

#### 2. "Device not connected"

```csharp
if (!device.IsConnected)
{
    Console.WriteLine("Device not connected:");
    Console.WriteLine("- Try unplugging and reconnecting USB");
    Console.WriteLine("- Restart ADB: adb kill-server && adb start-server");
    Console.WriteLine("- Check USB Debugging");
}
```

#### 3. "Scrcpy server connection failed"

```csharp
try
{
    bool connected = await device.ConnectToScrcpyServerAsync();
    if (!connected)
    {
        Console.WriteLine("Scrcpy connection failed:");
        Console.WriteLine("- Device may not support scrcpy");
        Console.WriteLine("- Try using standard capture method");
        Console.WriteLine("- Check if port 27183 is blocked");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Connection error: {ex.Message}");
}
```

#### 4. "File access denied"

```csharp
try
{
    await device.CaptureScreenshotToFileAsync("screenshot.png");
}
catch (UnauthorizedAccessException)
{
    Console.WriteLine("No file write permission:");
    Console.WriteLine("- Check write permissions in directory");
    Console.WriteLine("- Run application as Administrator");
    Console.WriteLine("- Try writing to different directory");
}
catch (DirectoryNotFoundException)
{
    Console.WriteLine("Directory not found");
    Console.WriteLine("- Create directory before saving file");
}
```

### Comprehensive Error Handling Template

```csharp
async Task<bool> SafeCaptureScreenshot(IAndroidDevice device, string filename)
{
    try
    {
        // Check preconditions
        if (!device.IsConnected)
        {
            Console.WriteLine("❌ Device not connected");
            return false;
        }
        
        // Create directory if needed
        var directory = Path.GetDirectoryName(filename);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        // Capture screenshot
        await device.CaptureScreenshotToFileAsync(filename);
        Console.WriteLine($"✅ Saved: {filename}");
        return true;
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"❌ Device error: {ex.Message}");
        return false;
    }
    catch (UnauthorizedAccessException)
    {
        Console.WriteLine($"❌ No file write permission: {filename}");
        return false;
    }
    catch (DirectoryNotFoundException)
    {
        Console.WriteLine($"❌ Directory not found: {Path.GetDirectoryName(filename)}");
        return false;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Unknown error: {ex.Message}");
        return false;
    }
}
```

## FAQ

### Q: Why does the library need adb.exe, AdbWinApi.dll?

A: The library uses ADB (Android Debug Bridge) to communicate with Android devices. These files are required for ADB to function on Windows.

### Q: Can I capture screenshots without scrcpy server?

A: Yes, use `CaptureScreenshotAsync()` or `CaptureScreenshotToFileAsync()` methods. They use Android's screencap command.

### Q: Which method is faster?

A: Persistent connection (`CaptureScreenshotFromConnectedServerAsync()`) is faster for multiple consecutive captures since it doesn't need to establish new connections each time.

### Q: Does the library work on Linux/macOS?

A: Currently only supports Windows due to System.Drawing.Common usage. To support cross-platform, image processing needs to be changed.

### Q: How to capture high-quality screenshots?

A: Screenshot quality depends on device screen resolution. The library captures at device's native resolution.

### Q: Can it record video?

A: Currently only supports screenshots. Video recording would require additional FFmpeg implementation.

### Q: Why are screenshots sometimes slow?

A:
- Standard method: Needs to transfer files via ADB (slower)
- Persistent connection: Faster but requires initial setup
- USB speed and device performance also affect speed

### Q: Can I use with multiple devices simultaneously?

A: Yes, create multiple IAndroidDevice instances for different devices:

```csharp
var devices = await deviceManager.GetConnectedDevicesAsync();
var deviceInstances = devices.Select(serial => 
    deviceManager.GetDevice(serial)).ToList();

// Capture from all devices
var tasks = deviceInstances.Select(async device => 
{
    if (device != null)
    {
        await device.CaptureScreenshotToFileAsync($"{device.Serial}_screenshot.png");
    }
});

await Task.WhenAll(tasks);
```

---

## Conclusion

AndroidScreenCapture library provides a simple and efficient way to capture screenshots from Android devices. With two flexible capture methods, you can choose the approach that suits your needs:

- **Single capture**: Use `CaptureScreenshotAsync()` for occasional screenshots
- **Continuous capture**: Use persistent connection for multiple consecutive captures

Always remember to call `Dispose()` or use `using` statement to release resources after use.
