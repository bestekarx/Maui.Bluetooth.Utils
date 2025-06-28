using Maui.Bluetooth.Utils.Shared.Interfaces;
using Maui.Bluetooth.Utils.Shared.Models;
using System.Text;

namespace Maui.Bluetooth.Utils.Shared.Services
{
    /// <summary>
    /// Zebra printer service implementation
    /// </summary>
    public class ZebraPrinterService : IZebraPrinterService
    {
        private readonly IBluetoothService _bluetoothService;
        private PrinterStatusModel _currentStatus;
        private PrinterSettingsModel _currentSettings;

        public event EventHandler<PrinterStatusChangedEventArgs>? PrinterStatusChanged;

        public ZebraPrinterService(IBluetoothService bluetoothService)
        {
            _bluetoothService = bluetoothService ?? throw new ArgumentNullException(nameof(bluetoothService));
            _currentStatus = new PrinterStatusModel();
            _currentSettings = new PrinterSettingsModel
            {
                PrintDarkness = 10,
                PrintSpeed = 3,
                LabelWidth = 609, // 4 inch at 203 DPI
                LabelLength = 609,
                MediaType = "Continuous",
                SensorType = "Gap",
                PrintMode = PrintMode.TearOff,
                PrintOrientation = PrintOrientation.Normal
            };
        }

        public async Task<bool> PrintLabelAsync(string zplCommands)
        {
            try
            {
                if (!await IsPrinterReadyAsync())
                {
                    throw new InvalidOperationException("Printer is not ready");
                }

                // ZPL komutlarını byte array'e çevir
                var data = Encoding.UTF8.GetBytes(zplCommands);
                
                // Bluetooth üzerinden gönder
                var result = await _bluetoothService.SendDataAsync(data);
                
                if (result)
                {
                    // Print status'u güncelle
                    await UpdatePrinterStatusAsync();
                }

                return result;
            }
            catch (Exception ex)
            {
                _currentStatus.ErrorMessage = ex.Message;
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, ex.Message));
                return false;
            }
        }

        public async Task<bool> PrintLabelWithDataAsync(string template, Dictionary<string, string> data)
        {
            try
            {
                var zplCommands = template;
                
                // Template'deki placeholder'ları data ile değiştir
                foreach (var kvp in data)
                {
                    zplCommands = zplCommands.Replace($"{{{kvp.Key}}}", kvp.Value);
                }

                return await PrintLabelAsync(zplCommands);
            }
            catch (Exception ex)
            {
                _currentStatus.ErrorMessage = ex.Message;
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, ex.Message));
                return false;
            }
        }

        public async Task<PrinterStatusModel> GetPrinterStatusAsync()
        {
            try
            {
                // Zebra printer status komutu gönder
                var statusCommand = "~HS\r\n";
                var data = Encoding.UTF8.GetBytes(statusCommand);
                
                await _bluetoothService.SendDataAsync(data);
                
                // Status response'unu bekle ve parse et
                await Task.Delay(1000); // Response için bekle
                
                return _currentStatus;
            }
            catch (Exception ex)
            {
                _currentStatus.ErrorMessage = ex.Message;
                return _currentStatus;
            }
        }

        public async Task<bool> IsPrinterReadyAsync()
        {
            try
            {
                var status = await GetPrinterStatusAsync();
                return status.IsReady && !status.IsPaused && !status.IsWaitingForLabel && !status.IsWaitingForRibbon && !status.IsWaitingForMedia;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CalibratePrinterAsync()
        {
            try
            {
                var calibrateCommand = "~JC\r\n";
                var data = Encoding.UTF8.GetBytes(calibrateCommand);
                
                return await _bluetoothService.SendDataAsync(data);
            }
            catch (Exception ex)
            {
                _currentStatus.ErrorMessage = ex.Message;
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, ex.Message));
                return false;
            }
        }

        public async Task<bool> PrintTestLabelAsync()
        {
            try
            {
                var testLabel = GenerateTestLabel();
                return await PrintLabelAsync(testLabel);
            }
            catch (Exception ex)
            {
                _currentStatus.ErrorMessage = ex.Message;
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, ex.Message));
                return false;
            }
        }

        public async Task<bool> SetPrintDarknessAsync(int darkness)
        {
            try
            {
                if (darkness < 0 || darkness > 30)
                    throw new ArgumentOutOfRangeException(nameof(darkness), "Darkness must be between 0 and 30");

                var command = $"~SD{darkness:D2}\r\n";
                var data = Encoding.UTF8.GetBytes(command);
                
                var result = await _bluetoothService.SendDataAsync(data);
                
                if (result)
                {
                    _currentSettings.PrintDarkness = darkness;
                }

                return result;
            }
            catch (Exception ex)
            {
                _currentStatus.ErrorMessage = ex.Message;
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, ex.Message));
                return false;
            }
        }

        public async Task<bool> SetPrintSpeedAsync(int speed)
        {
            try
            {
                if (speed < 1 || speed > 14)
                    throw new ArgumentOutOfRangeException(nameof(speed), "Speed must be between 1 and 14");

                var command = $"^PR{speed:D2}\r\n";
                var data = Encoding.UTF8.GetBytes(command);
                
                var result = await _bluetoothService.SendDataAsync(data);
                
                if (result)
                {
                    _currentSettings.PrintSpeed = speed;
                }

                return result;
            }
            catch (Exception ex)
            {
                _currentStatus.ErrorMessage = ex.Message;
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, ex.Message));
                return false;
            }
        }

        public async Task<bool> SetLabelDimensionsAsync(int width, int length)
        {
            try
            {
                var command = $"^PW{width}\r\n^LL{length}\r\n";
                var data = Encoding.UTF8.GetBytes(command);
                
                var result = await _bluetoothService.SendDataAsync(data);
                
                if (result)
                {
                    _currentSettings.LabelWidth = width;
                    _currentSettings.LabelLength = length;
                }

                return result;
            }
            catch (Exception ex)
            {
                _currentStatus.ErrorMessage = ex.Message;
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, ex.Message));
                return false;
            }
        }

        public async Task<PrinterSettingsModel> GetPrinterSettingsAsync()
        {
            try
            {
                // Printer settings komutları gönder
                var settingsCommands = new[]
                {
                    "~SD\r\n", // Print darkness
                    "^PR\r\n", // Print speed
                    "^PW\r\n", // Print width
                    "^LL\r\n"  // Label length
                };

                foreach (var command in settingsCommands)
                {
                    var data = Encoding.UTF8.GetBytes(command);
                    await _bluetoothService.SendDataAsync(data);
                    await Task.Delay(100); // Response için bekle
                }

                return _currentSettings;
            }
            catch (Exception ex)
            {
                _currentStatus.ErrorMessage = ex.Message;
                return _currentSettings;
            }
        }

        private async Task UpdatePrinterStatusAsync()
        {
            var previousStatus = _currentStatus;
            _currentStatus = await GetPrinterStatusAsync();
            
            if (!previousStatus.Equals(_currentStatus))
            {
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(previousStatus, _currentStatus));
            }
        }

        private string GenerateTestLabel()
        {
            var sb = new StringBuilder();
            
            // Test label ZPL komutları
            sb.AppendLine("^XA"); // Start of label
            sb.AppendLine("^FO50,50^A0N,50,50^FDZebra Test Label^FS"); // Text
            sb.AppendLine("^FO50,120^BY3^BCN,100,Y,N,N^FD123456789^FS"); // Barcode
            sb.AppendLine("^FO50,250^A0N,30,30^FDDate: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "^FS"); // Date
            sb.AppendLine("^XZ"); // End of label
            
            return sb.ToString();
        }

        /// <summary>
        /// Generate ZPL commands for text
        /// </summary>
        public static string GenerateTextZpl(string text, int x, int y, string font = "0", int fontSize = 10, int rotation = 0)
        {
            return $"^FO{x},{y}^A{font}N,{fontSize},{fontSize}^FD{text}^FS";
        }

        /// <summary>
        /// Generate ZPL commands for barcode
        /// </summary>
        public static string GenerateBarcodeZpl(string data, int x, int y, string barcodeType = "1", int height = 50, int width = 2, int rotation = 0)
        {
            return $"^FO{x},{y}^BY{width}^BCN,{height},Y,N,N^FD{data}^FS";
        }

        /// <summary>
        /// Generate ZPL commands for QR code
        /// </summary>
        public static string GenerateQrCodeZpl(string data, int x, int y, int size = 3, string errorLevel = "L", int rotation = 0)
        {
            return $"^FO{x},{y}^BQN,2,{size},{errorLevel}^FD{data}^FS";
        }

        /// <summary>
        /// Generate ZPL commands for line
        /// </summary>
        public static string GenerateLineZpl(int x1, int y1, int x2, int y2, int thickness = 1)
        {
            return $"^FO{x1},{y1}^GB{x2 - x1},{y2 - y1},{thickness}^FS";
        }

        /// <summary>
        /// Generate ZPL commands for rectangle
        /// </summary>
        public static string GenerateRectangleZpl(int x, int y, int width, int height, int thickness = 1)
        {
            return $"^FO{x},{y}^GB{width},{height},{thickness}^FS";
        }
    }
}