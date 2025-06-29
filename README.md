# MAUI Bluetooth Printer Utils

 **MAUI Bluetooth Printer Utils** is a cross-platform Bluetooth printer library for .NET MAUI, supporting both ESC/POS and Zebra (ZPL/CPCL) protocols.

## Features

- ESC/POS and Zebra (ZPL/CPCL) protocol support
- Works on Android and iOS platforms
- Device discovery and connection management
- Print job queue and printer status monitoring
- Dependency Injection support
- Performance and security focused architecture

## Installation

Install via NuGet:
```
dotnet add package Ble.Print.Utils
```

## Quick Start

```csharp
// Start the Bluetooth service
var bluetoothService = serviceProvider.GetRequiredService<IBluetoothService>();

// Discover devices
var devices = await bluetoothService.DiscoverDevicesAsync();

// Connect to a printer
await bluetoothService.ConnectAsync(devices.First());

// Print data
var printerService = serviceProvider.GetRequiredService<IEscPosPrinterService>();
await printerService.PrintAsync(new PrintDataModel { Content = "Hello World!" });
```

## Platform Notes

- **Android:** Make sure to add the required Bluetooth permissions in your AndroidManifest.xml.
- **iOS:** Add Bluetooth usage descriptions in your Info.plist.

## Dependencies

- Microsoft.Maui.Controls
- Microsoft.Maui.Essentials
- Microsoft.Extensions.DependencyInjection
- System.Text.Json
- Zebra.Printer.SDK

## Contribution & License

Contributions are welcome! Please submit a pull request via [GitHub](https://github.com/bestekarx/Maui.Bluetooth.Utils).

Licensed under the MIT License.

---

**For more information and examples, visit the [documentation](https://github.com/bestekarx/Maui.Bluetooth.Utils) page.** 