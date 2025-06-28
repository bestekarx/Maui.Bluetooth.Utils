using System.Text;
using Maui.Bluetooth.Utils.Shared.Interfaces;
using Maui.Bluetooth.Utils.Shared.Models;
using TextAlignment = Maui.Bluetooth.Utils.Shared.Models.TextAlignment;

namespace Maui.Bluetooth.Utils.Shared.Services
{
    /// <summary>
    /// Zebra printer service implementation for ZPL commands
    /// </summary>
    public class ZebraPrinterService : IZebraPrinterService
    {
        private readonly IBluetoothService _bluetoothService;
        private bool _isInitialized = false;
        private int _labelWidth = 203;
        private int _printSpeed = 2;
        private int _printDarkness = 10;
        private string _mediaType = "continuous";
        private string _sensorType = "gap";

        public event EventHandler<PrintJobStatusChangedEventArgs>? PrintJobStatusChanged;

        public ZebraPrinterService(IBluetoothService bluetoothService)
        {
            _bluetoothService = bluetoothService ?? throw new ArgumentNullException(nameof(bluetoothService));
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                await InitializeZebraAsync();
                _isInitialized = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> InitializeZebraAsync()
        {
            // ~HS - Host status
            var initCommand = "~HS";
            return await SendZplCommandAsync(initCommand);
        }

        public async Task<bool> SetLabelWidthAsync(int width)
        {
            if (!_isInitialized)
                return false;

            try
            {
                _labelWidth = width;
                var command = $"^PW{width}";
                return await SendZplCommandAsync(command);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetPrintSpeedAsync(int speed)
        {
            if (!_isInitialized)
                return false;

            try
            {
                _printSpeed = speed;
                var command = $"^PR{speed}";
                return await SendZplCommandAsync(command);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetPrintDarknessAsync(int darkness)
        {
            if (!_isInitialized)
                return false;

            try
            {
                _printDarkness = darkness;
                var command = $"~SD{darkness:D2}";
                return await SendZplCommandAsync(command);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetMediaTypeAsync(string mediaType)
        {
            if (!_isInitialized)
                return false;

            try
            {
                _mediaType = mediaType;
                var command = $"^MT{mediaType}";
                return await SendZplCommandAsync(command);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetSensorTypeAsync(string sensorType)
        {
            if (!_isInitialized)
                return false;

            try
            {
                _sensorType = sensorType;
                var command = $"^MM{sensorType}";
                return await SendZplCommandAsync(command);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PrintZplTextAsync(string text, int x, int y, string font = "0", int fontSize = 10, int rotation = 0)
        {
            if (!_isInitialized)
                return false;

            try
            {
                var command = $"^FO{x},{y}^A{font}N,{fontSize},{fontSize}^FD{text}^FS";
                return await SendZplCommandAsync(command);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PrintZplBarcodeAsync(string data, int x, int y, string barcodeType = "1", int height = 50, int width = 2, int rotation = 0)
        {
            if (!_isInitialized)
                return false;

            try
            {
                var command = $"^FO{x},{y}^BY{width}^BCN,{height},Y,N,N^FD{data}^FS";
                return await SendZplCommandAsync(command);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PrintZplQrCodeAsync(string data, int x, int y, int size = 3, string errorLevel = "L", int rotation = 0)
        {
            if (!_isInitialized)
                return false;

            try
            {
                var command = $"^FO{x},{y}^BQN,2,{size}^FD{errorLevel}A,{data}^FS";
                return await SendZplCommandAsync(command);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PrintZplImageAsync(byte[] imageData, int x, int y, int width, int height)
        {
            if (!_isInitialized || imageData == null)
                return false;

            try
            {
                // Convert image to ZPL format (simplified)
                var command = $"^FO{x},{y}^GFA,{imageData.Length},{imageData.Length},{width},{height},";
                await SendZplCommandAsync(command);
                
                // Send image data
                return await _bluetoothService.SendDataAsync(imageData);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PrintZplLineAsync(int x1, int y1, int x2, int y2, int thickness = 1)
        {
            if (!_isInitialized)
                return false;

            try
            {
                var command = $"^FO{x1},{y1}^GB{x2 - x1},{y2 - y1},{thickness}^FS";
                return await SendZplCommandAsync(command);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PrintZplRectangleAsync(int x, int y, int width, int height, int thickness = 1)
        {
            if (!_isInitialized)
                return false;

            try
            {
                var command = $"^FO{x},{y}^GB{width},{height},{thickness}^FS";
                return await SendZplCommandAsync(command);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PrintZplCircleAsync(int x, int y, int radius, int thickness = 1)
        {
            if (!_isInitialized)
                return false;

            try
            {
                var command = $"^FO{x},{y}^GC{radius},{thickness}^FS";
                return await SendZplCommandAsync(command);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetFieldOriginAsync(int x, int y)
        {
            if (!_isInitialized)
                return false;

            try
            {
                var command = $"^FO{x},{y}";
                return await SendZplCommandAsync(command);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetFieldSeparatorAsync(char separator)
        {
            if (!_isInitialized)
                return false;

            try
            {
                var command = $"^FS{separator}";
                return await SendZplCommandAsync(command);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetLabelHomeAsync(int x, int y)
        {
            if (!_isInitialized)
                return false;

            try
            {
                var command = $"^LH{x},{y}";
                return await SendZplCommandAsync(command);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PrintLabelAsync(int copies = 1)
        {
            if (!_isInitialized)
                return false;

            try
            {
                var command = $"^PQ{copies}";
                return await SendZplCommandAsync(command);
            }
            catch
            {
                return false;
            }
        }

        public async Task<ZebraPrinterStatus> GetZebraPrinterStatusAsync()
        {
            if (!_isInitialized)
                return new ZebraPrinterStatus();

            try
            {
                // ~HS - Host status command
                var command = "~HS";
                await SendZplCommandAsync(command);

                // In a real implementation, you would parse the response
                return new ZebraPrinterStatus
                {
                    IsOnline = true,
                    HasPaper = true,
                    IsCoverOpen = false,
                    HasError = false,
                    PaperRemaining = 100,
                    Temperature = 25,
                    IsReady = true,
                    IsPaused = false,
                    IsPrinting = false,
                    IsProcessing = false,
                    IsWaitingForLabel = false,
                    IsWaitingForRibbon = false,
                    IsWaitingForMedia = false,
                    IsWaitingForHeadUp = false,
                    IsWaitingForRibbonOut = false,
                    IsWaitingForRibbonIn = false,
                    IsWaitingForMediaOut = false,
                    IsWaitingForMediaIn = false,
                    IsWaitingForHeadDown = false,
                    IsWaitingForRibbonOut2 = false,
                    IsWaitingForRibbonIn2 = false,
                    IsWaitingForMediaOut2 = false,
                    IsWaitingForMediaIn2 = false,
                    IsWaitingForHeadDown2 = false,
                    IsWaitingForRibbonOut3 = false,
                    IsWaitingForRibbonIn3 = false,
                    IsWaitingForMediaOut3 = false,
                    IsWaitingForMediaIn3 = false,
                    IsWaitingForHeadDown3 = false
                };
            }
            catch
            {
                return new ZebraPrinterStatus();
            }
        }

        // IPrinterService implementation
        public async Task<bool> PrintTextAsync(string text, TextAlignment alignment = TextAlignment.Left, bool isBold = false, bool isUnderlined = false)
        {
            if (!_isInitialized)
                return false;

            try
            {
                var x = alignment switch
                {
                    TextAlignment.Center => _labelWidth / 2,
                    TextAlignment.Right => _labelWidth - 10,
                    _ => 10
                };

                var font = isBold ? "1" : "0";
                var fontSize = isBold ? 12 : 10;

                return await PrintZplTextAsync(text, x, 10, font, fontSize);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PrintBarcodeAsync(string data, BarcodeType barcodeType = BarcodeType.Code128, int height = 100, int width = 2)
        {
            if (!_isInitialized)
                return false;

            try
            {
                var zplBarcodeType = barcodeType switch
                {
                    BarcodeType.Code128 => "1",
                    BarcodeType.Code39 => "2",
                    BarcodeType.Ean13 => "3",
                    BarcodeType.Ean8 => "4",
                    BarcodeType.UpcA => "5",
                    BarcodeType.UpcE => "6",
                    BarcodeType.Itf => "7",
                    BarcodeType.Codabar => "8",
                    _ => "1"
                };

                return await PrintZplBarcodeAsync(data, 10, 50, zplBarcodeType, height, width);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PrintQrCodeAsync(string data, int size = 3, QrCodeErrorLevel errorLevel = QrCodeErrorLevel.L)
        {
            if (!_isInitialized)
                return false;

            try
            {
                var zplErrorLevel = errorLevel switch
                {
                    QrCodeErrorLevel.L => "L",
                    QrCodeErrorLevel.M => "M",
                    QrCodeErrorLevel.Q => "Q",
                    QrCodeErrorLevel.H => "H",
                    _ => "L"
                };

                return await PrintZplQrCodeAsync(data, 10, 150, size, zplErrorLevel);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PrintImageAsync(byte[] imageData, int width, int height)
        {
            if (!_isInitialized || imageData == null)
                return false;

            try
            {
                return await PrintZplImageAsync(imageData, 10, 200, width, height);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PrintLineBreakAsync(int lines = 1)
        {
            if (!_isInitialized)
                return false;

            try
            {
                // In ZPL, we typically use field separators for line breaks
                for (int i = 0; i < lines; i++)
                {
                    await SendZplCommandAsync("^FS");
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CutPaperAsync()
        {
            if (!_isInitialized)
                return false;

            try
            {
                // In ZPL, we use print commands to advance the label
                var command = "^XZ";
                return await SendZplCommandAsync(command);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PrintDataAsync(List<PrintDataModel> printData)
        {
            if (!_isInitialized || printData == null)
                return false;

            try
            {
                // Start label
                await SendZplCommandAsync("^XA");

                foreach (var data in printData)
                {
                    switch (data.Type)
                    {
                        case PrintDataType.Text:
                            await PrintTextAsync(data.Content, data.Alignment, data.IsBold, data.IsUnderlined);
                            break;
                        case PrintDataType.Barcode:
                            await PrintBarcodeAsync(data.Content, data.BarcodeType);
                            break;
                        case PrintDataType.QrCode:
                            await PrintQrCodeAsync(data.Content, 3, data.QrErrorLevel);
                            break;
                        case PrintDataType.Image:
                            if (data.ImageData != null)
                                await PrintImageAsync(data.ImageData, data.ImageWidth, data.ImageHeight);
                            break;
                        case PrintDataType.LineBreak:
                            await PrintLineBreakAsync(1);
                            break;
                        case PrintDataType.Cut:
                            await CutPaperAsync();
                            break;
                    }
                }

                // End label and print
                await SendZplCommandAsync("^XZ");

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<PrinterStatus> GetPrinterStatusAsync()
        {
            var zebraStatus = await GetZebraPrinterStatusAsync();
            return zebraStatus;
        }

        public async Task<bool> SetPrinterDarknessAsync(int value)
        {
            return await SetPrintDarknessAsync(value);
        }

        private async Task<bool> SendZplCommandAsync(string command)
        {
            if (!_isInitialized || string.IsNullOrEmpty(command))
                return false;

            try
            {
                var data = Encoding.ASCII.GetBytes(command);
                return await _bluetoothService.SendDataAsync(data);
            }
            catch
            {
                return false;
            }
        }
    }
} 