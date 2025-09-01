using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AndroidScreenCapture;

/// <summary>
/// Implementation of device manager for Android devices using ADB and scrcpy protocol
/// </summary>
public class DeviceManager : IDeviceManager
{
    private readonly string _adbPath;
    private readonly string _scrcpyServerPath;
    private readonly Dictionary<string, AndroidDevice> _cachedDevices;

    /// <summary>
    /// Initializes a new instance of DeviceManager
    /// </summary>
    /// <param name="adbPath">Path to ADB executable. If null, will search in current directory or PATH</param>
    /// <param name="scrcpyServerPath">Path to scrcpy-server file. If null, will search in current directory</param>
    public DeviceManager(string? adbPath = null, string? scrcpyServerPath = null)
    {
        // Try to find ADB in current directory first, then PATH
        _adbPath = adbPath ?? FindAdbPath();
        
        // Try to find scrcpy-server in current directory
        _scrcpyServerPath = scrcpyServerPath ?? FindScrcpyServerPath();
        
        _cachedDevices = new Dictionary<string, AndroidDevice>();
    }

    /// <summary>
    /// Finds ADB executable path
    /// </summary>
    private string FindAdbPath()
    {
        // Check current directory first
        var currentDirAdb = Path.Combine(AppContext.BaseDirectory, "adb.exe");
        if (File.Exists(currentDirAdb))
        {
            return currentDirAdb;
        }

        // Fallback to PATH
        return "adb";
    }

    /// <summary>
    /// Finds scrcpy-server file path
    /// </summary>
    private string FindScrcpyServerPath()
    {
        // Check current directory first
        var currentDirServer = Path.Combine(AppContext.BaseDirectory, "scrcpy-server");
        if (File.Exists(currentDirServer))
        {
            return currentDirServer;
        }

        // Try without extension
        var serverPath = Path.Combine(AppContext.BaseDirectory, "scrcpy-server");
        if (File.Exists(serverPath))
        {
            return serverPath;
        }

        throw new FileNotFoundException("scrcpy-server file not found. Please ensure it's in the application directory.");
    }

    /// <inheritdoc/>
    public async Task<List<string>> GetConnectedDevicesAsync()
    {
        var devices = new List<string>();
        
        try
        {
            var result = await RunAdbCommandAsync("devices");
            if (result.IsSuccess && !string.IsNullOrEmpty(result.Output))
            {
                var lines = result.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines.Skip(1)) // Skip "List of devices attached" header
                {
                    var parts = line.Trim().Split('\t');
                    if (parts.Length >= 2 && parts[1].Trim() == "device")
                    {
                        devices.Add(parts[0].Trim());
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get connected devices: {ex.Message}", ex);
        }

        return devices;
    }

    /// <inheritdoc/>
    public IAndroidDevice? GetDevice(string serial)
    {
        if (string.IsNullOrWhiteSpace(serial))
            return null;

        // Return cached device if available
        if (_cachedDevices.TryGetValue(serial, out var cachedDevice))
            return cachedDevice;

        // Create new device instance with scrcpy server path
        var device = new AndroidDevice(serial, _adbPath, _scrcpyServerPath);
        _cachedDevices[serial] = device;
        return device;
    }

    /// <inheritdoc/>
    public async Task<bool> IsAdbAvailableAsync()
    {
        try
        {
            var result = await RunAdbCommandAsync("version");
            return result.IsSuccess && result.Output.Contains("Android Debug Bridge");
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if scrcpy-server file is available
    /// </summary>
    public bool IsScrcpyServerAvailable()
    {
        return File.Exists(_scrcpyServerPath);
    }

    /// <summary>
    /// Gets the path to ADB executable
    /// </summary>
    public string AdbPath => _adbPath;

    /// <summary>
    /// Gets the path to scrcpy-server file
    /// </summary>
    public string ScrcpyServerPath => _scrcpyServerPath;

    /// <summary>
    /// Runs an ADB command and returns the result
    /// </summary>
    private async Task<(bool IsSuccess, string Output, string Error)> RunAdbCommandAsync(string arguments)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = _adbPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            process.Start();
            
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();
            
            var output = await outputTask;
            var error = await errorTask;

            return (process.ExitCode == 0, output, error);
        }
        catch (Exception ex)
        {
            return (false, string.Empty, ex.Message);
        }
    }
}
