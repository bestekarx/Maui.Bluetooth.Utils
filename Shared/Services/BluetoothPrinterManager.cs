using Maui.Bluetooth.Utils.Shared.Interfaces;
using Maui.Bluetooth.Utils.Shared.Models;
using Maui.Bluetooth.Utils.Shared.Utils;
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
        public async Task<bool> PrintQrCodeAsync(string data, int size = 6)
        {
            if (_currentPrinterService == null)
                throw new InvalidOperationException("No ESC/POS printer connected. Call ConnectAsync first.");

            return await _currentPrinterService.PrintQrCodeAsync(data, size);
        }

        /// <summary>
        /// Print simple barcode for ESC/POS printers (improved compatibility)
        /// </summary>
        /// <param name="data">Barcode data</param>
        /// <param name="barcodeType">Barcode type</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintSimpleBarcodeAsync(string data, BarcodeType barcodeType = BarcodeType.Code128)
        {
            if (_currentPrinterService == null)
                throw new InvalidOperationException("No ESC/POS printer connected. Call ConnectAsync first.");

            if (_currentPrinterService is IEscPosPrinterService escPosService)
            {
                return await escPosService.PrintSimpleBarcodeAsync(data, barcodeType);
            }

            // Fallback to regular method
            return await _currentPrinterService.PrintBarcodeAsync(data, barcodeType);
        }

        /// <summary>
        /// Print simple QR code for ESC/POS printers (improved compatibility)
        /// </summary>
        /// <param name="data">QR code data</param>
        /// <param name="size">QR code size</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintSimpleQrCodeAsync(string data, int size = 6)
        {
            if (_currentPrinterService == null)
                throw new InvalidOperationException("No ESC/POS printer connected. Call ConnectAsync first.");

            if (_currentPrinterService is IEscPosPrinterService escPosService)
            {
                return await escPosService.PrintSimpleQrCodeAsync(data, size);
            }

            // Fallback to regular method
            return await _currentPrinterService.PrintQrCodeAsync(data, size);
        }

        /// <summary>
        /// Print alternative QR code for ESC/POS printers (more reliable)
        /// </summary>
        /// <param name="data">QR code data</param>
        /// <param name="size">QR code size</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintQrCodeAlternativeAsync(string data, int size = 6)
        {
            if (_currentPrinterService == null)
                throw new InvalidOperationException("No ESC/POS printer connected. Call ConnectAsync first.");

            if (_currentPrinterService is IEscPosPrinterService escPosService)
            {
                return await escPosService.PrintQrCodeAlternativeAsync(data, size);
            }

            // Fallback to regular method
            return await _currentPrinterService.PrintQrCodeAsync(data, size);
        }

        /// <summary>
        /// Print very simple QR code for ESC/POS printers (most compatible)
        /// </summary>
        /// <param name="data">QR code data</param>
        /// <param name="size">QR code size</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintQrCodeVerySimpleAsync(string data, int size = 4)
        {
            if (_currentPrinterService == null)
                throw new InvalidOperationException("No ESC/POS printer connected. Call ConnectAsync first.");

            if (_currentPrinterService is IEscPosPrinterService escPosService)
            {
                return await escPosService.PrintQrCodeVerySimpleAsync(data, size);
            }

            // Fallback to regular method
            return await _currentPrinterService.PrintQrCodeAsync(data, size);
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
        /// Print CPCL label (Zebra printers)
        /// </summary>
        /// <param name="cpclCommands">CPCL commands to print</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintCpclLabelAsync(string cpclCommands)
        {
            if (_currentZebraService == null)
                throw new InvalidOperationException("No Zebra printer connected. Call ConnectAsync first.");

            return await _currentZebraService.PrintLabelAsync(cpclCommands);
        }

        /// <summary>
        /// Print text label using CPCL (Zebra printers)
        /// </summary>
        /// <param name="text">Text to print</param>
        /// <param name="x">X position (default: 30)</param>
        /// <param name="y">Y position (default: 40)</param>
        /// <param name="font">Font number (default: 4)</param>
        /// <param name="fontSize">Font size (default: 0)</param>
        /// <param name="feedLines">Number of feed lines (default: 5)</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintZebraTextLabelAsync(string text, int x = 30, int y = 40, int font = 4, int fontSize = 0, int feedLines = 5)
        {
            if (_currentZebraService == null)
                throw new InvalidOperationException("No Zebra printer connected. Call ConnectAsync first.");
            var cpclCommands = ZebraCpclUtils.GenerateTextLabel(text, x, y, font, fontSize, feedLines);
            return await _currentZebraService.PrintLabelAsync(cpclCommands);
        }

        /// <summary>
        /// Print barcode label using CPCL (Zebra printers)
        /// </summary>
        /// <param name="barcode">Barcode data</param>
        /// <param name="label">Optional text label</param>
        /// <param name="x">X position (default: 30)</param>
        /// <param name="y">Y position (default: 40)</param>
        /// <param name="barcodeType">Barcode type (default: 128)</param>
        /// <param name="height">Barcode height (default: 50)</param>
        /// <param name="width">Barcode width (default: 1)</param>
        /// <param name="font">Font number (default: 4)</param>
        /// <param name="fontSize">Font size (default: 0)</param>
        /// <param name="labelAbove">Whether label is above barcode (default: false)</param>
        /// <param name="feedLines">Number of feed lines (default: 5)</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintZebraBarcodeLabelAsync(string barcode, string label = "", int x = 30, int y = 40, int barcodeType = 128, int height = 50, int width = 1, int font = 4, int fontSize = 0, bool labelAbove = false, int feedLines = 5)
        {
            if (_currentZebraService == null)
                throw new InvalidOperationException("No Zebra printer connected. Call ConnectAsync first.");
            var cpclCommands = ZebraCpclUtils.GenerateBarcodeLabel(barcode, label, x, y, barcodeType, height, width, font, fontSize, labelAbove, feedLines);
            return await _currentZebraService.PrintLabelAsync(cpclCommands);
        }

        /// <summary>
        /// Print QR code label using CPCL (Zebra printers)
        /// </summary>
        /// <param name="qrData">QR code data</param>
        /// <param name="label">Optional text label</param>
        /// <param name="x">X position (default: 50)</param>
        /// <param name="y">Y position (default: 100)</param>
        /// <param name="size">QR code size (default: 2)</param>
        /// <param name="errorLevel">Error correction level (default: M)</param>
        /// <param name="font">Font number (default: 4)</param>
        /// <param name="fontSize">Font size (default: 0)</param>
        /// <param name="labelYOffset">Y position offset for label (default: -60)</param>
        /// <param name="feedLines">Number of feed lines (default: 5)</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintZebraQrLabelAsync(string qrData, string label = "", int x = 50, int y = 100, int size = 2, string errorLevel = "M", int font = 4, int fontSize = 0, int labelYOffset = -60, int feedLines = 5)
        {
            if (_currentZebraService == null)
                throw new InvalidOperationException("No Zebra printer connected. Call ConnectAsync first.");
            var cpclCommands = ZebraCpclUtils.GenerateQrLabel(qrData, label, x, y, size, errorLevel, font, fontSize, labelYOffset, feedLines);
            return await _currentZebraService.PrintLabelAsync(cpclCommands);
        }

        /// <summary>
        /// Print receipt-style label using CPCL (Zebra printers)
        /// </summary>
        /// <param name="title">Receipt title</param>
        /// <param name="items">List of items to print</param>
        /// <param name="total">Total amount</param>
        /// <param name="x">X position (default: 30)</param>
        /// <param name="y">Y position (default: 40)</param>
        /// <param name="font">Font number (default: 4)</param>
        /// <param name="fontSize">Font size (default: 0)</param>
        /// <param name="lineHeight">Line height (default: 40)</param>
        /// <param name="feedLines">Number of feed lines (default: 5)</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintZebraReceiptLabelAsync(string title, List<(string Name, decimal Price)> items, decimal total, int x = 30, int y = 40, int font = 4, int fontSize = 0, int lineHeight = 40, int feedLines = 5)
        {
            if (_currentZebraService == null)
                throw new InvalidOperationException("No Zebra printer connected. Call ConnectAsync first.");
            var cpclCommands = ZebraCpclUtils.GenerateReceiptLabel(title, items, total, x, y, font, fontSize, lineHeight, feedLines);
            return await _currentZebraService.PrintLabelAsync(cpclCommands);
        }

        /// <summary>
        /// Print address label using CPCL (Zebra printers)
        /// </summary>
        /// <param name="name">Recipient name</param>
        /// <param name="address">Street address</param>
        /// <param name="city">City</param>
        /// <param name="postalCode">Postal code</param>
        /// <param name="country">Country</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintZebraAddressLabelAsync(string name, string address, string city, string postalCode, string country)
        {
            if (_currentZebraService == null)
                throw new InvalidOperationException("No Zebra printer connected. Call ConnectAsync first.");

            var cpclCommands = ZebraCpclUtils.GenerateAddressLabel(name, address, city, postalCode, country);
            return await _currentZebraService.PrintLabelAsync(cpclCommands);
        }

        /// <summary>
        /// Print product label using CPCL (Zebra printers)
        /// </summary>
        /// <param name="productName">Product name</param>
        /// <param name="productCode">Product code/barcode</param>
        /// <param name="price">Product price</param>
        /// <param name="description">Product description</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintZebraProductLabelAsync(string productName, string productCode, decimal price, string description = "")
        {
            if (_currentZebraService == null)
                throw new InvalidOperationException("No Zebra printer connected. Call ConnectAsync first.");

            var cpclCommands = ZebraCpclUtils.GenerateProductLabel(productName, productCode, price, description);
            return await _currentZebraService.PrintLabelAsync(cpclCommands);
        }

        /// <summary>
        /// Print shipping label using CPCL (Zebra printers)
        /// </summary>
        /// <param name="trackingNumber">Tracking number</param>
        /// <param name="fromAddress">Sender address</param>
        /// <param name="toAddress">Recipient address</param>
        /// <param name="weight">Package weight</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintZebraShippingLabelAsync(string trackingNumber, string fromAddress, string toAddress, string weight)
        {
            if (_currentZebraService == null)
                throw new InvalidOperationException("No Zebra printer connected. Call ConnectAsync first.");

            var cpclCommands = ZebraCpclUtils.GenerateShippingLabel(trackingNumber, fromAddress, toAddress, weight);
            return await _currentZebraService.PrintLabelAsync(cpclCommands);
        }

        /// <summary>
        /// Print inventory label using CPCL (Zebra printers)
        /// </summary>
        /// <param name="itemCode">Item code</param>
        /// <param name="itemName">Item name</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="location">Storage location</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintZebraInventoryLabelAsync(string itemCode, string itemName, int quantity, string location)
        {
            if (_currentZebraService == null)
                throw new InvalidOperationException("No Zebra printer connected. Call ConnectAsync first.");

            var cpclCommands = ZebraCpclUtils.GenerateInventoryLabel(itemCode, itemName, quantity, location);
            return await _currentZebraService.PrintLabelAsync(cpclCommands);
        }

        /// <summary>
        /// Print custom label using CPCL (Zebra printers)
        /// </summary>
        /// <param name="elements">List of CPCL elements to include</param>
        /// <param name="labelWidth">Label width in dots (default: 400)</param>
        /// <param name="labelLength">Label length in dots (default: 600)</param>
        /// <param name="feedLines">Number of feed lines (default: 5)</param>
        /// <returns>True if print successful</returns>
        public async Task<bool> PrintZebraCustomLabelAsync(List<string> elements, int labelWidth = 400, int labelLength = 600, int feedLines = 5)
        {
            if (_currentZebraService == null)
                throw new InvalidOperationException("No Zebra printer connected. Call ConnectAsync first.");
            var cpclCommands = ZebraCpclUtils.GenerateCustomLabel(elements, labelWidth, labelLength, feedLines);
            return await _currentZebraService.PrintLabelAsync(cpclCommands);
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