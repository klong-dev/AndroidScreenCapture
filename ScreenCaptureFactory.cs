using System.Drawing;

namespace AndroidScreenCapture;

/// <summary>
/// Factory class for easy access to Android screenshot functionality using scrcpy protocol
/// </summary>
public static class ScreenCaptureFactory
{
    /// <summary>
    /// Creates a new device manager instance
    /// </summary>
    /// <param name="adbPath">Optional path to ADB executable</param>
    /// <param name="scrcpyServerPath">Optional path to scrcpy-server file</param>
    /// <returns>Device manager instance</returns>
    public static IDeviceManager CreateDeviceManager(string? adbPath = null, string? scrcpyServerPath = null)
    {
        return new DeviceManager(adbPath, scrcpyServerPath);
    }

    /// <summary>
    /// Quick method to capture screenshot from the first available device
    /// </summary>
    /// <param name="adbPath">Optional path to ADB executable</param>
    /// <param name="scrcpyServerPath">Optional path to scrcpy-server file</param>
    /// <returns>Screenshot bitmap or null if no devices or capture failed</returns>
    public static async Task<Bitmap?> CaptureScreenshotFromFirstDeviceAsync(string? adbPath = null, string? scrcpyServerPath = null)
    {
        var deviceManager = CreateDeviceManager(adbPath, scrcpyServerPath);
        
        var devices = await deviceManager.GetConnectedDevicesAsync();
        if (!devices.Any())
            return null;

        var device = deviceManager.GetDevice(devices.First());
        return device != null ? await device.CaptureScreenshotAsync() : null;
    }

    /// <summary>
    /// Quick method to capture screenshot from a specific device
    /// </summary>
    /// <param name="deviceSerial">Device serial number</param>
    /// <param name="adbPath">Optional path to ADB executable</param>
    /// <param name="scrcpyServerPath">Optional path to scrcpy-server file</param>
    /// <returns>Screenshot bitmap or null if device not found or capture failed</returns>
    public static async Task<Bitmap?> CaptureScreenshotFromDeviceAsync(string deviceSerial, string? adbPath = null, string? scrcpyServerPath = null)
    {
        if (string.IsNullOrWhiteSpace(deviceSerial))
            return null;

        var deviceManager = CreateDeviceManager(adbPath, scrcpyServerPath);
        var device = deviceManager.GetDevice(deviceSerial);
        
        return device != null ? await device.CaptureScreenshotAsync() : null;
    }

    /// <summary>
    /// Quick method to save screenshot from first available device to file
    /// </summary>
    /// <param name="filePath">Path where to save the screenshot</param>
    /// <param name="adbPath">Optional path to ADB executable</param>
    /// <param name="scrcpyServerPath">Optional path to scrcpy-server file</param>
    /// <returns>True if screenshot was saved successfully</returns>
    public static async Task<bool> SaveScreenshotFromFirstDeviceAsync(string filePath, string? adbPath = null, string? scrcpyServerPath = null)
    {
        var deviceManager = CreateDeviceManager(adbPath, scrcpyServerPath);
        
        var devices = await deviceManager.GetConnectedDevicesAsync();
        if (!devices.Any())
            return false;

        var device = deviceManager.GetDevice(devices.First());
        return device != null && await device.CaptureScreenshotToFileAsync(filePath);
    }
}
