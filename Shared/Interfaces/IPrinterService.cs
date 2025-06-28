using Maui.Bluetooth.Utils.Shared.Models;
using TextAlignment = Maui.Bluetooth.Utils.Shared.Models.TextAlignment;

namespace Maui.Bluetooth.Utils.Shared.Interfaces
{
    /// <summary>
    /// Printer service interface for different printer types
    /// </summary>
    public interface IPrinterService
    {
        /// <summary>
        /// Event fired when print job status changes
        /// </summary>
        event EventHandler<PrintJobStatusChangedEventArgs>? PrintJobStatusChanged;

        /// <summary>
        /// Initialize printer
        /// </summary>
        /// <returns>True if initialization successful</returns>
        Task<bool> InitializeAsync();

        /// <summary>
        /// Print text
        /// </summary>
        /// <param name="text">Text to print</param>
        /// <param name="alignment">Text alignment</param>
        /// <param name="isBold">Whether text is bold</param>
        /// <param name="isUnderlined">Whether text is underlined</param>
        /// <returns>True if print successful</returns>
        Task<bool> PrintTextAsync(string text, TextAlignment alignment = TextAlignment.Left, bool isBold = false, bool isUnderlined = false);

        /// <summary>
        /// Print barcode
        /// </summary>
        /// <param name="data">Barcode data</param>
        /// <param name="barcodeType">Barcode type</param>
        /// <param name="height">Barcode height</param>
        /// <param name="width">Barcode width</param>
        /// <returns>True if print successful</returns>
        Task<bool> PrintBarcodeAsync(string data, BarcodeType barcodeType = BarcodeType.Code128, int height = 100, int width = 2);

        /// <summary>
        /// Print QR code
        /// </summary>
        /// <param name="data">QR code data</param>
        /// <param name="size">QR code size</param>
        /// <param name="errorLevel">Error correction level</param>
        /// <returns>True if print successful</returns>
        Task<bool> PrintQrCodeAsync(string data, int size = 3, QrCodeErrorLevel errorLevel = QrCodeErrorLevel.L);

        /// <summary>
        /// Print image
        /// </summary>
        /// <param name="imageData">Image data</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <returns>True if print successful</returns>
        Task<bool> PrintImageAsync(byte[] imageData, int width, int height);

        /// <summary>
        /// Print line break
        /// </summary>
        /// <param name="lines">Number of lines</param>
        /// <returns>True if print successful</returns>
        Task<bool> PrintLineBreakAsync(int lines = 1);

        /// <summary>
        /// Cut paper
        /// </summary>
        /// <returns>True if cut successful</returns>
        Task<bool> CutPaperAsync();

        /// <summary>
        /// Print multiple data items
        /// </summary>
        /// <param name="printData">List of print data</param>
        /// <returns>True if all items printed successfully</returns>
        Task<bool> PrintDataAsync(List<PrintDataModel> printData);

        /// <summary>
        /// Get printer status
        /// </summary>
        /// <returns>Printer status information</returns>
        Task<PrinterStatus> GetPrinterStatusAsync();

        /// <summary>
        /// Set printer darkness/contrast
        /// </summary>
        /// <param name="value">Darkness value (0-255)</param>
        /// <returns>True if setting applied successfully</returns>
        Task<bool> SetPrinterDarknessAsync(int value);
    }

    /// <summary>
    /// Event arguments for print job status changes
    /// </summary>
    public class PrintJobStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Print job ID
        /// </summary>
        public string JobId { get; }

        /// <summary>
        /// Current status
        /// </summary>
        public PrintJobStatus Status { get; }

        /// <summary>
        /// Progress percentage (0-100)
        /// </summary>
        public int Progress { get; }

        /// <summary>
        /// Error message if failed
        /// </summary>
        public string? ErrorMessage { get; }

        public PrintJobStatusChangedEventArgs(string jobId, PrintJobStatus status, int progress = 0, string? errorMessage = null)
        {
            JobId = jobId;
            Status = status;
            Progress = progress;
            ErrorMessage = errorMessage;
        }
    }

    /// <summary>
    /// Print job status
    /// </summary>
    public enum PrintJobStatus
    {
        /// <summary>
        /// Job is queued
        /// </summary>
        Queued = 0,

        /// <summary>
        /// Job is printing
        /// </summary>
        Printing = 1,

        /// <summary>
        /// Job completed successfully
        /// </summary>
        Completed = 2,

        /// <summary>
        /// Job failed
        /// </summary>
        Failed = 3,

        /// <summary>
        /// Job cancelled
        /// </summary>
        Cancelled = 4
    }

    /// <summary>
    /// Printer status information
    /// </summary>
    public class PrinterStatus
    {
        /// <summary>
        /// Whether printer is online
        /// </summary>
        public bool IsOnline { get; set; }

        /// <summary>
        /// Whether printer has paper
        /// </summary>
        public bool HasPaper { get; set; }

        /// <summary>
        /// Whether printer cover is open
        /// </summary>
        public bool IsCoverOpen { get; set; }

        /// <summary>
        /// Whether printer has error
        /// </summary>
        public bool HasError { get; set; }

        /// <summary>
        /// Error message if any
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Paper remaining percentage
        /// </summary>
        public int PaperRemaining { get; set; }

        /// <summary>
        /// Print head temperature
        /// </summary>
        public int Temperature { get; set; }
    }
} 