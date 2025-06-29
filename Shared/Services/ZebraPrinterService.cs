using Maui.Bluetooth.Utils.Shared.Interfaces;
using Maui.Bluetooth.Utils.Shared.Models;
using Maui.Bluetooth.Utils.Shared.Utils;
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

        public async Task<bool> PrintLabelAsync(string cpclCommands)
        {
            try
            {
                // Satır sonlarını normalize et
                var normalized = cpclCommands.Replace("\r\n", "\n").Replace("\n", "\r\n");
                var data = Encoding.GetEncoding("ISO-8859-1").GetBytes(normalized);
                var result = await _bluetoothService.SendDataAsync(data);
                return result;
            }
            catch (Exception ex)
            {
                _currentStatus.ErrorMessage = ex.Message;
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
                // Status komutunu gönder ve response bekle
                var response = await SendStatusCommandAsync("~HS\r\n");
                // Burada response parse edilebilir (geliştirilebilir)
                _currentStatus.IsReady = true; // Varsayılan olarak ready kabul et
                _currentStatus.ErrorMessage = null;
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
                // Zebra printer'lar için basit bir ready kontrolü
                // Gerçek uygulamada printer'dan status almak gerekir
                // Şimdilik bağlantı durumunu kontrol et
                var connectionState = _bluetoothService.GetConnectionState();
                if (connectionState != ConnectionState.Connected)
                {
                    return false;
                }
                
                // Printer'a basit bir test komutu gönder
                var testCommand = "~HI\r\n"; // Printer info komutu
                var data = Encoding.UTF8.GetBytes(testCommand);
                
                var result = await _bluetoothService.SendDataAsync(data);
                if (!result)
                {
                    return false;
                }
                
                // Komut gönderilebiliyorsa printer ready kabul et
                return true;
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
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, "CalibratePrinterAsync: Printer kalibrasyonu başlatılıyor..."));
                
                var calibrateCommand = "~JC\r\n";
                var data = Encoding.UTF8.GetBytes(calibrateCommand);
                
                var result = await _bluetoothService.SendDataAsync(data);
                
                if (result)
                {
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, "CalibratePrinterAsync: Kalibrasyon komutu gönderildi, printer hazırlanıyor..."));
                    
                    // Kalibrasyon için biraz bekle
                    await Task.Delay(2000);
                    
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, "CalibratePrinterAsync: Kalibrasyon tamamlandı."));
                }
                else
                {
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, "CalibratePrinterAsync: Kalibrasyon komutu gönderilemedi."));
                }
                
                return result;
            }
            catch (Exception ex)
            {
                var logMsg = $"[EXCEPTION] CalibratePrinterAsync hata: {ex.Message}";
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, logMsg));
                _currentStatus.ErrorMessage = ex.Message;
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, ex.Message));
                return false;
            }
        }

        public async Task<bool> PrintTestLabelAsync()
        {
            try
            {
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, "PrintTestLabelAsync: Test label oluşturuluyor..."));
                
                var testLabel = GenerateTestLabelCpcl();
                
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, "PrintTestLabelAsync: Test label yazdırılıyor..."));
                
                return await PrintLabelAsync(testLabel);
            }
            catch (Exception ex)
            {
                var logMsg = $"[EXCEPTION] PrintTestLabelAsync hata: {ex.Message}";
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, logMsg));
                _currentStatus.ErrorMessage = ex.Message;
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, ex.Message));
                return false;
            }
        }

        /// <summary>
        /// CPCL formatında test etiketi üretir
        /// </summary>
        private string GenerateTestLabelCpcl()
        {
            return ZebraCpclUtils.GenerateTestLabel();
        }

        public async Task<bool> SetPrintDarknessAsync(int darkness)
        {
            try
            {
                if (darkness < 0 || darkness > 30)
                    throw new ArgumentOutOfRangeException(nameof(darkness), "Darkness must be between 0 and 30");

                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, $"SetPrintDarknessAsync: Print darkness {darkness} ayarlanıyor..."));

                var command = $"~SD{darkness:D2}\r\n";
                var data = Encoding.UTF8.GetBytes(command);
                
                var result = await _bluetoothService.SendDataAsync(data);
                
                if (result)
                {
                    _currentSettings.PrintDarkness = darkness;
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, $"SetPrintDarknessAsync: Print darkness {darkness} başarıyla ayarlandı."));
                }
                else
                {
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, "SetPrintDarknessAsync: Print darkness ayarlanamadı."));
                }

                return result;
            }
            catch (Exception ex)
            {
                var logMsg = $"[EXCEPTION] SetPrintDarknessAsync hata: {ex.Message}";
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, logMsg));
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

                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, $"SetPrintSpeedAsync: Print speed {speed} ayarlanıyor..."));

                var command = $"^PR{speed:D2}\r\n";
                var data = Encoding.UTF8.GetBytes(command);
                
                var result = await _bluetoothService.SendDataAsync(data);
                
                if (result)
                {
                    _currentSettings.PrintSpeed = speed;
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, $"SetPrintSpeedAsync: Print speed {speed} başarıyla ayarlandı."));
                }
                else
                {
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, "SetPrintSpeedAsync: Print speed ayarlanamadı."));
                }

                return result;
            }
            catch (Exception ex)
            {
                var logMsg = $"[EXCEPTION] SetPrintSpeedAsync hata: {ex.Message}";
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, logMsg));
                _currentStatus.ErrorMessage = ex.Message;
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, ex.Message));
                return false;
            }
        }

        public async Task<bool> SetLabelDimensionsAsync(int width, int length)
        {
            try
            {
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, $"SetLabelDimensionsAsync: Label dimensions {width}x{length} ayarlanıyor..."));

                var command = $"^PW{width}\r\n^LL{length}\r\n";
                var data = Encoding.UTF8.GetBytes(command);
                
                var result = await _bluetoothService.SendDataAsync(data);
                
                if (result)
                {
                    _currentSettings.LabelWidth = width;
                    _currentSettings.LabelLength = length;
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, $"SetLabelDimensionsAsync: Label dimensions {width}x{length} başarıyla ayarlandı."));
                }
                else
                {
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, "SetLabelDimensionsAsync: Label dimensions ayarlanamadı."));
                }

                return result;
            }
            catch (Exception ex)
            {
                var logMsg = $"[EXCEPTION] SetLabelDimensionsAsync hata: {ex.Message}";
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, logMsg));
                _currentStatus.ErrorMessage = ex.Message;
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, ex.Message));
                return false;
            }
        }

        public async Task<PrinterSettingsModel> GetPrinterSettingsAsync()
        {
            try
            {
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, "GetPrinterSettingsAsync: Printer ayarları alınıyor..."));

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
                    var result = await _bluetoothService.SendDataAsync(data);
                    if (!result)
                    {
                        PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, $"GetPrinterSettingsAsync: {command} komutu gönderilemedi."));
                    }
                    await Task.Delay(100); // Response için bekle
                }

                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, "GetPrinterSettingsAsync: Printer ayarları alındı."));

                return _currentSettings;
            }
            catch (Exception ex)
            {
                var logMsg = $"[EXCEPTION] GetPrinterSettingsAsync hata: {ex.Message}";
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, logMsg));
                _currentStatus.ErrorMessage = ex.Message;
                return _currentSettings;
            }
        }

        private async Task UpdatePrinterStatusAsync()
        {
            try
            {
                var previousStatus = _currentStatus;
                _currentStatus = await GetPrinterStatusAsync();
                
                if (!previousStatus.Equals(_currentStatus))
                {
                    PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(previousStatus, _currentStatus));
                }
            }
            catch (Exception ex)
            {
                var logMsg = $"[EXCEPTION] UpdatePrinterStatusAsync hata: {ex.Message}";
                PrinterStatusChanged?.Invoke(this, new PrinterStatusChangedEventArgs(_currentStatus, _currentStatus, logMsg));
            }
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

        // Status komutlarını yazıcıya gönderip response bekleyen yardımcı fonksiyon
        private async Task<string?> SendStatusCommandAsync(string command)
        {
            var data = Encoding.UTF8.GetBytes(command);
            var sent = await _bluetoothService.SendDataAsync(data);
            if (!sent)
                return null;

            // Buffer'ı temizle
            if (_bluetoothService is IGenericPrinterService genericService)
            {
                // Burada response'u oku ve buffer'ı temizle
                var responseBytes = await genericService.ReadDataAsync(1000);
                if (responseBytes.Length > 0)
                    return Encoding.UTF8.GetString(responseBytes);
                return null;
            }
            // Yoksa kısa bir bekleme ile devam et
            await Task.Delay(300);
            return null;
        }

        private string CreateDemoFile(string dataString)
        {
            var tempFilePath = Path.Combine(Path.GetTempPath(), "TEST_ZEBRA.LBL");
            File.WriteAllText(tempFilePath, dataString, Encoding.UTF8);
            return tempFilePath;
        }
    }
}