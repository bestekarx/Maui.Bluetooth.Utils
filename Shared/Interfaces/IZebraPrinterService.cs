using Maui.Bluetooth.Utils.Shared.Models;

namespace Maui.Bluetooth.Utils.Shared.Interfaces
{
    /// <summary>
    /// Zebra printer service interface for ZPL commands
    /// </summary>
    public interface IZebraPrinterService : IPrinterService
    {
        /// <summary>
        /// Initialize Zebra printer with ZPL commands
        /// </summary>
        /// <returns>True if initialization successful</returns>
        Task<bool> InitializeZebraAsync();

        /// <summary>
        /// Set label width
        /// </summary>
        /// <param name="width">Label width in dots</param>
        /// <returns>True if setting applied successfully</returns>
        Task<bool> SetLabelWidthAsync(int width);

        /// <summary>
        /// Set print speed
        /// </summary>
        /// <param name="speed">Print speed (1-14)</param>
        /// <returns>True if setting applied successfully</returns>
        Task<bool> SetPrintSpeedAsync(int speed);

        /// <summary>
        /// Set print darkness
        /// </summary>
        /// <param name="darkness">Print darkness (0-30)</param>
        /// <returns>True if setting applied successfully</returns>
        Task<bool> SetPrintDarknessAsync(int darkness);

        /// <summary>
        /// Set media type
        /// </summary>
        /// <param name="mediaType">Media type (continuous, die-cut, etc.)</param>
        /// <returns>True if setting applied successfully</returns>
        Task<bool> SetMediaTypeAsync(string mediaType);

        /// <summary>
        /// Set sensor type
        /// </summary>
        /// <param name="sensorType">Sensor type (gap, black mark, etc.)</param>
        /// <returns>True if setting applied successfully</returns>
        Task<bool> SetSensorTypeAsync(string sensorType);

        /// <summary>
        /// Print text with ZPL formatting
        /// </summary>
        /// <param name="text">Text to print</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="font">Font name</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="rotation">Text rotation (0, 90, 180, 270)</param>
        /// <returns>True if print successful</returns>
        Task<bool> PrintZplTextAsync(string text, int x, int y, string font = "0", int fontSize = 10, int rotation = 0);

        /// <summary>
        /// Print barcode with ZPL
        /// </summary>
        /// <param name="data">Barcode data</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="barcodeType">Barcode type (Code128, Code39, etc.)</param>
        /// <param name="height">Barcode height</param>
        /// <param name="width">Barcode width</param>
        /// <param name="rotation">Barcode rotation</param>
        /// <returns>True if print successful</returns>
        Task<bool> PrintZplBarcodeAsync(string data, int x, int y, string barcodeType = "1", int height = 50, int width = 2, int rotation = 0);

        /// <summary>
        /// Print QR code with ZPL
        /// </summary>
        /// <param name="data">QR code data</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="size">QR code size</param>
        /// <param name="errorLevel">Error correction level (L, M, Q, H)</param>
        /// <param name="rotation">QR code rotation</param>
        /// <returns>True if print successful</returns>
        Task<bool> PrintZplQrCodeAsync(string data, int x, int y, int size = 3, string errorLevel = "L", int rotation = 0);

        /// <summary>
        /// Print image with ZPL
        /// </summary>
        /// <param name="imageData">Image data</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <returns>True if print successful</returns>
        Task<bool> PrintZplImageAsync(byte[] imageData, int x, int y, int width, int height);

        /// <summary>
        /// Print line with ZPL
        /// </summary>
        /// <param name="x1">Start X position</param>
        /// <param name="y1">Start Y position</param>
        /// <param name="x2">End X position</param>
        /// <param name="y2">End Y position</param>
        /// <param name="thickness">Line thickness</param>
        /// <returns>True if print successful</returns>
        Task<bool> PrintZplLineAsync(int x1, int y1, int x2, int y2, int thickness = 1);

        /// <summary>
        /// Print rectangle with ZPL
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="width">Rectangle width</param>
        /// <param name="height">Rectangle height</param>
        /// <param name="thickness">Border thickness</param>
        /// <returns>True if print successful</returns>
        Task<bool> PrintZplRectangleAsync(int x, int y, int width, int height, int thickness = 1);

        /// <summary>
        /// Print circle with ZPL
        /// </summary>
        /// <param name="x">Center X position</param>
        /// <param name="y">Center Y position</param>
        /// <param name="radius">Circle radius</param>
        /// <param name="thickness">Border thickness</param>
        /// <returns>True if print successful</returns>
        Task<bool> PrintZplCircleAsync(int x, int y, int radius, int thickness = 1);

        /// <summary>
        /// Set field origin
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <returns>True if setting applied successfully</returns>
        Task<bool> SetFieldOriginAsync(int x, int y);

        /// <summary>
        /// Set field separator
        /// </summary>
        /// <param name="separator">Field separator character</param>
        /// <returns>True if setting applied successfully</returns>
        Task<bool> SetFieldSeparatorAsync(char separator);

        /// <summary>
        /// Set label home position
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <returns>True if setting applied successfully</returns>
        Task<bool> SetLabelHomeAsync(int x, int y);

        /// <summary>
        /// Print label
        /// </summary>
        /// <param name="copies">Number of copies</param>
        /// <returns>True if print successful</returns>
        Task<bool> PrintLabelAsync(int copies = 1);

        /// <summary>
        /// Get printer status
        /// </summary>
        /// <returns>Zebra printer status</returns>
        Task<ZebraPrinterStatus> GetZebraPrinterStatusAsync();
    }

    /// <summary>
    /// Zebra printer specific status information
    /// </summary>
    public class ZebraPrinterStatus : PrinterStatus
    {
        /// <summary>
        /// Whether printer is ready
        /// </summary>
        public bool IsReady { get; set; }

        /// <summary>
        /// Whether printer is paused
        /// </summary>
        public bool IsPaused { get; set; }

        /// <summary>
        /// Whether printer is printing
        /// </summary>
        public bool IsPrinting { get; set; }

        /// <summary>
        /// Whether printer is processing
        /// </summary>
        public bool IsProcessing { get; set; }

        /// <summary>
        /// Whether printer is waiting for label
        /// </summary>
        public bool IsWaitingForLabel { get; set; }

        /// <summary>
        /// Whether printer is waiting for ribbon
        /// </summary>
        public bool IsWaitingForRibbon { get; set; }

        /// <summary>
        /// Whether printer is waiting for media
        /// </summary>
        public bool IsWaitingForMedia { get; set; }

        /// <summary>
        /// Whether printer is waiting for head up
        /// </summary>
        public bool IsWaitingForHeadUp { get; set; }

        /// <summary>
        /// Whether printer is waiting for ribbon out
        /// </summary>
        public bool IsWaitingForRibbonOut { get; set; }

        /// <summary>
        /// Whether printer is waiting for ribbon in
        /// </summary>
        public bool IsWaitingForRibbonIn { get; set; }

        /// <summary>
        /// Whether printer is waiting for media out
        /// </summary>
        public bool IsWaitingForMediaOut { get; set; }

        /// <summary>
        /// Whether printer is waiting for media in
        /// </summary>
        public bool IsWaitingForMediaIn { get; set; }

        /// <summary>
        /// Whether printer is waiting for head down
        /// </summary>
        public bool IsWaitingForHeadDown { get; set; }

        /// <summary>
        /// Whether printer is waiting for ribbon out
        /// </summary>
        public bool IsWaitingForRibbonOut2 { get; set; }

        /// <summary>
        /// Whether printer is waiting for ribbon in
        /// </summary>
        public bool IsWaitingForRibbonIn2 { get; set; }

        /// <summary>
        /// Whether printer is waiting for media out
        /// </summary>
        public bool IsWaitingForMediaOut2 { get; set; }

        /// <summary>
        /// Whether printer is waiting for media in
        /// </summary>
        public bool IsWaitingForMediaIn2 { get; set; }

        /// <summary>
        /// Whether printer is waiting for head down
        /// </summary>
        public bool IsWaitingForHeadDown2 { get; set; }

        /// <summary>
        /// Whether printer is waiting for ribbon out
        /// </summary>
        public bool IsWaitingForRibbonOut3 { get; set; }

        /// <summary>
        /// Whether printer is waiting for ribbon in
        /// </summary>
        public bool IsWaitingForRibbonIn3 { get; set; }

        /// <summary>
        /// Whether printer is waiting for media out
        /// </summary>
        public bool IsWaitingForMediaOut3 { get; set; }

        /// <summary>
        /// Whether printer is waiting for media in
        /// </summary>
        public bool IsWaitingForMediaIn3 { get; set; }

        /// <summary>
        /// Whether printer is waiting for head down
        /// </summary>
        public bool IsWaitingForHeadDown3 { get; set; }
    }
} 