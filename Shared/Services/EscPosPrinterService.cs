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

                // Send all commands with small delays
                foreach (var command in commands)
                {
                    await _bluetoothService.SendDataAsync(command);
                    await Task.Delay(5); // Small delay between commands
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrintTextAsync error: {ex.Message}");
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

                // Send all commands with small delays
                foreach (var command in commands)
                {
                    await _bluetoothService.SendDataAsync(command);
                    await Task.Delay(5); // Small delay between commands
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrintDoubleHeightTextAsync error: {ex.Message}");
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

                // Send all commands with small delays
                foreach (var command in commands)
                {
                    await _bluetoothService.SendDataAsync(command);
                    await Task.Delay(5); // Small delay between commands
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrintDoubleWidthTextAsync error: {ex.Message}");
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

                // Send all commands with small delays
                foreach (var command in commands)
                {
                    await _bluetoothService.SendDataAsync(command);
                    await Task.Delay(5); // Small delay between commands
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrintDoubleSizeTextAsync error: {ex.Message}");
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

                // Send all commands with small delays
                foreach (var command in commands)
                {
                    await _bluetoothService.SendDataAsync(command);
                    await Task.Delay(5); // Small delay between commands
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrintInvertedTextAsync error: {ex.Message}");
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

                // Send all commands with small delays
                foreach (var command in commands)
                {
                    await _bluetoothService.SendDataAsync(command);
                    await Task.Delay(5); // Small delay between commands
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrintUpsideDownTextAsync error: {ex.Message}");
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

                // Send all commands with small delays
                foreach (var command in commands)
                {
                    await _bluetoothService.SendDataAsync(command);
                    await Task.Delay(5); // Small delay between commands
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrintTextWithFontAsync error: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> SendCommandWithRetryAsync(byte[] command, int maxRetries = 3, int delayMs = 10)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var result = await _bluetoothService.SendDataAsync(command);
                    if (result)
                    {
                        await Task.Delay(delayMs);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SendCommandWithRetryAsync attempt {i + 1} failed: {ex.Message}");
                }
                
                if (i < maxRetries - 1)
                {
                    await Task.Delay(50); // Wait before retry
                }
            }
            return false;
        }

        private async Task<bool> SendCommandsBatchAsync(List<byte[]> commands, int delayBetweenCommands = 5)
        {
            try
            {
                foreach (var command in commands)
                {
                    var success = await SendCommandWithRetryAsync(command, 2, delayBetweenCommands);
                    if (!success)
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to send command in batch");
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendCommandsBatchAsync error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ESC/POS cihazları için basit barcode yazdırma metodu
        /// </summary>
        public async Task<bool> PrintSimpleBarcodeAsync(string data, BarcodeType barcodeType = BarcodeType.Code128)
        {
            if (!_isInitialized)
                return false;

            try
            {
                // Set barcode height (GS h n)
                await _bluetoothService.SendDataAsync(new byte[] { 0x1D, 0x68, 0x64 }); // Height = 100
                await Task.Delay(20);

                // Set barcode width (GS w n)
                await _bluetoothService.SendDataAsync(new byte[] { 0x1D, 0x77, 0x02 }); // Width = 2
                await Task.Delay(20);

                // Set HRI position (GS H n) - Print HRI below barcode
                await _bluetoothService.SendDataAsync(new byte[] { 0x1D, 0x48, 0x02 });
                await Task.Delay(20);

                // Set HRI font (GS f n) - Font A
                await _bluetoothService.SendDataAsync(new byte[] { 0x1D, 0x66, 0x00 });
                await Task.Delay(20);

                // Print barcode based on type
                switch (barcodeType)
                {
                    case BarcodeType.Code128:
                        // GS k m d1...dk NUL
                        await _bluetoothService.SendDataAsync(new byte[] { 0x1D, 0x6B, 0x08 });
                        await Task.Delay(20);
                        await _bluetoothService.SendDataAsync(Encoding.ASCII.GetBytes(data));
                        await Task.Delay(20);
                        await _bluetoothService.SendDataAsync(new byte[] { 0x00 });
                        break;

                    case BarcodeType.Code39:
                        // GS k m d1...dk NUL
                        await _bluetoothService.SendDataAsync(new byte[] { 0x1D, 0x6B, 0x04 });
                        await Task.Delay(20);
                        await _bluetoothService.SendDataAsync(Encoding.ASCII.GetBytes(data));
                        await Task.Delay(20);
                        await _bluetoothService.SendDataAsync(new byte[] { 0x00 });
                        break;

                    case BarcodeType.Ean13:
                        // GS k m d1...d12
                        if (data.Length == 12)
                        {
                            await _bluetoothService.SendDataAsync(new byte[] { 0x1D, 0x6B, 0x02 });
                            await Task.Delay(20);
                            await _bluetoothService.SendDataAsync(Encoding.ASCII.GetBytes(data));
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"EAN-13 requires exactly 12 digits, got {data.Length}");
                            return false;
                        }
                        break;

                    case BarcodeType.Ean8:
                        // GS k m d1...d7
                        if (data.Length == 7)
                        {
                            await _bluetoothService.SendDataAsync(new byte[] { 0x1D, 0x6B, 0x03 });
                            await Task.Delay(20);
                            await _bluetoothService.SendDataAsync(Encoding.ASCII.GetBytes(data));
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"EAN-8 requires exactly 7 digits, got {data.Length}");
                            return false;
                        }
                        break;

                    default:
                        // Default to Code128
                        await _bluetoothService.SendDataAsync(new byte[] { 0x1D, 0x6B, 0x08 });
                        await Task.Delay(20);
                        await _bluetoothService.SendDataAsync(Encoding.ASCII.GetBytes(data));
                        await Task.Delay(20);
                        await _bluetoothService.SendDataAsync(new byte[] { 0x00 });
                        break;
                }

                await Task.Delay(100); // Wait for barcode to print

                // Feed paper after barcode
                await PrintLineBreakAsync(2);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrintSimpleBarcodeAsync error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ESC/POS cihazları için alternatif QR kod yazdırma metodu (daha basit)
        /// </summary>
        public async Task<bool> PrintQrCodeAlternativeAsync(string data, int size = 6)
        {
            if (!_isInitialized)
                return false;

            try
            {
                // QR Code: Select the QR code model
                await _bluetoothService.SendDataAsync(new byte[] { 0x1D, 0x28, 0x6B, 0x04, 0x00, 0x31, 0x41, 0x32, 0x00 });
                await Task.Delay(50);

                // QR Code: Set the size of module
                await _bluetoothService.SendDataAsync(new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x43, (byte)size });
                await Task.Delay(50);

                // QR Code: Set error correction level
                await _bluetoothService.SendDataAsync(new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x45, 0x30 });
                await Task.Delay(50);

                // QR Code: Store the data in the symbol storage area
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var dataLength = dataBytes.Length;
                var pL = (byte)(dataLength + 3);
                var pH = (byte)0x00;
                
                await _bluetoothService.SendDataAsync(new byte[] { 0x1D, 0x28, 0x6B, pL, pH, 0x31, 0x50, 0x30 });
                await Task.Delay(20);
                await _bluetoothService.SendDataAsync(dataBytes);
                await Task.Delay(50);

                // QR Code: Print the symbol data in the symbol storage area
                await _bluetoothService.SendDataAsync(new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x51, 0x30 });
                await Task.Delay(100);

                // Feed paper after QR code
                await PrintLineBreakAsync(2);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrintQrCodeAlternativeAsync error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ESC/POS cihazları için çok basit QR kod yazdırma metodu (kaynaklardaki örneklere göre)
        /// </summary>
        public async Task<bool> PrintQrCodeVerySimpleAsync(string data, int size = 4)
        {
            if (!_isInitialized)
                return false;

            try
            {
                // Initialize printer first
                await _bluetoothService.SendDataAsync(new byte[] { 0x1B, 0x40 });
                await Task.Delay(100);

                // QR Code: Select the QR code model (Function 165)
                await _bluetoothService.SendDataAsync(new byte[] { 0x1D, 0x28, 0x6B, 0x04, 0x00, 0x31, 0x41, 0x32, 0x00 });
                await Task.Delay(100);

                // QR Code: Set the size of module (Function 167)
                await _bluetoothService.SendDataAsync(new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x43, (byte)size });
                await Task.Delay(100);

                // QR Code: Set error correction level (Function 169)
                await _bluetoothService.SendDataAsync(new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x45, 0x30 });
                await Task.Delay(100);

                // QR Code: Store the data in the symbol storage area (Function 180)
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var dataLength = dataBytes.Length;
                var pL = (byte)(dataLength + 3);
                var pH = (byte)0x00;
                
                await _bluetoothService.SendDataAsync(new byte[] { 0x1D, 0x28, 0x6B, pL, pH, 0x31, 0x50, 0x30 });
                await Task.Delay(50);
                await _bluetoothService.SendDataAsync(dataBytes);
                await Task.Delay(100);

                // QR Code: Print the symbol data in the symbol storage area (Function 181)
                await _bluetoothService.SendDataAsync(new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x51, 0x30 });
                await Task.Delay(200);

                // Feed paper after QR code
                await PrintLineBreakAsync(3);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrintQrCodeVerySimpleAsync error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ESC/POS cihazları için basit QR kod yazdırma metodu (interface uyumluluğu için)
        /// </summary>
        public async Task<bool> PrintSimpleQrCodeAsync(string data, int size = 6)
        {
            // Basit QR kod metodu için PrintQrCodeVerySimpleAsync'ı kullan
            return await PrintQrCodeVerySimpleAsync(data, size);
        }

        /// <summary>
        /// ESC/POS cihazları için özel barcode yazdırma metodu
        /// </summary>
        public async Task<bool> PrintBarcodeEscPosAsync(string data, BarcodeType barcodeType = BarcodeType.Code128, int height = 100, int width = 2)
        {
            if (!_isInitialized)
                return false;

            try
            {
                var commands = new List<byte[]>();

                // Set barcode height (GS h n)
                commands.Add(new byte[] { 0x1D, 0x68, (byte)height });

                // Set barcode width (GS w n)
                commands.Add(new byte[] { 0x1D, 0x77, (byte)width });

                // Set barcode HRI position (GS H n) - Print HRI below barcode
                commands.Add(new byte[] { 0x1D, 0x48, 0x02 });

                // Set barcode HRI font (GS f n) - Font A
                commands.Add(new byte[] { 0x1D, 0x66, 0x00 });

                // Print barcode based on type
                switch (barcodeType)
                {
                    case BarcodeType.Code128:
                        // GS k m d1...dk NUL
                        commands.Add(new byte[] { 0x1D, 0x6B, 0x08 });
                        commands.Add(Encoding.ASCII.GetBytes(data));
                        commands.Add(new byte[] { 0x00 });
                        break;

                    case BarcodeType.Code39:
                        // GS k m d1...dk NUL
                        commands.Add(new byte[] { 0x1D, 0x6B, 0x04 });
                        commands.Add(Encoding.ASCII.GetBytes(data));
                        commands.Add(new byte[] { 0x00 });
                        break;

                    case BarcodeType.Ean13:
                        // GS k m d1...d12
                        if (data.Length == 12)
                        {
                            commands.Add(new byte[] { 0x1D, 0x6B, 0x02 });
                            commands.Add(Encoding.ASCII.GetBytes(data));
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"EAN-13 requires exactly 12 digits, got {data.Length}");
                            return false;
                        }
                        break;

                    case BarcodeType.Ean8:
                        // GS k m d1...d7
                        if (data.Length == 7)
                        {
                            commands.Add(new byte[] { 0x1D, 0x6B, 0x03 });
                            commands.Add(Encoding.ASCII.GetBytes(data));
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"EAN-8 requires exactly 7 digits, got {data.Length}");
                            return false;
                        }
                        break;

                    case BarcodeType.UpcA:
                        // GS k m d1...d11
                        if (data.Length == 11)
                        {
                            commands.Add(new byte[] { 0x1D, 0x6B, 0x00 });
                            commands.Add(Encoding.ASCII.GetBytes(data));
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"UPC-A requires exactly 11 digits, got {data.Length}");
                            return false;
                        }
                        break;

                    case BarcodeType.UpcE:
                        // GS k m d1...d11
                        if (data.Length == 11)
                        {
                            commands.Add(new byte[] { 0x1D, 0x6B, 0x01 });
                            commands.Add(Encoding.ASCII.GetBytes(data));
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"UPC-E requires exactly 11 digits, got {data.Length}");
                            return false;
                        }
                        break;

                    case BarcodeType.Itf:
                        // GS k m d1...dk NUL
                        commands.Add(new byte[] { 0x1D, 0x6B, 0x05 });
                        commands.Add(Encoding.ASCII.GetBytes(data));
                        commands.Add(new byte[] { 0x00 });
                        break;

                    case BarcodeType.Codabar:
                        // GS k m d1...dk NUL
                        commands.Add(new byte[] { 0x1D, 0x6B, 0x06 });
                        commands.Add(Encoding.ASCII.GetBytes(data));
                        commands.Add(new byte[] { 0x00 });
                        break;

                    default:
                        // Default to Code128
                        commands.Add(new byte[] { 0x1D, 0x6B, 0x08 });
                        commands.Add(Encoding.ASCII.GetBytes(data));
                        commands.Add(new byte[] { 0x00 });
                        break;
                }

                // Send all commands as batch
                var success = await SendCommandsBatchAsync(commands, 10);

                if (success)
                {
                    // Feed paper after barcode
                    await PrintLineBreakAsync(2);
                }

                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrintBarcodeEscPosAsync error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ESC/POS cihazları için özel QR kod yazdırma metodu
        /// </summary>
        public async Task<bool> PrintQrCodeEscPosAsync(string data, int size = 3, QrCodeErrorLevel errorLevel = QrCodeErrorLevel.L)
        {
            if (!_isInitialized)
                return false;

            try
            {
                var commands = new List<byte[]>();

                // QR Code: Select the QR code model
                // GS ( k pL pH cn fn n1 n2
                // pL pH: 0x00 0x00 (2 bytes)
                // cn: 0x31 (49 for QR Code)
                // fn: 0x01 (1 for model selection)
                // n1: 0x01 (1 for model 1)
                commands.Add(new byte[] { 0x1D, 0x28, 0x6B, 0x04, 0x00, 0x31, 0x01, 0x01 });

                // QR Code: Set the size of module
                // GS ( k pL pH cn fn n
                // pL pH: 0x00 0x00 (2 bytes)
                // cn: 0x31 (49 for QR Code)
                // fn: 0x03 (3 for module size)
                // n: size (1-8)
                commands.Add(new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x03, (byte)size });

                // QR Code: Set error correction level
                // GS ( k pL pH cn fn n
                // pL pH: 0x00 0x00 (2 bytes)
                // cn: 0x31 (49 for QR Code)
                // fn: 0x02 (2 for error correction level)
                // n: error level (48=L, 49=M, 50=Q, 51=H)
                byte errorLevelByte = (byte)(48 + (int)errorLevel);
                commands.Add(new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x02, errorLevelByte });

                // QR Code: Store the data in the symbol storage area
                // GS ( k pL pH cn fn m d1...dk
                // pL pH: 0x00 0x00 (2 bytes)
                // cn: 0x31 (49 for QR Code)
                // fn: 0x32 (50 for data storage)
                // m: 0x30 (48 for 8-bit byte mode)
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var dataLength = dataBytes.Length;
                var pL = (byte)(dataLength + 3);
                var pH = (byte)0x00;
                
                var qrDataCommand = new byte[] { 0x1D, 0x28, 0x6B, pL, pH, 0x31, 0x32, 0x30 };
                commands.Add(qrDataCommand);
                commands.Add(dataBytes);

                // QR Code: Print the symbol data in the symbol storage area
                // GS ( k pL pH cn fn m
                // pL pH: 0x00 0x00 (2 bytes)
                // cn: 0x31 (49 for QR Code)
                // fn: 0x33 (51 for print)
                // m: 0x30 (48 for normal)
                commands.Add(new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x33, 0x30 });

                // Send all commands as batch
                var success = await SendCommandsBatchAsync(commands, 10);

                if (success)
                {
                    // Feed paper after QR code
                    await PrintLineBreakAsync(2);
                }

                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrintQrCodeEscPosAsync error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Legacy barcode printing method (for backward compatibility)
        /// </summary>
        public async Task<bool> PrintBarcodeAsync(string data, BarcodeType barcodeType = BarcodeType.Code128, int height = 100, int width = 2)
        {
            return await PrintBarcodeEscPosAsync(data, barcodeType, height, width);
        }

        /// <summary>
        /// Legacy QR code printing method (for backward compatibility)
        /// </summary>
        public async Task<bool> PrintQrCodeAsync(string data, int size = 3, QrCodeErrorLevel errorLevel = QrCodeErrorLevel.L)
        {
            return await PrintQrCodeEscPosAsync(data, size, errorLevel);
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
                await Task.Delay(10); // Small delay between commands
                await _bluetoothService.SendDataAsync(imageData);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrintBitmapAsync error: {ex.Message}");
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
                    await Task.Delay(5); // Small delay between line breaks
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrintLineBreakAsync error: {ex.Message}");
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
                await _bluetoothService.SendDataAsync(command);
                await Task.Delay(50); // Longer delay for cut operation
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PartialCutAsync error: {ex.Message}");
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
                await _bluetoothService.SendDataAsync(command);
                await Task.Delay(50); // Longer delay for cut operation
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FullCutAsync error: {ex.Message}");
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
                await _bluetoothService.SendDataAsync(command);
                await Task.Delay(10); // Small delay for feed operation
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FeedPaperAsync error: {ex.Message}");
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
                await _bluetoothService.SendDataAsync(command);
                await Task.Delay(10); // Small delay for feed operation
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FeedPaperBackwardAsync error: {ex.Message}");
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
                    bool success = false;
                    
                    switch (data.Type)
                    {
                        case PrintDataType.Text:
                            success = await PrintTextAsync(data.Content, data.Alignment, data.IsBold, data.IsUnderlined);
                            break;
                        case PrintDataType.Barcode:
                            success = await PrintSimpleBarcodeAsync(data.Content, data.BarcodeType);
                            break;
                        case PrintDataType.QrCode:
                            success = await PrintSimpleQrCodeAsync(data.Content, 6);
                            break;
                        case PrintDataType.Image:
                            if (data.ImageData != null)
                                success = await PrintImageAsync(data.ImageData, data.ImageWidth, data.ImageHeight);
                            break;
                        case PrintDataType.LineBreak:
                            success = await PrintLineBreakAsync(1);
                            break;
                        case PrintDataType.Cut:
                            success = await CutPaperAsync();
                            break;
                    }

                    if (!success)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to print data type: {data.Type}");
                    }

                    // Small delay between different print operations
                    await Task.Delay(20);
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrintDataAsync error: {ex.Message}");
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