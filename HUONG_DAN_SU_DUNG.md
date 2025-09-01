# Hướng Dẫn Sử Dụng AndroidScreenCapture Library

## Mục Lục
1. [Giới thiệu](#giới-thiệu)
2. [Yêu cầu hệ thống](#yêu-cầu-hệ-thống)
3. [Build Project](#build-project)
4. [Copy Library vào Project khác](#copy-library-vào-project-khác)
5. [Cài đặt và cấu hình](#cài-đặt-và-cấu-hình)
6. [Hướng dẫn sử dụng chi tiết](#hướng-dẫn-sử-dụng-chi-tiết)
7. [Ví dụ thực tế](#ví-dụ-thực-tế)
8. [Xử lý lỗi](#xử-lý-lỗi)
9. [FAQ](#faq)

---

## Giới thiệu

AndroidScreenCapture là một thư viện .NET cho phép bạn chụp ảnh màn hình từ thiết bị Android một cách dễ dàng. Thư viện hỗ trợ hai phương thức:
- **Phương thức thông thường**: Chụp từng ảnh riêng lẻ
- **Phương thức kết nối liên tục**: Duy trì kết nối với scrcpy server để chụp ảnh nhanh hơn

## Yêu cầu hệ thống

- .NET 8.0 hoặc cao hơn
- Windows (do sử dụng System.Drawing.Common)
- Thiết bị Android đã bật USB Debugging
- ADB (Android Debug Bridge) - đã được tích hợp trong thư viện

## Build Project

### Bước 1: Clone hoặc Download source code
```bash
# Nếu có git
git clone [repository-url]

# Hoặc download ZIP và giải nén
```

### Bước 2: Build thư viện
```bash
# Mở PowerShell/Command Prompt tại thư mục gốc
cd "c:\Users\user\Desktop\VSCode\.Net"

# Build project
dotnet build --configuration Release

# Hoặc build cụ thể
dotnet build AndroidScreenCapture.csproj --configuration Release
```

### Bước 3: Kiểm tra file build
Sau khi build thành công, các file sẽ được tạo tại:
```
bin/Release/net8.0/
├── AndroidScreenCapture.dll      # File thư viện chính
├── AndroidScreenCapture.xml      # Documentation XML
├── AndroidScreenCapture.pdb      # Debug symbols
├── adb.exe                       # ADB executable
├── AdbWinApi.dll                 # ADB Windows API
├── AdbWinUsbApi.dll             # ADB USB API
├── scrcpy-server                 # Scrcpy server file
└── [các dependencies khác]
```

## Copy Library vào Project khác

### Phương án 1: Copy thủ công

#### Bước 1: Tạo thư mục libs trong project đích
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

#### Bước 2: Copy các file cần thiết
```bash
# Copy file DLL chính
copy "bin\Release\net8.0\AndroidScreenCapture.dll" "YourProject\libs\"

# Copy file XML documentation (optional)
copy "bin\Release\net8.0\AndroidScreenCapture.xml" "YourProject\libs\"

# Copy các file dependencies cần thiết
copy "bin\Release\net8.0\dependencies\adb.exe" "YourProject\libs\"
copy "bin\Release\net8.0\dependencies\AdbWinApi.dll" "YourProject\libs\"
copy "bin\Release\net8.0\dependencies\AdbWinUsbApi.dll" "YourProject\libs\"
copy "bin\Release\net8.0\dependencies\scrcpy-server" "YourProject\libs\"
```

#### Bước 3: Cấu hình project file (.csproj)
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <!-- Reference thư viện -->
  <ItemGroup>
    <Reference Include="AndroidScreenCapture">
      <HintPath>libs\AndroidScreenCapture.dll</HintPath>
    </Reference>
  </ItemGroup>

  <!-- Copy các file cần thiết vào output -->
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

### Phương án 2: Tạo NuGet Package (Nâng cao)

#### Bước 1: Cấu hình .csproj để tạo NuGet
```xml
<PropertyGroup>
  <PackageId>AndroidScreenCapture</PackageId>
  <Version>1.0.0</Version>
  <Authors>Your Name</Authors>
  <Description>Android Screen Capture Library using QtScrcpy</Description>
  <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
</PropertyGroup>
```

#### Bước 2: Build NuGet package
```bash
dotnet pack --configuration Release
```

#### Bước 3: Cài đặt trong project khác
```bash
dotnet add package AndroidScreenCapture --source "path/to/nupkg"
```

## Cài đặt và cấu hình

### 1. Thêm using statements
```csharp
using AndroidScreenCapture;
using System.Drawing;
```

### 2. Khởi tạo thư viện
```csharp
// Tạo device manager
var deviceManager = ScreenCaptureFactory.CreateDeviceManager();

// Lấy danh sách thiết bị
var devices = await deviceManager.GetConnectedDevicesAsync();

// Chọn thiết bị đầu tiên
if (devices.Any())
{
    var device = deviceManager.GetDevice(devices.First());
    // Sử dụng device...
}
```

## Hướng dẫn sử dụng chi tiết

### 1. IDeviceManager Interface

#### `GetConnectedDevicesAsync()`
**Mục đích**: Lấy danh sách tất cả thiết bị Android đã kết nối

```csharp
// Cú pháp
Task<IEnumerable<string>> GetConnectedDevicesAsync()

// Ví dụ sử dụng
var deviceManager = ScreenCaptureFactory.CreateDeviceManager();
var devices = await deviceManager.GetConnectedDevicesAsync();

foreach (var deviceSerial in devices)
{
    Console.WriteLine($"Thiết bị tìm thấy: {deviceSerial}");
}

// Kết quả mẫu:
// Thiết bị tìm thấy: emulator-5554
// Thiết bị tìm thấy: ABC123DEF456
```

#### `GetDevice(string serial)`
**Mục đích**: Lấy instance của thiết bị cụ thể

```csharp
// Cú pháp
IAndroidDevice? GetDevice(string serial)

// Ví dụ sử dụng
var device = deviceManager.GetDevice("emulator-5554");
if (device != null)
{
    Console.WriteLine($"Đã kết nối với thiết bị: {device.Name}");
}
else
{
    Console.WriteLine("Không tìm thấy thiết bị");
}
```

### 2. IAndroidDevice Interface

#### Properties

##### `Serial` (string)
**Mục đích**: Lấy serial number của thiết bị
```csharp
string serial = device.Serial;
Console.WriteLine($"Serial: {serial}"); // Ví dụ: emulator-5554
```

##### `Name` (string)
**Mục đích**: Lấy tên hiển thị của thiết bị
```csharp
string name = device.Name;
Console.WriteLine($"Tên thiết bị: {name}"); // Ví dụ: Samsung Galaxy S21
```

##### `IsConnected` (bool)
**Mục đích**: Kiểm tra xem thiết bị có đang kết nối qua ADB không
```csharp
if (device.IsConnected)
{
    Console.WriteLine("Thiết bị đang kết nối");
}
else
{
    Console.WriteLine("Thiết bị không kết nối");
}
```

##### `IsScrcpyConnected` (bool)
**Mục đích**: Kiểm tra xem có đang kết nối liên tục với scrcpy server không
```csharp
if (device.IsScrcpyConnected)
{
    Console.WriteLine("Đã kết nối với scrcpy server");
}
else
{
    Console.WriteLine("Chưa kết nối với scrcpy server");
}
```

#### Methods - Chụp ảnh cơ bản

##### `CaptureScreenshotAsync()`
**Mục đích**: Chụp ảnh màn hình một lần (phương thức thông thường)

```csharp
// Cú pháp
Task<Bitmap?> CaptureScreenshotAsync()

// Ví dụ sử dụng
try
{
    using var screenshot = await device.CaptureScreenshotAsync();
    if (screenshot != null)
    {
        Console.WriteLine($"Chụp ảnh thành công: {screenshot.Width}x{screenshot.Height}");
        
        // Lưu ảnh
        screenshot.Save("screenshot.png");
    }
    else
    {
        Console.WriteLine("Chụp ảnh thất bại");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Lỗi: {ex.Message}");
}
```

##### `CaptureScreenshotToFileAsync(string filename)`
**Mục đích**: Chụp ảnh và lưu trực tiếp vào file

```csharp
// Cú pháp
Task CaptureScreenshotToFileAsync(string filename)

// Ví dụ sử dụng
try
{
    await device.CaptureScreenshotToFileAsync("my_screenshot.png");
    Console.WriteLine("Đã lưu ảnh vào my_screenshot.png");
}
catch (Exception ex)
{
    Console.WriteLine($"Lỗi khi lưu ảnh: {ex.Message}");
}

// Hỗ trợ các định dạng
await device.CaptureScreenshotToFileAsync("screenshot.png");  // PNG
await device.CaptureScreenshotToFileAsync("screenshot.jpg");  // JPEG
await device.CaptureScreenshotToFileAsync("screenshot.bmp");  // BMP
await device.CaptureScreenshotToFileAsync("screenshot.gif");  // GIF
```

#### Methods - Kết nối liên tục (Persistent Connection)

##### `ConnectToScrcpyServerAsync()`
**Mục đích**: Thiết lập kết nối liên tục với scrcpy server để chụp ảnh nhanh hơn

```csharp
// Cú pháp
Task<bool> ConnectToScrcpyServerAsync()

// Ví dụ sử dụng
try
{
    bool connected = await device.ConnectToScrcpyServerAsync();
    if (connected)
    {
        Console.WriteLine("Đã kết nối với scrcpy server");
        Console.WriteLine($"Trạng thái kết nối: {device.IsScrcpyConnected}");
    }
    else
    {
        Console.WriteLine("Kết nối thất bại");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Lỗi kết nối: {ex.Message}");
}
```

##### `CaptureScreenshotFromConnectedServerAsync()`
**Mục đích**: Chụp ảnh từ kết nối liên tục (nhanh hơn cho nhiều ảnh liên tiếp)

```csharp
// Cú pháp
Task<Bitmap?> CaptureScreenshotFromConnectedServerAsync()

// Ví dụ sử dụng
// Lưu ý: Phải gọi ConnectToScrcpyServerAsync() trước
if (device.IsScrcpyConnected)
{
    try
    {
        using var screenshot = await device.CaptureScreenshotFromConnectedServerAsync();
        if (screenshot != null)
        {
            Console.WriteLine($"Chụp nhanh thành công: {screenshot.Width}x{screenshot.Height}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Lỗi chụp nhanh: {ex.Message}");
    }
}
else
{
    Console.WriteLine("Chưa kết nối server. Gọi ConnectToScrcpyServerAsync() trước");
}
```

##### `DisconnectFromScrcpyServerAsync()`
**Mục đích**: Ngắt kết nối liên tục với scrcpy server

```csharp
// Cú pháp
Task DisconnectFromScrcpyServerAsync()

// Ví dụ sử dụng
try
{
    await device.DisconnectFromScrcpyServerAsync();
    Console.WriteLine("Đã ngắt kết nối server");
    Console.WriteLine($"Trạng thái: {device.IsScrcpyConnected}"); // False
}
catch (Exception ex)
{
    Console.WriteLine($"Lỗi ngắt kết nối: {ex.Message}");
}
```

##### `Dispose()`
**Mục đích**: Giải phóng tài nguyên (nên gọi khi kết thúc)

```csharp
// Ví dụ sử dụng
device.Dispose(); // Hoặc sử dụng using statement

// Tốt hơn với using
using var device = deviceManager.GetDevice(deviceSerial);
// Tự động gọi Dispose() khi ra khỏi scope
```

## Ví dụ thực tế

### Ví dụ 1: Chụp ảnh đơn giản
```csharp
using AndroidScreenCapture;
using System.Drawing;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Chụp ảnh màn hình Android ===");
        
        try
        {
            // Bước 1: Tạo device manager
            var deviceManager = ScreenCaptureFactory.CreateDeviceManager();
            
            // Bước 2: Lấy danh sách thiết bị
            var devices = await deviceManager.GetConnectedDevicesAsync();
            
            if (!devices.Any())
            {
                Console.WriteLine("Không tìm thấy thiết bị Android nào");
                return;
            }
            
            // Bước 3: Chọn thiết bị đầu tiên
            using var device = deviceManager.GetDevice(devices.First());
            if (device == null)
            {
                Console.WriteLine("Không thể kết nối với thiết bị");
                return;
            }
            
            Console.WriteLine($"Sử dụng thiết bị: {device.Name} ({device.Serial})");
            
            // Bước 4: Chụp ảnh
            using var screenshot = await device.CaptureScreenshotAsync();
            if (screenshot != null)
            {
                var filename = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                screenshot.Save(filename);
                Console.WriteLine($"Đã lưu ảnh: {filename}");
                Console.WriteLine($"Kích thước: {screenshot.Width}x{screenshot.Height}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi: {ex.Message}");
        }
        
        Console.WriteLine("Nhấn phím bất kỳ để thoát...");
        Console.ReadKey();
    }
}
```

### Ví dụ 2: Chụp nhiều ảnh liên tiếp với kết nối liên tục
```csharp
using AndroidScreenCapture;
using System.Drawing;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Chụp ảnh liên tục ===");
        
        try
        {
            var deviceManager = ScreenCaptureFactory.CreateDeviceManager();
            var devices = await deviceManager.GetConnectedDevicesAsync();
            
            if (!devices.Any())
            {
                Console.WriteLine("Không tìm thấy thiết bị");
                return;
            }
            
            using var device = deviceManager.GetDevice(devices.First());
            Console.WriteLine($"Thiết bị: {device.Name}");
            
            // Bước 1: Kết nối với scrcpy server
            Console.WriteLine("Đang kết nối với scrcpy server...");
            bool connected = await device.ConnectToScrcpyServerAsync();
            
            if (!connected)
            {
                Console.WriteLine("Kết nối thất bại, sử dụng phương thức thông thường");
                return;
            }
            
            Console.WriteLine("Kết nối thành công!");
            
            // Bước 2: Chụp nhiều ảnh
            for (int i = 1; i <= 5; i++)
            {
                Console.WriteLine($"Chụp ảnh {i}/5...");
                
                try
                {
                    using var screenshot = await device.CaptureScreenshotFromConnectedServerAsync();
                    if (screenshot != null)
                    {
                        var filename = $"screenshot_{i:D2}_{DateTime.Now:HHmmss}.png";
                        screenshot.Save(filename);
                        Console.WriteLine($"  ✓ Đã lưu: {filename}");
                    }
                    else
                    {
                        Console.WriteLine($"  ✗ Ảnh {i} thất bại");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ✗ Lỗi ảnh {i}: {ex.Message}");
                }
                
                // Đợi 1 giây trước khi chụp ảnh tiếp theo
                if (i < 5) await Task.Delay(1000);
            }
            
            // Bước 3: Ngắt kết nối
            Console.WriteLine("Ngắt kết nối server...");
            await device.DisconnectFromScrcpyServerAsync();
            Console.WriteLine("Hoàn thành!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi: {ex.Message}");
        }
        
        Console.ReadKey();
    }
}
```

### Ví dụ 3: Ứng dụng Console đầy đủ
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
        
        // Khởi tạo
        _deviceManager = ScreenCaptureFactory.CreateDeviceManager();
        
        while (true)
        {
            try
            {
                await ShowMainMenuAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
                Console.WriteLine("Nhấn Enter để tiếp tục...");
                Console.ReadLine();
            }
        }
    }
    
    async Task ShowMainMenuAsync()
    {
        Console.Clear();
        Console.WriteLine("=== MENU CHÍNH ===");
        Console.WriteLine("1. Quét thiết bị");
        Console.WriteLine("2. Chọn thiết bị");
        Console.WriteLine("3. Chụp ảnh đơn");
        Console.WriteLine("4. Chụp ảnh liên tục");
        Console.WriteLine("5. Kết nối scrcpy server");
        Console.WriteLine("6. Ngắt kết nối server");
        Console.WriteLine("7. Thông tin thiết bị");
        Console.WriteLine("0. Thoát");
        
        if (_currentDevice != null)
        {
            Console.WriteLine($"\nThiết bị hiện tại: {_currentDevice.Name}");
            Console.WriteLine($"ADB kết nối: {_currentDevice.IsConnected}");
            Console.WriteLine($"Scrcpy kết nối: {_currentDevice.IsScrcpyConnected}");
        }
        
        Console.Write("\nChọn: ");
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
                Console.WriteLine("Lựa chọn không hợp lệ");
                await Task.Delay(1000);
                break;
        }
    }
    
    async Task ScanDevicesAsync()
    {
        Console.WriteLine("Đang quét thiết bị...");
        var devices = await _deviceManager!.GetConnectedDevicesAsync();
        
        if (!devices.Any())
        {
            Console.WriteLine("Không tìm thấy thiết bị nào");
        }
        else
        {
            Console.WriteLine($"Tìm thấy {devices.Count()} thiết bị:");
            foreach (var device in devices)
            {
                Console.WriteLine($"  - {device}");
            }
        }
        
        Console.WriteLine("Nhấn Enter để tiếp tục...");
        Console.ReadLine();
    }
    
    async Task SelectDeviceAsync()
    {
        var devices = await _deviceManager!.GetConnectedDevicesAsync();
        
        if (!devices.Any())
        {
            Console.WriteLine("Không có thiết bị nào. Vui lòng kết nối thiết bị trước.");
            Console.ReadLine();
            return;
        }
        
        Console.WriteLine("Chọn thiết bị:");
        var deviceList = devices.ToList();
        for (int i = 0; i < deviceList.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {deviceList[i]}");
        }
        
        Console.Write("Nhập số: ");
        if (int.TryParse(Console.ReadLine(), out int index) && 
            index > 0 && index <= deviceList.Count)
        {
            _currentDevice?.Dispose();
            _currentDevice = _deviceManager.GetDevice(deviceList[index - 1]);
            Console.WriteLine($"Đã chọn: {_currentDevice?.Name}");
        }
        else
        {
            Console.WriteLine("Lựa chọn không hợp lệ");
        }
        
        Console.ReadLine();
    }
    
    async Task CaptureScreenshotAsync()
    {
        if (_currentDevice == null)
        {
            Console.WriteLine("Vui lòng chọn thiết bị trước");
            Console.ReadLine();
            return;
        }
        
        Console.WriteLine("Đang chụp ảnh...");
        
        var filename = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        await _currentDevice.CaptureScreenshotToFileAsync(filename);
        Console.WriteLine($"Đã lưu: {filename}");
        
        Console.ReadLine();
    }
    
    async Task CaptureMultipleAsync()
    {
        if (_currentDevice == null)
        {
            Console.WriteLine("Vui lòng chọn thiết bị trước");
            Console.ReadLine();
            return;
        }
        
        Console.Write("Số lượng ảnh cần chụp: ");
        if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
        {
            Console.WriteLine("Số lượng không hợp lệ");
            Console.ReadLine();
            return;
        }
        
        Console.Write("Khoảng cách giữa các ảnh (giây): ");
        if (!int.TryParse(Console.ReadLine(), out int interval) || interval < 0)
        {
            interval = 1;
        }
        
        for (int i = 1; i <= count; i++)
        {
            Console.WriteLine($"Chụp ảnh {i}/{count}...");
            
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
                Console.WriteLine($"  ✗ Lỗi: {ex.Message}");
            }
            
            if (i < count && interval > 0)
            {
                await Task.Delay(interval * 1000);
            }
        }
        
        Console.WriteLine("Hoàn thành!");
        Console.ReadLine();
    }
    
    async Task ConnectServerAsync()
    {
        if (_currentDevice == null)
        {
            Console.WriteLine("Vui lòng chọn thiết bị trước");
            Console.ReadLine();
            return;
        }
        
        Console.WriteLine("Đang kết nối scrcpy server...");
        bool success = await _currentDevice.ConnectToScrcpyServerAsync();
        
        if (success)
        {
            Console.WriteLine("Kết nối thành công!");
        }
        else
        {
            Console.WriteLine("Kết nối thất bại");
        }
        
        Console.ReadLine();
    }
    
    async Task DisconnectServerAsync()
    {
        if (_currentDevice == null)
        {
            Console.WriteLine("Vui lòng chọn thiết bị trước");
            Console.ReadLine();
            return;
        }
        
        Console.WriteLine("Đang ngắt kết nối...");
        await _currentDevice.DisconnectFromScrcpyServerAsync();
        Console.WriteLine("Đã ngắt kết nối");
        Console.ReadLine();
    }
    
    void ShowDeviceInfo()
    {
        if (_currentDevice == null)
        {
            Console.WriteLine("Chưa chọn thiết bị");
        }
        else
        {
            Console.WriteLine("=== THÔNG TIN THIẾT BỊ ===");
            Console.WriteLine($"Tên: {_currentDevice.Name}");
            Console.WriteLine($"Serial: {_currentDevice.Serial}");
            Console.WriteLine($"ADB kết nối: {_currentDevice.IsConnected}");
            Console.WriteLine($"Scrcpy kết nối: {_currentDevice.IsScrcpyConnected}");
        }
        
        Console.ReadLine();
    }
}
```

## Xử lý lỗi

### Các lỗi thường gặp và cách khắc phục

#### 1. "No devices found"
```csharp
// Nguyên nhân và cách khắc phục
var devices = await deviceManager.GetConnectedDevicesAsync();
if (!devices.Any())
{
    Console.WriteLine("Kiểm tra:");
    Console.WriteLine("1. Thiết bị đã kết nối USB chưa?");
    Console.WriteLine("2. Đã bật USB Debugging chưa?");
    Console.WriteLine("3. Đã cài đặt driver ADB chưa?");
    Console.WriteLine("4. Thử lệnh 'adb devices' trong command line");
}
```

#### 2. "Device not connected"
```csharp
if (!device.IsConnected)
{
    Console.WriteLine("Thiết bị không kết nối:");
    Console.WriteLine("- Thử ngắt và cắm lại USB");
    Console.WriteLine("- Khởi động lại ADB: adb kill-server && adb start-server");
    Console.WriteLine("- Kiểm tra USB Debugging");
}
```

#### 3. "Scrcpy server connection failed"
```csharp
try
{
    bool connected = await device.ConnectToScrcpyServerAsync();
    if (!connected)
    {
        Console.WriteLine("Kết nối scrcpy thất bại:");
        Console.WriteLine("- Thiết bị có thể không hỗ trợ scrcpy");
        Console.WriteLine("- Thử sử dụng phương thức chụp thông thường");
        Console.WriteLine("- Kiểm tra port 27183 có bị chặn không");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Lỗi kết nối: {ex.Message}");
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
    Console.WriteLine("Không có quyền ghi file:");
    Console.WriteLine("- Kiểm tra quyền write trong thư mục");
    Console.WriteLine("- Chạy ứng dụng với quyền Administrator");
    Console.WriteLine("- Thử ghi vào thư mục khác");
}
catch (DirectoryNotFoundException)
{
    Console.WriteLine("Thư mục không tồn tại");
    Console.WriteLine("- Tạo thư mục trước khi lưu file");
}
```

### Template xử lý lỗi toàn diện
```csharp
async Task<bool> SafeCaptureScreenshot(IAndroidDevice device, string filename)
{
    try
    {
        // Kiểm tra điều kiện trước
        if (!device.IsConnected)
        {
            Console.WriteLine("❌ Thiết bị không kết nối");
            return false;
        }
        
        // Tạo thư mục nếu cần
        var directory = Path.GetDirectoryName(filename);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        // Thực hiện chụp ảnh
        await device.CaptureScreenshotToFileAsync(filename);
        Console.WriteLine($"✅ Đã lưu: {filename}");
        return true;
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"❌ Lỗi thiết bị: {ex.Message}");
        return false;
    }
    catch (UnauthorizedAccessException)
    {
        Console.WriteLine($"❌ Không có quyền ghi file: {filename}");
        return false;
    }
    catch (DirectoryNotFoundException)
    {
        Console.WriteLine($"❌ Thư mục không tồn tại: {Path.GetDirectoryName(filename)}");
        return false;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Lỗi không xác định: {ex.Message}");
        return false;
    }
}
```

## FAQ

### Q: Tại sao thư viện cần file adb.exe, AdbWinApi.dll?
A: Thư viện sử dụng ADB (Android Debug Bridge) để giao tiếp với thiết bị Android. Các file này cần thiết để ADB hoạt động trên Windows.

### Q: Có thể chụp ảnh mà không cần scrcpy server không?
A: Có, sử dụng phương thức `CaptureScreenshotAsync()` hoặc `CaptureScreenshotToFileAsync()`. Chúng sử dụng lệnh screencap của Android.

### Q: Phương thức nào nhanh hơn?
A: Kết nối liên tục (`CaptureScreenshotFromConnectedServerAsync()`) nhanh hơn cho nhiều ảnh liên tiếp vì không cần thiết lập kết nối mới mỗi lần.

### Q: Thư viện có hoạt động trên Linux/macOS không?
A: Hiện tại chỉ hỗ trợ Windows do sử dụng System.Drawing.Common. Để hỗ trợ cross-platform cần thay đổi xử lý hình ảnh.

### Q: Làm sao để chụp ảnh chất lượng cao?
A: Chất lượng ảnh phụ thuộc vào độ phân giải màn hình thiết bị. Thư viện chụp ảnh với độ phân giải gốc của thiết bị.

### Q: Có thể chụp video không?
A: Hiện tại chỉ hỗ trợ chụp ảnh. Để chụp video cần implement thêm tính năng sử dụng FFmpeg.

### Q: Tại sao đôi khi chụp ảnh bị chậm?
A: 
- Phương thức thông thường: Cần truyền file qua ADB (chậm hơn)
- Kết nối liên tục: Nhanh hơn nhưng cần setup ban đầu
- Tốc độ USB và hiệu năng thiết bị cũng ảnh hưởng

### Q: Có thể sử dụng với nhiều thiết bị cùng lúc không?
A: Có, tạo nhiều instance IAndroidDevice cho các thiết bị khác nhau:

```csharp
var devices = await deviceManager.GetConnectedDevicesAsync();
var deviceInstances = devices.Select(serial => 
    deviceManager.GetDevice(serial)).ToList();

// Chụp ảnh từ tất cả thiết bị
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

## Kết luận

AndroidScreenCapture library cung cấp một cách đơn giản và hiệu quả để chụp ảnh màn hình từ thiết bị Android. Với hai phương thức chụp ảnh linh hoạt, bạn có thể chọn cách phù hợp với nhu cầu của mình:

- **Chụp đơn lẻ**: Sử dụng `CaptureScreenshotAsync()` cho việc chụp thỉnh thoảng
- **Chụp liên tục**: Sử dụng kết nối liên tục cho việc chụp nhiều ảnh liên tiếp

Nhớ luôn gọi `Dispose()` hoặc sử dụng `using` statement để giải phóng tài nguyên sau khi sử dụng.
