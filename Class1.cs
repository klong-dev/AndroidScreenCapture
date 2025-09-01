using System.Drawing;

namespace AndroidScreenCapture;

/// <summary>
/// Interface for Android device management and screenshot capture
/// </summary>
public interface IDeviceManager
{
    /// <summary>
    /// Gets a list of connected Android devices
    /// </summary>
    /// <returns>List of device serial numbers</returns>
    Task<List<string>> GetConnectedDevicesAsync();

    /// <summary>
    /// Gets an Android device instance by serial number
    /// </summary>
    /// <param name="serial">Device serial number</param>
    /// <returns>Android device instance</returns>
    IAndroidDevice? GetDevice(string serial);

    /// <summary>
    /// Checks if ADB is available and accessible
    /// </summary>
    /// <returns>True if ADB is available, false otherwise</returns>
    Task<bool> IsAdbAvailableAsync();
}

/// <summary>
/// Interface for Android device operations
/// </summary>
public interface IAndroidDevice : IDisposable
{
    /// <summary>
    /// Device serial number
    /// </summary>
    string Serial { get; }

    /// <summary>
    /// Device name/model
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Indicates if the device is connected
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Indicates if the device is connected to scrcpy server
    /// </summary>
    bool IsScrcpyConnected { get; }

    /// <summary>
    /// Captures a screenshot from the Android device using scrcpy protocol
    /// </summary>
    /// <returns>Screenshot as a Bitmap, or null if capture failed</returns>
    Task<Bitmap?> CaptureScreenshotAsync();

    /// <summary>
    /// Captures a screenshot and saves it to a file
    /// </summary>
    /// <param name="filePath">Path where to save the screenshot</param>
    /// <returns>True if screenshot was saved successfully, false otherwise</returns>
    Task<bool> CaptureScreenshotToFileAsync(string filePath);

    /// <summary>
    /// Establishes and maintains a persistent connection to scrcpy server
    /// </summary>
    /// <returns>True if connection established successfully, false otherwise</returns>
    Task<bool> ConnectToScrcpyServerAsync();

    /// <summary>
    /// Disconnects from scrcpy server and cleans up resources
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    Task DisconnectFromScrcpyServerAsync();

    /// <summary>
    /// Captures a screenshot from the already connected scrcpy server instance
    /// This method requires an active connection established via ConnectToScrcpyServerAsync()
    /// </summary>
    /// <returns>Screenshot as a Bitmap, or null if capture failed or not connected</returns>
    Task<Bitmap?> CaptureScreenshotFromConnectedServerAsync();
}
