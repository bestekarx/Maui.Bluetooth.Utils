# MAUI Bluetooth Printer Utils

Cross-platform Bluetooth printer library for .NET MAUI supporting ESC/POS and Zebra printers.

## Features

- **Cross-platform support**: Android and iOS
- **Multiple printer types**: ESC/POS, Zebra (ZPL), and Generic printers
- **Easy integration**: Simple dependency injection setup
- **Comprehensive API**: Full printer control with text, barcodes, QR codes, and images
- **Event-driven**: Real-time connection and print job status updates
- **Type-safe**: Strongly typed interfaces and models

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
}
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

## Platform-Specific Setup

### Android

Add the following permissions to your `Platforms/Android/AndroidManifest.xml`:

```xml
<uses-permission android:name="android.permission.BLUETOOTH" />
<uses-permission android:name="android.permission.BLUETOOTH_ADMIN" />
<uses-permission android:name="android.permission.BLUETOOTH_CONNECT" />
<uses-permission android:name="android.permission.BLUETOOTH_SCAN" />
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
```

### iOS

Add the following keys to your `Platforms/iOS/Info.plist`:

```xml
<key>NSBluetoothAlwaysUsageDescription</key>
<string>This app uses Bluetooth to connect to printers</string>
<key>NSBluetoothPeripheralUsageDescription</key>
<string>This app uses Bluetooth to connect to printers</string>
```

## API Reference

### Core Classes

- `BluetoothPrinterManager`: Main printer manager class
- `IBluetoothService`: Platform-specific Bluetooth service interface
- `IEscPosPrinterService`: ESC/POS printer service interface
- `IZebraPrinterService`: Zebra printer service interface
- `IGenericPrinterService`: Generic printer service interface

### Models

- `BluetoothDeviceModel`: Bluetooth device information
- `PrintDataModel`: Print data item
- `PrinterStatus`: Printer status information
- `ConnectionState`: Connection state enumeration
- `PrinterType`: Printer type enumeration

### Events

- `ConnectionStateChanged`: Fired when connection state changes
- `DeviceDiscovered`: Fired when a new device is discovered
- `PrintJobStatusChanged`: Fired when print job status changes

## Supported Printer Types

### ESC/POS Printers
- Thermal receipt printers
- POS printers
- Label printers with ESC/POS support

### Zebra Printers
- ZPL-compatible label printers
- Industrial label printers
- RFID printers

### Generic Printers
- Any Bluetooth printer with raw data support
- Custom printer protocols

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and questions, please open an issue on GitHub or contact the maintainers. 