namespace Maui.Bluetooth.Utils.Shared.Models
{
    /// <summary>
    /// Represents a print data item
    /// </summary>
    public class PrintDataModel
    {
        /// <summary>
        /// Data type (text, barcode, qrcode, image)
        /// </summary>
        public PrintDataType Type { get; set; }

        /// <summary>
        /// Content to print
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Font size (for text)
        /// </summary>
        public int FontSize { get; set; } = 12;

        /// <summary>
        /// Alignment (left, center, right)
        /// </summary>
        public TextAlignment Alignment { get; set; } = TextAlignment.Left;

        /// <summary>
        /// Whether text is bold
        /// </summary>
        public bool IsBold { get; set; }

        /// <summary>
        /// Whether text is underlined
        /// </summary>
        public bool IsUnderlined { get; set; }

        /// <summary>
        /// Barcode type (for barcode data)
        /// </summary>
        public BarcodeType BarcodeType { get; set; } = BarcodeType.Code128;

        /// <summary>
        /// QR code error correction level
        /// </summary>
        public QrCodeErrorLevel QrErrorLevel { get; set; } = QrCodeErrorLevel.L;

        /// <summary>
        /// Image data (for image type)
        /// </summary>
        public byte[]? ImageData { get; set; }

        /// <summary>
        /// Image width (for image type)
        /// </summary>
        public int ImageWidth { get; set; }

        /// <summary>
        /// Image height (for image type)
        /// </summary>
        public int ImageHeight { get; set; }
    }

    /// <summary>
    /// Print data types
    /// </summary>
    public enum PrintDataType
    {
        /// <summary>
        /// Plain text
        /// </summary>
        Text = 0,

        /// <summary>
        /// Barcode
        /// </summary>
        Barcode = 1,

        /// <summary>
        /// QR Code
        /// </summary>
        QrCode = 2,

        /// <summary>
        /// Image
        /// </summary>
        Image = 3,

        /// <summary>
        /// Line break
        /// </summary>
        LineBreak = 4,

        /// <summary>
        /// Cut paper
        /// </summary>
        Cut = 5
    }

    /// <summary>
    /// Text alignment options
    /// </summary>
    public enum TextAlignment
    {
        /// <summary>
        /// Left aligned
        /// </summary>
        Left = 0,

        /// <summary>
        /// Center aligned
        /// </summary>
        Center = 1,

        /// <summary>
        /// Right aligned
        /// </summary>
        Right = 2
    }

    /// <summary>
    /// Barcode types
    /// </summary>
    public enum BarcodeType
    {
        /// <summary>
        /// Code 128
        /// </summary>
        Code128 = 0,

        /// <summary>
        /// Code 39
        /// </summary>
        Code39 = 1,

        /// <summary>
        /// EAN-13
        /// </summary>
        Ean13 = 2,

        /// <summary>
        /// EAN-8
        /// </summary>
        Ean8 = 3,

        /// <summary>
        /// UPC-A
        /// </summary>
        UpcA = 4,

        /// <summary>
        /// UPC-E
        /// </summary>
        UpcE = 5,

        /// <summary>
        /// ITF (Interleaved 2 of 5)
        /// </summary>
        Itf = 6,

        /// <summary>
        /// Codabar
        /// </summary>
        Codabar = 7
    }

    /// <summary>
    /// QR Code error correction levels
    /// </summary>
    public enum QrCodeErrorLevel
    {
        /// <summary>
        /// Low error correction (7%)
        /// </summary>
        L = 0,

        /// <summary>
        /// Medium error correction (15%)
        /// </summary>
        M = 1,

        /// <summary>
        /// Quartile error correction (25%)
        /// </summary>
        Q = 2,

        /// <summary>
        /// High error correction (30%)
        /// </summary>
        H = 3
    }
} 