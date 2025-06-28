using System.Text;
using Maui.Bluetooth.Utils.Shared.Interfaces;
using Maui.Bluetooth.Utils.Shared.Models;
using TextAlignment = Maui.Bluetooth.Utils.Shared.Models.TextAlignment;

namespace Maui.Bluetooth.Utils.Shared.Services
{
    /// <summary>
    /// ESC/POS printer service implementation
    /// </summary>
    public class EscPosPrinterService : IEscPosPrinterService
    {
        private readonly IBluetoothService _bluetoothService;
        private bool _isInitialized = false;

        public event EventHandler<PrintJobStatusChangedEventArgs>? PrintJobStatusChanged;

        public EscPosPrinterService(IBluetoothService bluetoothService)
        {
            _bluetoothService = bluetoothService ?? throw new ArgumentNullException(nameof(bluetoothService));
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                await InitializeEscPosAsync();
                _isInitialized = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> InitializeEscPosAsync()
        {
            // ESC @ - Initialize printer
            var initCommand = new byte[] { 0x1B, 0x40 };
            return await _bluetoothService.SendDataAsync(initCommand);
        }

        public async Task<bool> SetCodePageAsync(int codePage)
        {
            // ESC t n - Set code page
            var command = new byte[] { 0x1B, 0x74, (byte)codePage };
            return await _bluetoothService.SendDataAsync(command);
        }

        public async Task<bool> SetPrintDensityAsync(int density)
        {
            // GS ( N - Set print density
            var command = new byte[] { 0x1D, 0x28, 0x4E, 0x02, 0x00, (byte)density };
            return await _bluetoothService.SendDataAsync(command);
        }

        public async Task<bool> SetPrintSpeedAsync(int speed)
        {
            // GS ( N - Set print speed
            var command = new byte[] { 0x1D, 0x28, 0x4E, 0x02, 0x01, (byte)speed };
            return await _bluetoothService.SendDataAsync(command);
        }

        public async Task<bool> PrintTextAsync(string text, TextAlignment alignment = TextAlignment.Left, bool isBold = false, bool isUnderlined = false)
        {
            if (!_isInitialized)
                return false;

            try
            {
                var commands = new List<byte[]>();

                // Set alignment
                commands.Add(GetAlignmentCommand(alignment));

                // Set bold
                if (isBold)
                    commands.Add(new byte[] { 0x1B, 0x45, 0x01 });

                // Set underline
                if (isUnderlined)
                    commands.Add(new byte[] { 0x1B, 0x2D, 0x01 });

                // Add text
                commands.Add(Encoding.UTF8.GetBytes(text));

                // Reset formatting
                commands.Add(new byte[] { 0x1B, 0x45, 0x00, 0x1B, 0x2D, 0x00 });

                // Send all commands
                foreach (var command in commands)
                {
                    await _bluetoothService.SendDataAsync(command);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PrintDoubleHeightTextAsync(string text, TextAlignment alignment = TextAlignment.Left)
        {
            if (!_isInitialized)
                return false;

            try
            {
                var commands = new List<byte[]>();

                // Set alignment
                commands.Add(GetAlignmentCommand(alignment));

                // Set double height
                commands.Add(new byte[] { 0x1B, 0x21, 0x10 });

                // Add text
                commands.Add(Encoding.UTF8.GetBytes(text));

                // Reset formatting
                commands.Add(new byte[] { 0x1B, 0x21, 0x00 });

                foreach (var command in commands)
                {
                    await _bluetoothService.SendDataAsync(command);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PrintDoubleWidthTextAsync(string text, TextAlignment alignment = TextAlignment.Left)
        {
            if (!_isInitialized)
                return false;

            try
            {
                var commands = new List<byte[]>();

                // Set alignment
                commands.Add(GetAlignmentCommand(alignment));

                // Set double width
                commands.Add(new byte[] { 0x1B, 0x21, 0x20 });

                // Add text
                commands.Add(Encoding.UTF8.GetBytes(text));

                // Reset formatting
                commands.Add(new byte[] { 0x1B, 0x21, 0x00 });

                foreach (var command in commands)
                {
                    await _bluetoothService.SendDataAsync(command);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PrintDoubleSizeTextAsync(string text, TextAlignment alignment = TextAlignment.Left)
        {
            if (!_isInitialized)
                return false;

            try
            {
                var commands = new List<byte[]>();

                // Set alignment
                commands.Add(GetAlignmentCommand(alignment));

                // Set double height and width
                commands.Add(new byte[] { 0x1B, 0x21, 0x30 });

                // Add text
                commands.Add(Encoding.UTF8.GetBytes(text));

                // Reset formatting
                commands.Add(new byte[] { 0x1B, 0x21, 0x00 });

                foreach (var command in commands)
                {
                    await _bluetoothService.SendDataAsync(command);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PrintInvertedTextAsync(string text, TextAlignment alignment = TextAlignment.Left)
        {
            if (!_isInitialized)
                return false;

            try
            {
                var commands = new List<byte[]>();

                // Set alignment
                commands.Add(GetAlignmentCommand(alignment));

                // Set inverted text
                commands.Add(new byte[] { 0x1B, 0x7B, 0x01 });

                // Add text
                commands.Add(Encoding.UTF8.GetBytes(text));

                // Reset formatting
                commands.Add(new byte[] { 0x1B, 0x7B, 0x00 });

                foreach (var command in commands)
                {
                    await _bluetoothService.SendDataAsync(command);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PrintUpsideDownTextAsync(string text, TextAlignment alignment = TextAlignment.Left)
        {
            if (!_isInitialized)
                return false;

            try
            {
                var commands = new List<byte[]>();

                // Set alignment
                commands.Add(GetAlignmentCommand(alignment));

                // Set upside down text
                commands.Add(new byte[] { 0x1B, 0x7B, 0x02 });

                // Add text
                commands.Add(Encoding.UTF8.GetBytes(text));

                // Reset formatting
                commands.Add(new byte[] { 0x1B, 0x7B, 0x00 });

                foreach (var command in commands)
                {
                    await _bluetoothService.SendDataAsync(command);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PrintTextWithFontAsync(string text, int font, TextAlignment alignment = TextAlignment.Left)
        {
            if (!_isInitialized)
                return false;

            try
            {
                var commands = new List<byte[]>();

                // Set alignment
                commands.Add(GetAlignmentCommand(alignment));

                // Set font
                commands.Add(new byte[] { 0x1B, 0x4D, (byte)font });

                // Add text
                commands.Add(Encoding.UTF8.GetBytes(text));

                // Reset font
                commands.Add(new byte[] { 0x1B, 0x4D, 0x00 });

                foreach (var command in commands)
                {
                    await _bluetoothService.SendDataAsync(command);
                }

                return true;
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
                // Set barcode height
                var heightCommand = new byte[] { 0x1D, 0x68, (byte)height };
                await _bluetoothService.SendDataAsync(heightCommand);

                // Set barcode width
                var widthCommand = new byte[] { 0x1D, 0x77, (byte)width };
                await _bluetoothService.SendDataAsync(widthCommand);

                // Set barcode type
                var typeCommand = new byte[] { 0x1D, 0x6B, (byte)barcodeType };
                await _bluetoothService.SendDataAsync(typeCommand);

                // Send barcode data
                var dataBytes = Encoding.ASCII.GetBytes(data);
                await _bluetoothService.SendDataAsync(dataBytes);

                // Send null terminator
                await _bluetoothService.SendDataAsync(new byte[] { 0x00 });

                return true;
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
                // QR Code commands would be implemented here
                // This is a simplified implementation
                var qrCommand = new byte[] { 0x1D, 0x28, 0x6B, 0x04, 0x00, 0x31, 0x41, (byte)size, 0x00 };
                await _bluetoothService.SendDataAsync(qrCommand);

                var dataBytes = Encoding.UTF8.GetBytes(data);
                await _bluetoothService.SendDataAsync(dataBytes);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PrintImageAsync(byte[] imageData, int width, int height)
        {
            return await PrintBitmapAsync(imageData, width, height, 0);
        }

        public async Task<bool> PrintBitmapAsync(byte[] imageData, int width, int height, int mode = 0)
        {
            if (!_isInitialized)
                return false;

            try
            {
                // GS v 0 - Print bitmap
                var command = new byte[] { 0x1D, 0x76, 0x30, (byte)mode, (byte)(width & 0xFF), (byte)((width >> 8) & 0xFF), (byte)(height & 0xFF), (byte)((height >> 8) & 0xFF) };
                await _bluetoothService.SendDataAsync(command);
                await _bluetoothService.SendDataAsync(imageData);

                return true;
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
                for (int i = 0; i < lines; i++)
                {
                    await _bluetoothService.SendDataAsync(new byte[] { 0x0A });
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
            return await PartialCutAsync();
        }

        public async Task<bool> PartialCutAsync()
        {
            if (!_isInitialized)
                return false;

            try
            {
                // GS V 0 - Partial cut
                var command = new byte[] { 0x1D, 0x56, 0x00 };
                return await _bluetoothService.SendDataAsync(command);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> FullCutAsync()
        {
            if (!_isInitialized)
                return false;

            try
            {
                // GS V 1 - Full cut
                var command = new byte[] { 0x1D, 0x56, 0x01 };
                return await _bluetoothService.SendDataAsync(command);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> FeedPaperAsync(int lines = 1)
        {
            if (!_isInitialized)
                return false;

            try
            {
                // ESC J n - Feed paper
                var command = new byte[] { 0x1B, 0x4A, (byte)lines };
                return await _bluetoothService.SendDataAsync(command);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> FeedPaperBackwardAsync(int lines = 1)
        {
            if (!_isInitialized)
                return false;

            try
            {
                // ESC K n - Feed paper backward
                var command = new byte[] { 0x1B, 0x4B, (byte)lines };
                return await _bluetoothService.SendDataAsync(command);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PrintDataAsync(List<PrintDataModel> printData)
        {
            if (!_isInitialized)
                return false;

            try
            {
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
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<PrinterStatus> GetPrinterStatusAsync()
        {
            // Simplified status - in real implementation, you would query the printer
            return await Task.FromResult(new PrinterStatus
            {
                IsOnline = _isInitialized,
                HasPaper = true,
                IsCoverOpen = false,
                HasError = false,
                PaperRemaining = 100,
                Temperature = 25
            });
        }

        public async Task<bool> SetPrinterDarknessAsync(int value)
        {
            return await SetPrintDensityAsync(value);
        }

        private byte[] GetAlignmentCommand(TextAlignment alignment)
        {
            return alignment switch
            {
                TextAlignment.Left => new byte[] { 0x1B, 0x61, 0x00 },
                TextAlignment.Center => new byte[] { 0x1B, 0x61, 0x01 },
                TextAlignment.Right => new byte[] { 0x1B, 0x61, 0x02 },
                _ => new byte[] { 0x1B, 0x61, 0x00 }
            };
        }
    }
} 