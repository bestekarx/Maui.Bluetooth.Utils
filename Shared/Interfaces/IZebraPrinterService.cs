using Maui.Bluetooth.Utils.Shared.Models;

namespace Maui.Bluetooth.Utils.Shared.Interfaces
{
    /// <summary>
    /// Zebra printer service interface for ZPL commands
    /// </summary>
    public interface IZebraPrinterService
    {
        /// <summary>
        /// Event fired when printer status changes
        /// </summary>
        event EventHandler<PrinterStatusChangedEventArgs>? PrinterStatusChanged;

        /// <summary>
        /// Print ZPL label
        /// </summary>
        /// <param name="zplCommands">ZPL commands to print</param>
        /// <returns>True if print successful</returns>
        Task<bool> PrintLabelAsync(string zplCommands);

        /// <summary>
        /// Print ZPL label with data
        /// </summary>
        /// <param name="template">ZPL template</param>
        /// <param name="data">Data to replace in template</param>
        /// <returns>True if print successful</returns>
        Task<bool> PrintLabelWithDataAsync(string template, Dictionary<string, string> data);

        /// <summary>
        /// Get printer status
        /// </summary>
        /// <returns>Printer status information</returns>
        Task<PrinterStatusModel> GetPrinterStatusAsync();

        /// <summary>
        /// Check if printer is ready
        /// </summary>
        /// <returns>True if printer is ready</returns>
        Task<bool> IsPrinterReadyAsync();

        /// <summary>
        /// Calibrate printer
        /// </summary>
        /// <returns>True if calibration successful</returns>
        Task<bool> CalibratePrinterAsync();

        /// <summary>
        /// Print test label
        /// </summary>
        /// <returns>True if print successful</returns>
        Task<bool> PrintTestLabelAsync();

        /// <summary>
        /// Set print darkness
        /// </summary>
        /// <param name="darkness">Darkness level (0-30)</param>
        /// <returns>True if setting successful</returns>
        Task<bool> SetPrintDarknessAsync(int darkness);

        /// <summary>
        /// Set print speed
        /// </summary>
        /// <param name="speed">Print speed (1-14)</param>
        /// <returns>True if setting successful</returns>
        Task<bool> SetPrintSpeedAsync(int speed);

        /// <summary>
        /// Set label dimensions
        /// </summary>
        /// <param name="width">Label width in dots</param>
        /// <param name="length">Label length in dots</param>
        /// <returns>True if setting successful</returns>
        Task<bool> SetLabelDimensionsAsync(int width, int length);

        /// <summary>
        /// Get printer settings
        /// </summary>
        /// <returns>Printer settings</returns>
        Task<PrinterSettingsModel> GetPrinterSettingsAsync();
    }

    /// <summary>
    /// Event arguments for printer status changes
    /// </summary>
    public class PrinterStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Previous status
        /// </summary>
        public PrinterStatusModel PreviousStatus { get; }

        /// <summary>
        /// Current status
        /// </summary>
        public PrinterStatusModel CurrentStatus { get; }

        /// <summary>
        /// Error message if status change failed
        /// </summary>
        public string? ErrorMessage { get; }

        public PrinterStatusChangedEventArgs(PrinterStatusModel previousStatus, PrinterStatusModel currentStatus, string? errorMessage = null)
        {
            PreviousStatus = previousStatus;
            CurrentStatus = currentStatus;
            ErrorMessage = errorMessage;
        }
    }
} 