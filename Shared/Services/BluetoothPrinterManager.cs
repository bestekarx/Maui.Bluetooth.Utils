using Maui.Bluetooth.Utils.Shared.Interfaces;
using Maui.Bluetooth.Utils.Shared.Models;
using TextAlignment = Maui.Bluetooth.Utils.Shared.Models.TextAlignment;

namespace Maui.Bluetooth.Utils.Shared.Services
{
    /// <summary>
    /// Main Bluetooth printer manager that handles both ESC/POS and Zebra printers
    /// </summary>
    public class BluetoothPrinterManager : IDisposable
    {
        private readonly IBluetoothService _bluetoothService;
        private readonly IServiceProvider _serviceProvider;
        private IPrinterService? _currentPrinterService;
        private IZebraPrinterService? _currentZebraService;
        private BluetoothDeviceModel? _connectedDevice;
        private bool _disposed;

        /// <summary>
        /// Event fired when connection state changes
        /// </summary>
        public event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

        /// <summary>
        /// Event fired when a new device is discovered
        /// </summary>
        public event EventHandler<BluetoothDeviceModel>? DeviceDiscovered;

        /// <summary>
        /// Event fired when print job status changes
        /// </summary>
        public event EventHandler<PrintJobStatusChangedEventArgs>? PrintJobStatusChanged;

        /// <summary>
        /// Event fired when printer status changes (Zebra printers)
        /// </summary>
        public event EventHandler<PrinterStatusChangedEventArgs>? PrinterStatusChanged;

        public BluetoothPrinterManager(IBluetoothService bluetoothService, IServiceProvider serviceProvider)
        {
            _bluetoothService = bluetoothService ?? throw new ArgumentNullException(nameof(bluetoothService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            // Subscribe to Bluetooth service events
            _bluetoothService.ConnectionStateChanged += OnBluetoothConnectionStateChanged;
            _bluetoothService.DeviceDiscovered += OnBluetoothDeviceDiscovered;
        }

        /// <summary>
        /// Check if Bluetooth is available and enabled
        /// </summary>
        /// <returns>True if Bluetooth is available and enabled</returns>
        public async Task<bool> IsBluetoothAvailableAsync()
        {
            return await _bluetoothService.IsBluetoothAvailableAsync();
        }

        /// <summary>
        /// Request Bluetooth permissions
        /// </summary>
        /// <returns>True if permissions are granted</returns>
        public async Task<bool> RequestPermissionsAsync()
        {
            return await _bluetoothService.RequestPermissionsAsync();
        }

        /// <summary>
        /// Start scanning for Bluetooth devices
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of discovered devices</returns>
        public async Task<List<BluetoothDeviceModel>> ScanForDevicesAsync(CancellationToken cancellationToken = default)
        {
            return await _bluetoothService.ScanForDevicesAsync(cancellationToken);
        }

        /// <summary>
        /// Get paired/bonded devices
        /// </summary>
        /// <returns>List of paired devices</returns>
        public async Task<List<BluetoothDeviceModel>> GetPairedDevicesAsync()
        {
            return await _bluetoothService.GetPairedDevicesAsync();
        }

        /// <summary>
        /// Connect to a Bluetooth device
        /// </summary>
        /// <param name="device">Device to connect to</param>
        /// <returns>True if connection successful</returns>
        public async Task<bool> ConnectAsync(BluetoothDeviceModel device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            try
            {
                // Connect to Bluetooth device
                var connected = await _bluetoothService.ConnectAsync(device);
                if (!connected)
                    return false;

                _connectedDevice = device;

                // Create appropriate printer service based on device type
                if (device.PrinterType == PrinterType.Zebra)
                {
                    _currentZebraService = CreateZebraPrinterService();
                    if (_currentZebraService != null)
                    {
                        _currentZebraService.PrinterStatusChanged += OnPrinterStatusChanged;
                    }
                }
                else
                {
                    _currentPrinterService = CreatePrinterService(device.PrinterType);
                    if (_currentPrinterService != null)
                    {
                        await _currentPrinterService.InitializeAsync();
                        _currentPrinterService.PrintJobStatusChanged += OnPrintJobStatusChanged;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(
                    ConnectionState.Disconnected, 
                    ConnectionState.Failed, 
                    device, 
                    ex.Message));
                return false;
            }
        }

        /// <summary>
        /// Disconnect from current device
        /// </summary>
        /// <returns>True if disconnection successful</returns>
        public async Task<bool> DisconnectAsync()
        {
            try
            {
                if (_currentPrinterService != null)
                {
                    _currentPrinterService.PrintJobStatusChanged -= OnPrintJobStatusChanged;
                    _currentPrinterService = null;
                }

                if (_currentZebraService != null)
                {
                    _currentZebraService.PrinterStatusChanged -= OnPrinterStatusChanged;
                    _currentZebraService = null;
                }

                var result = await _bluetoothService.DisconnectAsync();
                _connectedDevice = null;
                return result;
            }
            catch (Exception ex)
            {
                ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(
                    ConnectionState.Connected, 
                    ConnectionState.Failed, 
                    _connectedDevice, 
                    ex.Message));
                return false;
            }
        }

        /// <summary>
        /// Print text (ESC/POS printers)
        /// </summary>
        /// <param name="text">Text to print</param>
        /// <param name="alignment">Text alignment</param>
        /// <param name="isBold">Whether text is bold</param>
        /// <param name="isUnderlined">Whether text is underlined</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintTextAsync(string text, TextAlignment alignment = TextAlignment.Left, bool isBold = false, bool isUnderlined = false)
        {
            if (_currentPrinterService == null)
                throw new InvalidOperationException("No ESC/POS printer connected. Call ConnectAsync first.");

            return await _currentPrinterService.PrintTextAsync(text, alignment, isBold, isUnderlined);
        }

        /// <summary>
        /// Print barcode (ESC/POS printers)
        /// </summary>
        /// <param name="data">Barcode data</param>
        /// <param name="barcodeType">Barcode type</param>
        /// <param name="height">Barcode height</param>
        /// <param name="width">Barcode width</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintBarcodeAsync(string data, BarcodeType barcodeType = BarcodeType.Code128, int height = 100, int width = 2)
        {
            if (_currentPrinterService == null)
                throw new InvalidOperationException("No ESC/POS printer connected. Call ConnectAsync first.");

            return await _currentPrinterService.PrintBarcodeAsync(data, barcodeType, height, width);
        }

        /// <summary>
        /// Print QR code (ESC/POS printers)
        /// </summary>
        /// <param name="data">QR code data</param>
        /// <param name="size">QR code size</param>
        /// <param name="errorLevel">Error correction level</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintQrCodeAsync(string data, int size = 3, QrCodeErrorLevel errorLevel = QrCodeErrorLevel.L)
        {
            if (_currentPrinterService == null)
                throw new InvalidOperationException("No ESC/POS printer connected. Call ConnectAsync first.");

            return await _currentPrinterService.PrintQrCodeAsync(data, size, errorLevel);
        }

        /// <summary>
        /// Print image (ESC/POS printers)
        /// </summary>
        /// <param name="imageData">Image data</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintImageAsync(byte[] imageData, int width, int height)
        {
            if (_currentPrinterService == null)
                throw new InvalidOperationException("No ESC/POS printer connected. Call ConnectAsync first.");

            return await _currentPrinterService.PrintImageAsync(imageData, width, height);
        }

        /// <summary>
        /// Print line break (ESC/POS printers)
        /// </summary>
        /// <param name="lines">Number of lines to advance</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintLineBreakAsync(int lines = 1)
        {
            if (_currentPrinterService == null)
                throw new InvalidOperationException("No ESC/POS printer connected. Call ConnectAsync first.");

            return await _currentPrinterService.PrintLineBreakAsync(lines);
        }

        /// <summary>
        /// Cut paper (ESC/POS printers)
        /// </summary>
        /// <returns>True if cut successful</returns>
        public async Task<bool> CutPaperAsync()
        {
            if (_currentPrinterService == null)
                throw new InvalidOperationException("No ESC/POS printer connected. Call ConnectAsync first.");

            return await _currentPrinterService.CutPaperAsync();
        }

        /// <summary>
        /// Print data (ESC/POS printers)
        /// </summary>
        /// <param name="printData">Print data list</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintDataAsync(List<PrintDataModel> printData)
        {
            if (_currentPrinterService == null)
                throw new InvalidOperationException("No ESC/POS printer connected. Call ConnectAsync first.");

            return await _currentPrinterService.PrintDataAsync(printData);
        }

        /// <summary>
        /// Print ZPL label (Zebra printers)
        /// </summary>
        /// <param name="zplCommands">ZPL commands to print</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintZplLabelAsync(string zplCommands)
        {
            if (_currentZebraService == null)
                throw new InvalidOperationException("No Zebra printer connected. Call ConnectAsync first.");

            return await _currentZebraService.PrintLabelAsync(zplCommands);
        }

        /// <summary>
        /// Print ZPL label with data (Zebra printers)
        /// </summary>
        /// <param name="template">ZPL template</param>
        /// <param name="data">Data to replace in template</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintZplLabelWithDataAsync(string template, Dictionary<string, string> data)
        {
            if (_currentZebraService == null)
                throw new InvalidOperationException("No Zebra printer connected. Call ConnectAsync first.");

            return await _currentZebraService.PrintLabelWithDataAsync(template, data);
        }

        /// <summary>
        /// Get printer status (Zebra printers)
        /// </summary>
        /// <returns>Printer status information</returns>
        public async Task<PrinterStatusModel> GetZebraPrinterStatusAsync()
        {
            if (_currentZebraService == null)
                throw new InvalidOperationException("No Zebra printer connected. Call ConnectAsync first.");

            return await _currentZebraService.GetPrinterStatusAsync();
        }

        /// <summary>
        /// Check if Zebra printer is ready
        /// </summary>
        /// <returns>True if printer is ready</returns>
        public async Task<bool> IsZebraPrinterReadyAsync()
        {
            if (_currentZebraService == null)
                return false;

            return await _currentZebraService.IsPrinterReadyAsync();
        }

        /// <summary>
        /// Calibrate Zebra printer
        /// </summary>
        /// <returns>True if calibration successful</returns>
        public async Task<bool> CalibrateZebraPrinterAsync()
        {
            if (_currentZebraService == null)
                throw new InvalidOperationException("No Zebra printer connected. Call ConnectAsync first.");

            return await _currentZebraService.CalibratePrinterAsync();
        }

        /// <summary>
        /// Print test label (Zebra printers)
        /// </summary>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintZebraTestLabelAsync()
        {
            if (_currentZebraService == null)
                throw new InvalidOperationException("No Zebra printer connected. Call ConnectAsync first.");

            return await _currentZebraService.PrintTestLabelAsync();
        }

        /// <summary>
        /// Set Zebra print darkness
        /// </summary>
        /// <param name="darkness">Darkness level (0-30)</param>
        /// <returns>True if setting successful</returns>
        public async Task<bool> SetZebraPrintDarknessAsync(int darkness)
        {
            if (_currentZebraService == null)
                throw new InvalidOperationException("No Zebra printer connected. Call ConnectAsync first.");

            return await _currentZebraService.SetPrintDarknessAsync(darkness);
        }

        /// <summary>
        /// Set Zebra print speed
        /// </summary>
        /// <param name="speed">Print speed (1-14)</param>
        /// <returns>True if setting successful</returns>
        public async Task<bool> SetZebraPrintSpeedAsync(int speed)
        {
            if (_currentZebraService == null)
                throw new InvalidOperationException("No Zebra printer connected. Call ConnectAsync first.");

            return await _currentZebraService.SetPrintSpeedAsync(speed);
        }

        /// <summary>
        /// Set Zebra label dimensions
        /// </summary>
        /// <param name="width">Label width in dots</param>
        /// <param name="length">Label length in dots</param>
        /// <returns>True if setting successful</returns>
        public async Task<bool> SetZebraLabelDimensionsAsync(int width, int length)
        {
            if (_currentZebraService == null)
                throw new InvalidOperationException("No Zebra printer connected. Call ConnectAsync first.");

            return await _currentZebraService.SetLabelDimensionsAsync(width, length);
        }

        /// <summary>
        /// Get Zebra printer settings
        /// </summary>
        /// <returns>Printer settings</returns>
        public async Task<PrinterSettingsModel> GetZebraPrinterSettingsAsync()
        {
            if (_currentZebraService == null)
                throw new InvalidOperationException("No Zebra printer connected. Call ConnectAsync first.");

            return await _currentZebraService.GetPrinterSettingsAsync();
        }

        /// <summary>
        /// Get printer status (ESC/POS printers)
        /// </summary>
        /// <returns>Printer status</returns>
        public async Task<PrinterStatus> GetPrinterStatusAsync()
        {
            if (_currentPrinterService == null)
                throw new InvalidOperationException("No ESC/POS printer connected. Call ConnectAsync first.");

            return await _currentPrinterService.GetPrinterStatusAsync();
        }

        /// <summary>
        /// Set printer darkness (ESC/POS printers)
        /// </summary>
        /// <param name="value">Darkness value</param>
        /// <returns>True if setting successful</returns>
        public async Task<bool> SetPrinterDarknessAsync(int value)
        {
            if (_currentPrinterService == null)
                throw new InvalidOperationException("No ESC/POS printer connected. Call ConnectAsync first.");

            return await _currentPrinterService.SetPrinterDarknessAsync(value);
        }

        /// <summary>
        /// Get current connection state
        /// </summary>
        /// <returns>Current connection state</returns>
        public ConnectionState GetConnectionState()
        {
            return _bluetoothService.GetConnectionState();
        }

        /// <summary>
        /// Get currently connected device
        /// </summary>
        /// <returns>Connected device or null</returns>
        public BluetoothDeviceModel? GetConnectedDevice()
        {
            return _connectedDevice;
        }

        /// <summary>
        /// Auto-detect printer type based on device name
        /// </summary>
        /// <param name="deviceName">Device name</param>
        /// <returns>Detected printer type</returns>
        public static PrinterType AutoDetectPrinterType(string deviceName)
        {
            if (string.IsNullOrEmpty(deviceName))
                return PrinterType.Unknown;

            var name = deviceName.ToLowerInvariant();

            // Zebra printer detection
            if (name.Contains("zebra") || name.Contains("zt") || name.Contains("zm") || 
                name.Contains("zq") || name.Contains("zr") || name.Contains("zs") ||
                name.Contains("zxp") || name.Contains("zxi") || name.Contains("zxd"))
            {
                return PrinterType.Zebra;
            }

            // ESC/POS printer detection
            if (name.Contains("esc") || name.Contains("pos") || name.Contains("thermal") ||
                name.Contains("receipt") || name.Contains("printer") || name.Contains("bt") ||
                name.Contains("bluetooth") || name.Contains("serial"))
            {
                return PrinterType.EscPos;
            }

            return PrinterType.Generic;
        }

        private IPrinterService CreatePrinterService(PrinterType printerType)
        {
            return printerType switch
            {
                PrinterType.EscPos => _serviceProvider.GetRequiredService<EscPosPrinterService>(),
                PrinterType.Generic => _serviceProvider.GetRequiredService<GenericPrinterService>(),
                _ => throw new NotSupportedException($"Printer type {printerType} is not supported for ESC/POS operations.")
            };
        }

        private IZebraPrinterService CreateZebraPrinterService()
        {
            return _serviceProvider.GetRequiredService<ZebraPrinterService>();
        }

        private void OnBluetoothConnectionStateChanged(object? sender, ConnectionStateChangedEventArgs e)
        {
            ConnectionStateChanged?.Invoke(this, e);
        }

        private void OnBluetoothDeviceDiscovered(object? sender, BluetoothDeviceModel device)
        {
            DeviceDiscovered?.Invoke(this, device);
        }

        private void OnPrintJobStatusChanged(object? sender, PrintJobStatusChangedEventArgs e)
        {
            PrintJobStatusChanged?.Invoke(this, e);
        }

        private void OnPrinterStatusChanged(object? sender, PrinterStatusChangedEventArgs e)
        {
            PrinterStatusChanged?.Invoke(this, e);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_currentPrinterService != null)
                {
                    _currentPrinterService.PrintJobStatusChanged -= OnPrintJobStatusChanged;
                    _currentPrinterService = null;
                }

                if (_currentZebraService != null)
                {
                    _currentZebraService.PrinterStatusChanged -= OnPrinterStatusChanged;
                    _currentZebraService = null;
                }

                if (_bluetoothService != null)
                {
                    _bluetoothService.ConnectionStateChanged -= OnBluetoothConnectionStateChanged;
                    _bluetoothService.DeviceDiscovered -= OnBluetoothDeviceDiscovered;
                }

                _disposed = true;
            }
        }
    }
} 