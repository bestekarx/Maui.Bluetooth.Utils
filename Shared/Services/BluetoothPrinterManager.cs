using Maui.Bluetooth.Utils.Shared.Interfaces;
using Maui.Bluetooth.Utils.Shared.Models;
using Microsoft.Extensions.DependencyInjection;
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
                _currentPrinterService = CreatePrinterService(device.PrinterType);

                // Initialize printer service
                if (_currentPrinterService != null)
                {
                    await _currentPrinterService.InitializeAsync();
                    
                    // Subscribe to print job events
                    _currentPrinterService.PrintJobStatusChanged += OnPrintJobStatusChanged;
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
        /// Print text
        /// </summary>
        /// <param name="text">Text to print</param>
        /// <param name="alignment">Text alignment</param>
        /// <param name="isBold">Whether text is bold</param>
        /// <param name="isUnderlined">Whether text is underlined</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintTextAsync(string text, TextAlignment alignment = TextAlignment.Left, bool isBold = false, bool isUnderlined = false)
        {
            if (_currentPrinterService == null)
                throw new InvalidOperationException("No printer connected. Call ConnectAsync first.");

            return await _currentPrinterService.PrintTextAsync(text, alignment, isBold, isUnderlined);
        }

        /// <summary>
        /// Print barcode
        /// </summary>
        /// <param name="data">Barcode data</param>
        /// <param name="barcodeType">Barcode type</param>
        /// <param name="height">Barcode height</param>
        /// <param name="width">Barcode width</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintBarcodeAsync(string data, BarcodeType barcodeType = BarcodeType.Code128, int height = 100, int width = 2)
        {
            if (_currentPrinterService == null)
                throw new InvalidOperationException("No printer connected. Call ConnectAsync first.");

            return await _currentPrinterService.PrintBarcodeAsync(data, barcodeType, height, width);
        }

        /// <summary>
        /// Print QR code
        /// </summary>
        /// <param name="data">QR code data</param>
        /// <param name="size">QR code size</param>
        /// <param name="errorLevel">Error correction level</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintQrCodeAsync(string data, int size = 3, QrCodeErrorLevel errorLevel = QrCodeErrorLevel.L)
        {
            if (_currentPrinterService == null)
                throw new InvalidOperationException("No printer connected. Call ConnectAsync first.");

            return await _currentPrinterService.PrintQrCodeAsync(data, size, errorLevel);
        }

        /// <summary>
        /// Print image
        /// </summary>
        /// <param name="imageData">Image data</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintImageAsync(byte[] imageData, int width, int height)
        {
            if (_currentPrinterService == null)
                throw new InvalidOperationException("No printer connected. Call ConnectAsync first.");

            return await _currentPrinterService.PrintImageAsync(imageData, width, height);
        }

        /// <summary>
        /// Print line break
        /// </summary>
        /// <param name="lines">Number of lines</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintLineBreakAsync(int lines = 1)
        {
            if (_currentPrinterService == null)
                throw new InvalidOperationException("No printer connected. Call ConnectAsync first.");

            return await _currentPrinterService.PrintLineBreakAsync(lines);
        }

        /// <summary>
        /// Cut paper
        /// </summary>
        /// <returns>True if cut successful</returns>
        public async Task<bool> CutPaperAsync()
        {
            if (_currentPrinterService == null)
                throw new InvalidOperationException("No printer connected. Call ConnectAsync first.");

            return await _currentPrinterService.CutPaperAsync();
        }

        /// <summary>
        /// Print multiple data items
        /// </summary>
        /// <param name="printData">List of print data</param>
        /// <returns>True if all items printed successfully</returns>
        public async Task<bool> PrintDataAsync(List<PrintDataModel> printData)
        {
            if (_currentPrinterService == null)
                throw new InvalidOperationException("No printer connected. Call ConnectAsync first.");

            return await _currentPrinterService.PrintDataAsync(printData);
        }

        /// <summary>
        /// Get printer status
        /// </summary>
        /// <returns>Printer status information</returns>
        public async Task<PrinterStatus> GetPrinterStatusAsync()
        {
            if (_currentPrinterService == null)
                throw new InvalidOperationException("No printer connected. Call ConnectAsync first.");

            return await _currentPrinterService.GetPrinterStatusAsync();
        }

        /// <summary>
        /// Set printer darkness/contrast
        /// </summary>
        /// <param name="value">Darkness value (0-255)</param>
        /// <returns>True if setting applied successfully</returns>
        public async Task<bool> SetPrinterDarknessAsync(int value)
        {
            if (_currentPrinterService == null)
                throw new InvalidOperationException("No printer connected. Call ConnectAsync first.");

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
            return _connectedDevice ?? _bluetoothService.GetConnectedDevice();
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
                name.Contains("zq") || name.Contains("zr") || name.Contains("ztc"))
            {
                return PrinterType.Zebra;
            }

            // ESC/POS printer detection
            if (name.Contains("pos") || name.Contains("esc") || name.Contains("thermal") ||
                name.Contains("receipt") || name.Contains("printer") || name.Contains("pt") ||
                name.Contains("tp") || name.Contains("brother"))
            {
                return PrinterType.EscPos;
            }

            return PrinterType.Generic;
        }

        private IPrinterService CreatePrinterService(PrinterType printerType)
        {
            return printerType switch
            {
                PrinterType.EscPos => _serviceProvider.GetRequiredService<IEscPosPrinterService>(),
                PrinterType.Zebra => _serviceProvider.GetRequiredService<IZebraPrinterService>(),
                PrinterType.Generic => _serviceProvider.GetRequiredService<IGenericPrinterService>(),
                _ => throw new NotSupportedException($"Printer type {printerType} is not supported.")
            };
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

        public void Dispose()
        {
            if (!_disposed)
            {
                _bluetoothService.ConnectionStateChanged -= OnBluetoothConnectionStateChanged;
                _bluetoothService.DeviceDiscovered -= OnBluetoothDeviceDiscovered;

                if (_currentPrinterService != null)
                {
                    _currentPrinterService.PrintJobStatusChanged -= OnPrintJobStatusChanged;
                }

                _disposed = true;
            }
        }
    }
} 