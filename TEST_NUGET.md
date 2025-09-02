# Test NuGet Package Locally

## 1. Create test project
```bash
mkdir TestNuGetPackage
cd TestNuGetPackage
dotnet new console
```

## 2. Add local package source
```bash
dotnet nuget add source "C:\Users\user\Desktop\VSCode\.Net\nupkg" --name "LocalPackages"
```

## 3. Install the package
```bash
dotnet add package AndroidScreenCapture --version 1.0.0 --source "LocalPackages"
```

## 4. Test the package
Add this code to Program.cs:

```csharp
using AndroidScreenCapture;

Console.WriteLine("Testing AndroidScreenCapture NuGet Package");

try
{
    var deviceManager = ScreenCaptureFactory.CreateDeviceManager();
    var devices = await deviceManager.GetConnectedDevicesAsync();
    
    Console.WriteLine($"Found {devices.Count()} devices");
    
    if (devices.Any())
    {
        var success = await ScreenCaptureFactory.SaveScreenshotFromFirstDeviceAsync("test_screenshot.png");
        Console.WriteLine($"Screenshot captured: {success}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

## 5. Build and run
```bash
dotnet build
dotnet run
```

## 6. Check dependencies are copied
The following files should be in the output directory:
- adb.exe
- AdbWinApi.dll  
- AdbWinUsbApi.dll
- scrcpy-server
