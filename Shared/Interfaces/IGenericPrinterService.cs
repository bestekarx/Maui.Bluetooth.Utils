using Maui.Bluetooth.Utils.Shared.Models;

namespace Maui.Bluetooth.Utils.Shared.Interfaces
{
    /// <summary>
    /// Generic printer service interface for raw data printing
    /// </summary>
    public interface IGenericPrinterService : IPrinterService
    {
        /// <summary>
        /// Send raw data to printer
        /// </summary>
        /// <param name="data">Raw data to send</param>
        /// <returns>True if data sent successfully</returns>
        Task<bool> SendRawDataAsync(byte[] data);

        /// <summary>
        /// Send raw text to printer
        /// </summary>
        /// <param name="text">Text to send</param>
        /// <param name="encoding">Text encoding</param>
        /// <returns>True if text sent successfully</returns>
        Task<bool> SendRawTextAsync(string text, string encoding = "UTF-8");

        /// <summary>
        /// Send command to printer
        /// </summary>
        /// <param name="command">Command to send</param>
        /// <returns>True if command sent successfully</returns>
        Task<bool> SendCommandAsync(string command);

        /// <summary>
        /// Send command with parameters to printer
        /// </summary>
        /// <param name="command">Command to send</param>
        /// <param name="parameters">Command parameters</param>
        /// <returns>True if command sent successfully</returns>
        Task<bool> SendCommandAsync(string command, params object[] parameters);

        /// <summary>
        /// Read data from printer
        /// </summary>
        /// <param name="timeout">Read timeout in milliseconds</param>
        /// <returns>Data read from printer</returns>
        Task<byte[]> ReadDataAsync(int timeout = 5000);

        /// <summary>
        /// Read text from printer
        /// </summary>
        /// <param name="timeout">Read timeout in milliseconds</param>
        /// <param name="encoding">Text encoding</param>
        /// <returns>Text read from printer</returns>
        Task<string> ReadTextAsync(int timeout = 5000, string encoding = "UTF-8");

        /// <summary>
        /// Flush output buffer
        /// </summary>
        /// <returns>True if flush successful</returns>
        Task<bool> FlushAsync();

        /// <summary>
        /// Clear input buffer
        /// </summary>
        /// <returns>True if clear successful</returns>
        Task<bool> ClearInputBufferAsync();

        /// <summary>
        /// Clear output buffer
        /// </summary>
        /// <returns>True if clear successful</returns>
        Task<bool> ClearOutputBufferAsync();

        /// <summary>
        /// Set read timeout
        /// </summary>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <returns>True if setting applied successfully</returns>
        Task<bool> SetReadTimeoutAsync(int timeout);

        /// <summary>
        /// Set write timeout
        /// </summary>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <returns>True if setting applied successfully</returns>
        Task<bool> SetWriteTimeoutAsync(int timeout);

        /// <summary>
        /// Get available bytes to read
        /// </summary>
        /// <returns>Number of available bytes</returns>
        Task<int> GetAvailableBytesAsync();

        /// <summary>
        /// Check if data is available to read
        /// </summary>
        /// <returns>True if data is available</returns>
        Task<bool> IsDataAvailableAsync();

        /// <summary>
        /// Send file to printer
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <returns>True if file sent successfully</returns>
        Task<bool> SendFileAsync(string filePath);

        /// <summary>
        /// Send stream to printer
        /// </summary>
        /// <param name="stream">Stream to send</param>
        /// <returns>True if stream sent successfully</returns>
        Task<bool> SendStreamAsync(Stream stream);

        /// <summary>
        /// Get printer capabilities
        /// </summary>
        /// <returns>Printer capabilities information</returns>
        Task<GenericPrinterCapabilities> GetCapabilitiesAsync();
    }

    /// <summary>
    /// Generic printer capabilities
    /// </summary>
    public class GenericPrinterCapabilities
    {
        /// <summary>
        /// Maximum data size that can be sent
        /// </summary>
        public int MaxDataSize { get; set; }

        /// <summary>
        /// Supported encodings
        /// </summary>
        public List<string> SupportedEncodings { get; set; } = new();

        /// <summary>
        /// Whether printer supports reading
        /// </summary>
        public bool SupportsReading { get; set; }

        /// <summary>
        /// Whether printer supports writing
        /// </summary>
        public bool SupportsWriting { get; set; }

        /// <summary>
        /// Whether printer supports duplex
        /// </summary>
        public bool SupportsDuplex { get; set; }

        /// <summary>
        /// Whether printer supports color
        /// </summary>
        public bool SupportsColor { get; set; }

        /// <summary>
        /// Whether printer supports stapling
        /// </summary>
        public bool SupportsStapling { get; set; }

        /// <summary>
        /// Whether printer supports punching
        /// </summary>
        public bool SupportsPunching { get; set; }

        /// <summary>
        /// Whether printer supports folding
        /// </summary>
        public bool SupportsFolding { get; set; }

        /// <summary>
        /// Whether printer supports binding
        /// </summary>
        public bool SupportsBinding { get; set; }

        /// <summary>
        /// Whether printer supports finishing
        /// </summary>
        public bool SupportsFinishing { get; set; }

        /// <summary>
        /// Whether printer supports variable data printing
        /// </summary>
        public bool SupportsVariableDataPrinting { get; set; }

        /// <summary>
        /// Whether printer supports page ranges
        /// </summary>
        public bool SupportsPageRanges { get; set; }

        /// <summary>
        /// Whether printer supports custom page sizes
        /// </summary>
        public bool SupportsCustomPageSizes { get; set; }

        /// <summary>
        /// Whether printer supports orientation
        /// </summary>
        public bool SupportsOrientation { get; set; }

        /// <summary>
        /// Whether printer supports resolution
        /// </summary>
        public bool SupportsResolution { get; set; }

        /// <summary>
        /// Whether printer supports quality
        /// </summary>
        public bool SupportsQuality { get; set; }

        /// <summary>
        /// Whether printer supports media type
        /// </summary>
        public bool SupportsMediaType { get; set; }

        /// <summary>
        /// Whether printer supports media source
        /// </summary>
        public bool SupportsMediaSource { get; set; }

        /// <summary>
        /// Whether printer supports media destination
        /// </summary>
        public bool SupportsMediaDestination { get; set; }

        /// <summary>
        /// Whether printer supports collation
        /// </summary>
        public bool SupportsCollation { get; set; }

        /// <summary>
        /// Whether printer supports copies
        /// </summary>
        public bool SupportsCopies { get; set; }

        /// <summary>
        /// Whether printer supports number up
        /// </summary>
        public bool SupportsNumberUp { get; set; }

        /// <summary>
        /// Whether printer supports sides
        /// </summary>
        public bool SupportsSides { get; set; }

        /// <summary>
        /// Whether printer supports job priority
        /// </summary>
        public bool SupportsJobPriority { get; set; }

        /// <summary>
        /// Whether printer supports job hold until
        /// </summary>
        public bool SupportsJobHoldUntil { get; set; }

        /// <summary>
        /// Whether printer supports job sheets
        /// </summary>
        public bool SupportsJobSheets { get; set; }

        /// <summary>
        /// Whether printer supports multiple document handling
        /// </summary>
        public bool SupportsMultipleDocumentHandling { get; set; }

        /// <summary>
        /// Whether printer supports print quality
        /// </summary>
        public bool SupportsPrintQuality { get; set; }

        /// <summary>
        /// Whether printer supports print color mode
        /// </summary>
        public bool SupportsPrintColorMode { get; set; }

        /// <summary>
        /// Whether printer supports print scaling
        /// </summary>
        public bool SupportsPrintScaling { get; set; }

        /// <summary>
        /// Whether printer supports presentation direction
        /// </summary>
        public bool SupportsPresentationDirection { get; set; }

        /// <summary>
        /// Whether printer supports media
        /// </summary>
        public bool SupportsMedia { get; set; }

        /// <summary>
        /// Whether printer supports media colored
        /// </summary>
        public bool SupportsMediaColored { get; set; }

        /// <summary>
        /// Whether printer supports media front coating
        /// </summary>
        public bool SupportsMediaFrontCoating { get; set; }

        /// <summary>
        /// Whether printer supports media back coating
        /// </summary>
        public bool SupportsMediaBackCoating { get; set; }

        /// <summary>
        /// Whether printer supports media size
        /// </summary>
        public bool SupportsMediaSize { get; set; }

        /// <summary>
        /// Whether printer supports media size name
        /// </summary>
        public bool SupportsMediaSizeName { get; set; }

        /// <summary>
        /// Whether printer supports media source
        /// </summary>
        public bool SupportsMediaSource2 { get; set; }

        /// <summary>
        /// Whether printer supports media type
        /// </summary>
        public bool SupportsMediaType2 { get; set; }

        /// <summary>
        /// Whether printer supports media weight metric
        /// </summary>
        public bool SupportsMediaWeightMetric { get; set; }

        /// <summary>
        /// Whether printer supports orientation requested
        /// </summary>
        public bool SupportsOrientationRequested { get; set; }

        /// <summary>
        /// Whether printer supports output bin
        /// </summary>
        public bool SupportsOutputBin { get; set; }

        /// <summary>
        /// Whether printer supports output device
        /// </summary>
        public bool SupportsOutputDevice { get; set; }

        /// <summary>
        /// Whether printer supports page delivery
        /// </summary>
        public bool SupportsPageDelivery { get; set; }

        /// <summary>
        /// Whether printer supports page order received
        /// </summary>
        public bool SupportsPageOrderReceived { get; set; }

        /// <summary>
        /// Whether printer supports page ranges
        /// </summary>
        public bool SupportsPageRanges2 { get; set; }

        /// <summary>
        /// Whether printer supports presentation direction
        /// </summary>
        public bool SupportsPresentationDirection2 { get; set; }

        /// <summary>
        /// Whether printer supports print accuracy
        /// </summary>
        public bool SupportsPrintAccuracy { get; set; }

        /// <summary>
        /// Whether printer supports print color mode
        /// </summary>
        public bool SupportsPrintColorMode2 { get; set; }

        /// <summary>
        /// Whether printer supports print quality
        /// </summary>
        public bool SupportsPrintQuality2 { get; set; }

        /// <summary>
        /// Whether printer supports print rendering intent
        /// </summary>
        public bool SupportsPrintRenderingIntent { get; set; }

        /// <summary>
        /// Whether printer supports print scaling
        /// </summary>
        public bool SupportsPrintScaling2 { get; set; }

        /// <summary>
        /// Whether printer supports printer resolution
        /// </summary>
        public bool SupportsPrinterResolution { get; set; }

        /// <summary>
        /// Whether printer supports sides
        /// </summary>
        public bool SupportsSides2 { get; set; }

        /// <summary>
        /// Whether printer supports x image position
        /// </summary>
        public bool SupportsXImagePosition { get; set; }

        /// <summary>
        /// Whether printer supports x image shift
        /// </summary>
        public bool SupportsXImageShift { get; set; }

        /// <summary>
        /// Whether printer supports x side1 image shift
        /// </summary>
        public bool SupportsXSide1ImageShift { get; set; }

        /// <summary>
        /// Whether printer supports x side2 image shift
        /// </summary>
        public bool SupportsXSide2ImageShift { get; set; }

        /// <summary>
        /// Whether printer supports y image position
        /// </summary>
        public bool SupportsYImagePosition { get; set; }

        /// <summary>
        /// Whether printer supports y image shift
        /// </summary>
        public bool SupportsYImageShift { get; set; }

        /// <summary>
        /// Whether printer supports y side1 image shift
        /// </summary>
        public bool SupportsYSide1ImageShift { get; set; }

        /// <summary>
        /// Whether printer supports y side2 image shift
        /// </summary>
        public bool SupportsYSide2ImageShift { get; set; }
    }
} 