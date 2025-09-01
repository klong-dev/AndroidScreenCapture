using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Sockets;
using System.Text;

namespace AndroidScreenCapture;

/// <summary>
/// Implementation of Android device for screenshot capture using scrcpy protocol
/// </summary>
public class AndroidDevice : IAndroidDevice
{
    private readonly string _adbPath;
    private readonly string _scrcpyServerPath;
    private string? _deviceName;
    private Process? _scrcpyServerProcess;
    private NetworkStream? _videoStream;
    private TcpClient? _tcpClient;
    private const int SCRCPY_VIDEO_PORT = 27183;
    private bool _isServerConnected = false;

    /// <summary>
    /// Initializes a new Android device instance
    /// </summary>
    /// <param name="serial">Device serial number</param>
    /// <param name="adbPath">Path to ADB executable</param>
    /// <param name="scrcpyServerPath">Path to scrcpy-server JAR file</param>
    public AndroidDevice(string serial, string adbPath, string scrcpyServerPath)
    {
        Serial = serial ?? throw new ArgumentNullException(nameof(serial));
        _adbPath = adbPath ?? throw new ArgumentNullException(nameof(adbPath));
        _scrcpyServerPath = scrcpyServerPath ?? throw new ArgumentNullException(nameof(scrcpyServerPath));
    }

    /// <inheritdoc/>
    public string Serial { get; }

    /// <inheritdoc/>
    public string Name
    {
        get
        {
            if (_deviceName == null)
            {
                _deviceName = GetDeviceNameAsync().GetAwaiter().GetResult() ?? Serial;
            }
            return _deviceName;
        }
    }

    /// <inheritdoc/>
    public bool IsConnected => CheckConnectionAsync().GetAwaiter().GetResult();

    /// <summary>
    /// Indicates if the device is connected to scrcpy server
    /// </summary>
    public bool IsScrcpyConnected => _isServerConnected;

    /// <inheritdoc/>
    public async Task<Bitmap?> CaptureScreenshotAsync()
    {
        try
        {
            // Method 1: Try using scrcpy server for high-quality screenshot
            var scrcpyResult = await CaptureScreenshotViaScrcpyAsync();
            if (scrcpyResult != null)
            {
                return scrcpyResult;
            }

            // Method 2: Fallback to traditional screencap method
            return await CaptureScreenshotViaScreencapAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Screenshot capture failed for device {Serial}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Captures screenshot using scrcpy server for high quality
    /// </summary>
    private async Task<Bitmap?> CaptureScreenshotViaScrcpyAsync()
    {
        try
        {
            // Start scrcpy server if not running
            if (!await StartScrcpyServerAsync())
            {
                return null;
            }

            // Connect to video stream
            if (!await ConnectToVideoStreamAsync())
            {
                return null;
            }

            // Read one frame from video stream and convert to bitmap
            return await ReadFrameFromStreamAsync();
        }
        catch
        {
            return null;
        }
        finally
        {
            await StopScrcpyServerAsync();
        }
    }

    /// <summary>
    /// Fallback method using traditional screencap
    /// </summary>
    private async Task<Bitmap?> CaptureScreenshotViaScreencapAsync()
    {
        try
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"screenshot_{Guid.NewGuid()}.png");
            
            try
            {
                // Use screencap command
                var screencapResult = await RunDeviceCommandAsync($"shell screencap -p /sdcard/temp_screenshot.png");
                if (!screencapResult.IsSuccess)
                {
                    throw new InvalidOperationException($"Failed to capture screenshot on device: {screencapResult.Error}");
                }

                var pullResult = await RunDeviceCommandAsync($"pull /sdcard/temp_screenshot.png \"{tempFile}\"");
                if (!pullResult.IsSuccess)
                {
                    throw new InvalidOperationException($"Failed to pull screenshot from device: {pullResult.Error}");
                }

                // Clean up device temp file
                _ = await RunDeviceCommandAsync("shell rm /sdcard/temp_screenshot.png");

                // Load bitmap from file
                if (File.Exists(tempFile))
                {
                    using var fileStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read);
                    return new Bitmap(fileStream);
                }

                return null;
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    try
                    {
                        File.Delete(tempFile);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Starts the scrcpy server on the device
    /// </summary>
    private async Task<bool> StartScrcpyServerAsync()
    {
        try
        {
            // Push scrcpy-server to device
            var pushResult = await RunDeviceCommandAsync($"push \"{_scrcpyServerPath}\" /data/local/tmp/scrcpy-server.jar");
            if (!pushResult.IsSuccess)
            {
                return false;
            }

            // Set up port forwarding
            var forwardResult = await RunDeviceCommandAsync($"forward tcp:{SCRCPY_VIDEO_PORT} localabstract:scrcpy");
            if (!forwardResult.IsSuccess)
            {
                return false;
            }

            // Start scrcpy server in background
            var startServerCmd = "shell CLASSPATH=/data/local/tmp/scrcpy-server.jar app_process / com.genymobile.scrcpy.Server 2.0 info=- tunnel_forward=true control=false cleanup=false power_off_on_close=false clipboard_autosync=false downsize_on_error=false send_frame_meta=false send_dummy_byte=false send_device_meta=false send_codec_meta=false raw_video_stream=false";
            
            _scrcpyServerProcess = new Process();
            _scrcpyServerProcess.StartInfo = new ProcessStartInfo
            {
                FileName = _adbPath,
                Arguments = $"-s {Serial} {startServerCmd}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            _scrcpyServerProcess.Start();
            
            // Wait a bit for server to start
            await Task.Delay(2000);
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Connects to the video stream from scrcpy server
    /// </summary>
    private async Task<bool> ConnectToVideoStreamAsync()
    {
        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync("127.0.0.1", SCRCPY_VIDEO_PORT);
            _videoStream = _tcpClient.GetStream();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Reads one frame from the video stream and converts to bitmap
    /// </summary>
    private async Task<Bitmap?> ReadFrameFromStreamAsync()
    {
        if (_videoStream == null) return null;

        try
        {
            // Read device info and video stream header
            var buffer = new byte[69]; // Device info size
            await _videoStream.ReadAsync(buffer, 0, buffer.Length);

            // Skip to video frames - this is a simplified implementation
            // Real implementation would need proper H.264 decoding using FFmpeg
            
            // For now, fall back to screencap method
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Stops the scrcpy server
    /// </summary>
    private async Task StopScrcpyServerAsync()
    {
        try
        {
            _videoStream?.Close();
            _tcpClient?.Close();
            
            if (_scrcpyServerProcess != null && !_scrcpyServerProcess.HasExited)
            {
                _scrcpyServerProcess.Kill();
                await _scrcpyServerProcess.WaitForExitAsync();
            }

            // Remove port forwarding
            _ = await RunDeviceCommandAsync($"forward --remove tcp:{SCRCPY_VIDEO_PORT}");
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    /// <inheritdoc/>
    public async Task<bool> CaptureScreenshotToFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        try
        {
            using var bitmap = await CaptureScreenshotAsync();
            if (bitmap == null)
                return false;

            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Determine format from file extension
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var format = extension switch
            {
                ".png" => ImageFormat.Png,
                ".jpg" or ".jpeg" => ImageFormat.Jpeg,
                ".bmp" => ImageFormat.Bmp,
                ".gif" => ImageFormat.Gif,
                _ => ImageFormat.Png // Default to PNG
            };

            bitmap.Save(filePath, format);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the device name/model
    /// </summary>
    private async Task<string?> GetDeviceNameAsync()
    {
        try
        {
            var result = await RunDeviceCommandAsync("shell getprop ro.product.model");
            return result.IsSuccess ? result.Output.Trim() : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Checks if the device is connected
    /// </summary>
    private async Task<bool> CheckConnectionAsync()
    {
        try
        {
            var result = await RunDeviceCommandAsync("get-state");
            return result.IsSuccess && result.Output.Trim() == "device";
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Runs an ADB command for this specific device
    /// </summary>
    private async Task<(bool IsSuccess, string Output, string Error)> RunDeviceCommandAsync(string command)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = _adbPath,
                Arguments = $"-s {Serial} {command}",
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

    /// <summary>
    /// Establishes and maintains a persistent connection to scrcpy server
    /// </summary>
    /// <returns>True if connection established successfully, false otherwise</returns>
    public async Task<bool> ConnectToScrcpyServerAsync()
    {
        try
        {
            // Stop any existing connection first
            if (_isServerConnected)
            {
                await DisconnectFromScrcpyServerAsync();
            }

            // Start scrcpy server
            if (!await StartScrcpyServerAsync())
            {
                return false;
            }

            // Connect to video stream
            if (!await ConnectToVideoStreamAsync())
            {
                await StopScrcpyServerAsync();
                return false;
            }

            _isServerConnected = true;
            return true;
        }
        catch (Exception ex)
        {
            await DisconnectFromScrcpyServerAsync();
            throw new InvalidOperationException($"Failed to connect to scrcpy server for device {Serial}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Disconnects from scrcpy server and cleans up resources
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    public async Task DisconnectFromScrcpyServerAsync()
    {
        try
        {
            _isServerConnected = false;
            await StopScrcpyServerAsync();
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    /// <summary>
    /// Captures a screenshot from the already connected scrcpy server instance
    /// This method requires an active connection established via ConnectToScrcpyServerAsync()
    /// </summary>
    /// <returns>Screenshot as a Bitmap, or null if capture failed or not connected</returns>
    public async Task<Bitmap?> CaptureScreenshotFromConnectedServerAsync()
    {
        if (!_isServerConnected || _videoStream == null)
        {
            throw new InvalidOperationException($"Device {Serial} is not connected to scrcpy server. Call ConnectToScrcpyServerAsync() first.");
        }

        try
        {
            // Method 1: Try to read frame from connected scrcpy server
            var scrcpyResult = await ReadFrameFromConnectedStreamAsync();
            if (scrcpyResult != null)
            {
                return scrcpyResult;
            }

            // Method 2: Fallback to traditional screencap while keeping server connection
            return await CaptureScreenshotViaScreencapAsync();
        }
        catch (Exception ex)
        {
            // Don't automatically disconnect on errors - the server connection might still be fine
            // Only specific network errors should cause disconnection (handled elsewhere)
            throw new InvalidOperationException($"Screenshot capture failed from connected server for device {Serial}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Reads one frame from the already connected video stream
    /// </summary>
    private Task<Bitmap?> ReadFrameFromConnectedStreamAsync()
    {
        if (_videoStream == null || !_isServerConnected)
        {
            return Task.FromResult<Bitmap?>(null);
        }

        try
        {
            // For now, since we haven't implemented H.264 decoding yet,
            // return null to use fallback method but keep connection alive
            // 
            // Note: We're not checking _tcpClient.Connected here because it can be unreliable
            // The connection status will be managed at a higher level
            //
            // A complete implementation would:
            // 1. Parse the scrcpy protocol headers properly
            // 2. Decode H.264 video frames using FFmpeg
            // 3. Extract a single frame as bitmap
            
            // For now, just return null to trigger fallback
            // but maintain the server connection status
            return Task.FromResult<Bitmap?>(null);
        }
        catch (Exception)
        {
            // Only mark as disconnected on actual exceptions, not just connection property checks
            _isServerConnected = false;
            return Task.FromResult<Bitmap?>(null);
        }
    }

    /// <summary>
    /// Dispose resources
    /// </summary>
    public void Dispose()
    {
        DisconnectFromScrcpyServerAsync().GetAwaiter().GetResult();
    }
}
