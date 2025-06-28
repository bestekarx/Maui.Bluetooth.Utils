# MAUI Bluetooth Printer Utils

Cross-platform Bluetooth printer library for .NET MAUI supporting ESC/POS and Zebra printers.

## Features

- **Cross-platform support**: Android and iOS
- **Multiple printer types**: ESC/POS, Zebra (ZPL), and Generic printers
- **Easy integration**: Simple dependency injection setup
- **Comprehensive API**: Full printer control with text, barcodes, QR codes, and images
- **Event-driven**: Real-time connection and print job status updates
- **Type-safe**: Strongly typed interfaces and models
- **ZPL Support**: Complete Zebra Programming Language support for label printing

## Installation

Add the NuGet package to your .NET MAUI project:

```xml
<PackageReference Include="Maui.Bluetooth.Utils" Version="1.0.0" />
```

## Quick Start

### 1. Register Services

In your `MauiProgram.cs`:

```csharp
using Maui.Bluetooth.Utils.Shared.Services;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register Bluetooth printer services
        builder.Services.AddBluetoothPrinterServices();

        return builder.Build();
    }
}
```

### 2. Use the Printer Manager

```csharp
public partial class MainPage : ContentPage
{
    private readonly BluetoothPrinterManager _printerManager;

    public MainPage(BluetoothPrinterManager printerManager)
    {
        InitializeComponent();
        _printerManager = printerManager;

        // Subscribe to events
        _printerManager.ConnectionStateChanged += OnConnectionStateChanged;
        _printerManager.DeviceDiscovered += OnDeviceDiscovered;
        _printerManager.PrintJobStatusChanged += OnPrintJobStatusChanged;
        _printerManager.PrinterStatusChanged += OnPrinterStatusChanged; // Zebra printers
    }

    private async void ScanButton_Clicked(object sender, EventArgs e)
    {
        // Check Bluetooth availability
        if (!await _printerManager.IsBluetoothAvailableAsync())
        {
            await DisplayAlert("Error", "Bluetooth is not available", "OK");
            return;
        }

        // Request permissions
        if (!await _printerManager.RequestPermissionsAsync())
        {
            await DisplayAlert("Error", "Bluetooth permissions denied", "OK");
            return;
        }

        // Scan for devices
        var devices = await _printerManager.ScanForDevicesAsync();
        foreach (var device in devices)
        {
            Console.WriteLine($"Found device: {device.Name} ({device.PrinterType})");
        }
    }

    private async void ConnectButton_Clicked(object sender, EventArgs e)
    {
        // Connect to a device (you would select this from a list)
        var device = new BluetoothDeviceModel
        {
            Name = "My Printer",
            Address = "00:11:22:33:44:55",
            PrinterType = PrinterType.EscPos
        };

        var connected = await _printerManager.ConnectAsync(device);
        if (connected)
        {
            await DisplayAlert("Success", "Connected to printer", "OK");
        }
    }

    private async void PrintButton_Clicked(object sender, EventArgs e)
    {
        // Print text
        await _printerManager.PrintTextAsync("Hello World!", TextAlignment.Center, true);

        // Print barcode
        await _printerManager.PrintBarcodeAsync("123456789", BarcodeType.Code128);

        // Print QR code
        await _printerManager.PrintQrCodeAsync("https://example.com");

        // Cut paper
        await _printerManager.CutPaperAsync();
    }

    private void OnConnectionStateChanged(object? sender, ConnectionStateChangedEventArgs e)
    {
        Console.WriteLine($"Connection state: {e.PreviousState} -> {e.CurrentState}");
    }

    private void OnDeviceDiscovered(object? sender, BluetoothDeviceModel device)
    {
        Console.WriteLine($"Device discovered: {device.Name}");
    }

    private void OnPrintJobStatusChanged(object? sender, PrintJobStatusChangedEventArgs e)
    {
        Console.WriteLine($"Print job {e.JobId}: {e.Status} ({e.Progress}%)");
    }

    private void OnPrinterStatusChanged(object? sender, PrinterStatusChangedEventArgs e)
    {
        Console.WriteLine($"Printer status: {e.CurrentStatus.IsReady}");
    }
}
```

## Zebra Printer Support

### Connect to Zebra Printer

```csharp
var zebraDevice = new BluetoothDeviceModel
{
    Name = "Zebra ZT230",
    Address = "00:11:22:33:44:55",
    PrinterType = PrinterType.Zebra
};

var connected = await _printerManager.ConnectAsync(zebraDevice);
```

### Print ZPL Labels

```csharp
// Simple ZPL label
var zplLabel = @"^XA
^FO50,50^A0N,50,50^FDHello Zebra^FS
^FO50,120^BY3^BCN,100,Y,N,N^FD123456789^FS
^XZ";

await _printerManager.PrintZplLabelAsync(zplLabel);

// Using ZebraUtils for easier ZPL generation
using Maui.Bluetooth.Utils.Shared.Utils;

var label = ZebraUtils.GenerateLabel(
    ZebraUtils.Text("Product Label", 50, 50, "0", 40),
    ZebraUtils.Barcode("ABC123", 50, 120, "1", 80, 3),
    ZebraUtils.QrCode("https://example.com", 50, 220, 5, "M"),
    ZebraUtils.Rectangle(40, 40, 400, 300, 2)
);

await _printerManager.PrintZplLabelAsync(label);
```

### Print Labels with Data

```csharp
// ZPL template with placeholders
var template = @"^XA
^FO50,50^A0N,40,40^FDProduct: {ProductName}^FS
^FO50,100^A0N,30,30^FDPrice: ${Price}^FS
^FO50,150^BY3^BCN,80,Y,N,N^FD{SKU}^FS
^XZ";

var data = new Dictionary<string, string>
{
    { "ProductName", "Sample Product" },
    { "Price", "29.99" },
    { "SKU", "SKU123456" }
};

await _printerManager.PrintZplLabelWithDataAsync(template, data);
```

### Zebra Printer Settings

```csharp
// Set print darkness (0-30)
await _printerManager.SetZebraPrintDarknessAsync(15);

// Set print speed (1-14)
await _printerManager.SetZebraPrintSpeedAsync(3);

// Set label dimensions
await _printerManager.SetZebraLabelDimensionsAsync(609, 609); // 4" x 4" at 203 DPI

// Get printer status
var status = await _printerManager.GetZebraPrinterStatusAsync();
if (status.IsReady)
{
    Console.WriteLine("Printer is ready");
}

// Check if printer is ready
if (await _printerManager.IsZebraPrinterReadyAsync())
{
    await _printerManager.PrintZplLabelAsync(zplLabel);
}

// Calibrate printer
await _printerManager.CalibrateZebraPrinterAsync();

// Print test label
await _printerManager.PrintZebraTestLabelAsync();
```

### ZebraUtils Helper Methods

```csharp
using Maui.Bluetooth.Utils.Shared.Utils;

// Generate common ZPL elements
var textElement = ZebraUtils.Text("Hello", 50, 50, "0", 30);
var barcodeElement = ZebraUtils.Barcode("123456", 50, 100, "1", 60, 2);
var qrElement = ZebraUtils.QrCode("https://example.com", 50, 180, 4, "M");
var lineElement = ZebraUtils.Line(50, 250, 350, 250, 2);
var rectElement = ZebraUtils.Rectangle(40, 40, 360, 220, 1);

// Generate complete labels
var simpleLabel = ZebraUtils.GenerateTextLabel("Simple Text", 40);
var barcodeLabel = ZebraUtils.GenerateBarcodeLabel("123456789", "Product SKU");
var qrLabel = ZebraUtils.GenerateQrCodeLabel("https://example.com", "Scan for more info");

// Print the labels
await _printerManager.PrintZplLabelAsync(simpleLabel);
await _printerManager.PrintZplLabelAsync(barcodeLabel);
await _printerManager.PrintZplLabelAsync(qrLabel);
```

## Advanced Usage

### Custom Configuration

```csharp
builder.Services.AddBluetoothPrinterServices(options =>
{
    options.DefaultScanTimeout = 15000;
    options.DefaultConnectionTimeout = 45000;
    options.EnableDebugLogging = true;
    options.EnableConnectionRetry = true;
    options.MaxConnectionRetries = 5;
});
```

### Custom Service Implementations

```csharp
// Custom Bluetooth service
public class CustomBluetoothService : IBluetoothService
{
    // Implementation...
}

// Custom printer service
public class CustomEscPosService : IEscPosPrinterService
{
    // Implementation...
}

// Register custom services
builder.Services.AddBluetoothPrinterServices<CustomBluetoothService>();
builder.Services.AddBluetoothPrinterServices<CustomEscPosService, CustomZebraService, CustomGenericService>();
```

### Print Complex Data

```csharp
var printData = new List<PrintDataModel>
{
    new PrintDataModel
    {
        Type = PrintDataType.Text,
        Content = "RECEIPT",
        Alignment = TextAlignment.Center,
        IsBold = true,
        FontSize = 16
    },
    new PrintDataModel
    {
        Type = PrintDataType.LineBreak,
        Content = ""
    },
    new PrintDataModel
    {
        Type = PrintDataType.Text,
        Content = "Item 1: $10.00",
        Alignment = TextAlignment.Left
    },
    new PrintDataModel
    {
        Type = PrintDataType.Barcode,
        Content = "123456789",
        BarcodeType = BarcodeType.Code128
    },
    new PrintDataModel
    {
        Type = PrintDataType.Cut,
        Content = ""
    }
};

await _printerManager.PrintDataAsync(printData);
```

## Platform Configuration

### Android

Add the following permissions to your `AndroidManifest.xml`:

```xml
<uses-permission android:name="android.permission.BLUETOOTH" />
<uses-permission android:name="android.permission.BLUETOOTH_ADMIN" />
<uses-permission android:name="android.permission.BLUETOOTH_CONNECT" />
<uses-permission android:name="android.permission.BLUETOOTH_SCAN" />
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
```

### iOS

Add the following keys to your `Info.plist`:

```xml
<key>NSBluetoothAlwaysUsageDescription</key>
<string>This app uses Bluetooth to connect to printers</string>
<key>NSBluetoothPeripheralUsageDescription</key>
<string>This app uses Bluetooth to connect to printers</string>
```

## Error Handling

```csharp
try
{
    await _printerManager.PrintTextAsync("Hello World!");
}
catch (BluetoothConnectionException ex)
{
    // Handle connection errors
    await DisplayAlert("Connection Error", ex.Message, "OK");
}
catch (PrinterException ex)
{
    // Handle printer errors
    await DisplayAlert("Printer Error", ex.Message, "OK");
}
catch (Exception ex)
{
    // Handle other errors
    await DisplayAlert("Error", ex.Message, "OK");
}
```

## Troubleshooting

### Common Issues

1. **Bluetooth not available**: Ensure Bluetooth is enabled and permissions are granted
2. **Device not found**: Check if the device is paired and in range
3. **Connection failed**: Verify the device address and try reconnecting
4. **Print not working**: Check printer status and ensure it's ready

### Debug Logging

Enable debug logging to troubleshoot issues:

```csharp
builder.Services.AddBluetoothPrinterServices(options =>
{
    options.EnableDebugLogging = true;
});
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details. 