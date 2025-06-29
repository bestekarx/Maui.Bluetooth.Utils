using System;
using System.Text;
using System.Collections.Generic;

namespace Maui.Bluetooth.Utils.Shared.Utils
{
    /// <summary>
    /// Zebra CPCL (Common Printer Command Language) utility class
    /// Provides easy-to-use methods for generating CPCL commands
    /// </summary>
    public static class ZebraCpclUtils
    {
        private static string GenerateFeedLines(int feedLines)
        {
            if (feedLines <= 0) return string.Empty;
            var sb = new StringBuilder();
            for (int i = 0; i < feedLines; i++)
                sb.AppendLine("! 0 200 24 1\nFORM\nPRINT"); // 24 dot = 1 satır
            return sb.ToString();
        }

        /// <summary>
        /// Generate simple text label with CPCL commands
        /// </summary>
        /// <param name="text">Text to print</param>
        /// <param name="x">X position (default: 30)</param>
        /// <param name="y">Y position (default: 40)</param>
        /// <param name="font">Font number (default: 4)</param>
        /// <param name="fontSize">Font size (default: 0)</param>
        /// <param name="feedLines">Number of feed lines (default: 5)</param>
        /// <returns>CPCL commands for text label</returns>
        public static string GenerateTextLabel(string text, int x = 30, int y = 40, int font = 4, int fontSize = 0, int feedLines = 5)
        {
            var sb = new StringBuilder();
            sb.AppendLine("! 0 200 200 400 1");
            sb.AppendLine($"TEXT {font} {fontSize} {x} {y} {text}");
            sb.AppendLine("FORM\nPRINT");
            sb.Append(GenerateFeedLines(feedLines));
            return sb.ToString();
        }

        /// <summary>
        /// Generate barcode label with CPCL commands
        /// </summary>
        /// <param name="barcode">Barcode data</param>
        /// <param name="label">Optional text label</param>
        /// <param name="x">X position (default: 30)</param>
        /// <param name="y">Y position (default: 40)</param>
        /// <param name="barcodeType">Barcode type (default: 128)</param>
        /// <param name="height">Barcode height (default: 50)</param>
        /// <param name="width">Barcode width (default: 1)</param>
        /// <param name="font">Font number (default: 4)</param>
        /// <param name="fontSize">Font size (default: 0)</param>
        /// <param name="labelAbove">Whether the label should be above the barcode</param>
        /// <param name="feedLines">Number of feed lines (default: 5)</param>
        /// <returns>CPCL commands for barcode label</returns>
        public static string GenerateBarcodeLabel(
            string barcode,
            string label = "",
            int x = 30,
            int y = 40,
            int barcodeType = 128,
            int height = 50,
            int width = 1,
            int font = 4,
            int fontSize = 0,
            bool labelAbove = false,
            int feedLines = 5)
        {
            var sb = new StringBuilder();
            sb.AppendLine("! 0 200 200 400 1");
            if (!string.IsNullOrEmpty(label) && labelAbove)
            {
                sb.AppendLine($"TEXT {font} {fontSize} {x} {y} {label}");
                sb.AppendLine($"BARCODE {barcodeType} {width} {width} {height} {x} {y + 30} {barcode}");
            }
            else if (!string.IsNullOrEmpty(label))
            {
                sb.AppendLine($"BARCODE {barcodeType} {width} {width} {height} {x} {y} {barcode}");
                sb.AppendLine($"TEXT {font} {fontSize} {x} {y + height + 20} {label}");
            }
            else
            {
                sb.AppendLine($"BARCODE {barcodeType} {width} {width} {height} {x} {y} {barcode}");
            }
            sb.AppendLine("FORM\nPRINT");
            sb.Append(GenerateFeedLines(feedLines));
            return sb.ToString();
        }

        /// <summary>
        /// Generate QR code label with CPCL commands
        /// </summary>
        /// <param name="qrData">QR code data</param>
        /// <param name="label">Optional text label</param>
        /// <param name="x">X position (default: 50)</param>
        /// <param name="y">Y position (default: 100)</param>
        /// <param name="size">QR code size (default: 2)</param>
        /// <param name="errorLevel">Error correction level (default: M)</param>
        /// <param name="font">Font number (default: 4)</param>
        /// <param name="fontSize">Font size (default: 0)</param>
        /// <param name="labelYOffset">Y offset for the label (default: -60)</param>
        /// <param name="feedLines">Number of feed lines (default: 5)</param>
        /// <returns>CPCL commands for QR code label</returns>
        public static string GenerateQrLabel(
            string qrData,
            string label = "",
            int x = 50,
            int y = 100,
            int size = 2,
            string errorLevel = "M",
            int font = 4,
            int fontSize = 0,
            int labelYOffset = -60,
            int feedLines = 5)
        {
            var sb = new StringBuilder();
            sb.AppendLine("! 0 200 200 400 1");
            if (!string.IsNullOrEmpty(label))
            {
                sb.AppendLine($"TEXT {font} {fontSize} {x} {y + labelYOffset} {label}");
            }
            sb.AppendLine($"B QR {x} {y} {errorLevel} {size} U 6");
            sb.AppendLine($"MA,{qrData}");
            sb.AppendLine("ENDQR");
            sb.AppendLine("FORM\nPRINT");
            sb.Append(GenerateFeedLines(feedLines));
            return sb.ToString();
        }

        /// <summary>
        /// Generate comprehensive test label with CPCL commands
        /// </summary>
        /// <param name="feedLines">Number of feed lines (default: 5)</param>
        /// <returns>CPCL commands for test label</returns>
        public static string GenerateTestLabel(int feedLines = 5)
        {
            var currentTime = DateTime.Now;
            var sb = new StringBuilder();
            sb.AppendLine("! 0 200 200 600 1");
            sb.AppendLine("TEXT 4 0 30 40 CPCL Test Label");
            sb.AppendLine($"TEXT 4 0 30 80 Tarih: {currentTime:yyyy-MM-dd}");
            sb.AppendLine($"TEXT 4 0 30 120 Saat: {currentTime:HH:mm:ss}");
            sb.AppendLine("BARCODE 128 1 1 50 50 160 123456789");
            sb.AppendLine("B QR 50 250 M 2 U 6");
            sb.AppendLine("MA,https://example.com");
            sb.AppendLine("ENDQR");
            sb.AppendLine("FORM\nPRINT");
            sb.Append(GenerateFeedLines(feedLines));
            return sb.ToString();
        }

        /// <summary>
        /// Generate receipt-style label with CPCL commands
        /// </summary>
        /// <param name="title">Receipt title</param>
        /// <param name="items">List of items to print</param>
        /// <param name="total">Total amount</param>
        /// <param name="x">X position (default: 30)</param>
        /// <param name="y">Y position (default: 40)</param>
        /// <param name="font">Font number (default: 4)</param>
        /// <param name="fontSize">Font size (default: 0)</param>
        /// <param name="lineHeight">Height of each line (default: 40)</param>
        /// <param name="feedLines">Number of feed lines (default: 5)</param>
        /// <returns>CPCL commands for receipt label</returns>
        public static string GenerateReceiptLabel(
            string title,
            List<(string Name, decimal Price)> items,
            decimal total,
            int x = 30,
            int y = 40,
            int font = 4,
            int fontSize = 0,
            int lineHeight = 40,
            int feedLines = 5)
        {
            var sb = new StringBuilder();
            sb.AppendLine("! 0 200 200 800 1");
            sb.AppendLine($"TEXT {font} {fontSize} {x} {y} {title}");
            sb.AppendLine($"TEXT {font} {fontSize} {x} {y + lineHeight} Tarih: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"TEXT {font} {fontSize} {x} {y + 2 * lineHeight} ------------------------------");
            var yPos = y + 3 * lineHeight;
            foreach (var item in items)
            {
                sb.AppendLine($"TEXT {font} {fontSize} {x} {yPos} {item.Name}: {item.Price:C}");
                yPos += lineHeight;
            }
            sb.AppendLine($"TEXT {font} {fontSize} {x} {yPos} ------------------------------");
            yPos += lineHeight;
            sb.AppendLine($"TEXT {font} {fontSize} {x} {yPos} Toplam: {total:C}");
            sb.AppendLine("FORM\nPRINT");
            sb.Append(GenerateFeedLines(feedLines));
            return sb.ToString();
        }

        /// <summary>
        /// Generate address label with CPCL commands
        /// </summary>
        /// <param name="name">Recipient name</param>
        /// <param name="address">Street address</param>
        /// <param name="city">City</param>
        /// <param name="postalCode">Postal code</param>
        /// <param name="country">Country</param>
        /// <returns>CPCL commands for address label</returns>
        public static string GenerateAddressLabel(string name, string address, string city, string postalCode, string country)
        {
            return $@"! 0 200 200 600 1
TEXT 4 0 30 40 {name}
TEXT 4 0 30 80 {address}
TEXT 4 0 30 120 {city}, {postalCode}
TEXT 4 0 30 160 {country}
PRINT
";
        }

        /// <summary>
        /// Generate product label with CPCL commands
        /// </summary>
        /// <param name="productName">Product name</param>
        /// <param name="productCode">Product code/barcode</param>
        /// <param name="price">Product price</param>
        /// <param name="description">Product description</param>
        /// <returns>CPCL commands for product label</returns>
        public static string GenerateProductLabel(string productName, string productCode, decimal price, string description = "")
        {
            var sb = new StringBuilder();
            sb.AppendLine("! 0 200 200 600 1");
            sb.AppendLine($"TEXT 4 0 30 40 {productName}");
            
            if (!string.IsNullOrEmpty(description))
            {
                sb.AppendLine($"TEXT 4 0 30 80 {description}");
            }
            
            sb.AppendLine($"BARCODE 128 1 1 50 50 120 {productCode}");
            sb.AppendLine($"TEXT 4 0 30 200 Fiyat: {price:C}");
            sb.AppendLine("PRINT");
            sb.Append(GenerateFeedLines(5));
            return sb.ToString();
        }

        /// <summary>
        /// Generate shipping label with CPCL commands
        /// </summary>
        /// <param name="trackingNumber">Tracking number</param>
        /// <param name="fromAddress">Sender address</param>
        /// <param name="toAddress">Recipient address</param>
        /// <param name="weight">Package weight</param>
        /// <returns>CPCL commands for shipping label</returns>
        public static string GenerateShippingLabel(string trackingNumber, string fromAddress, string toAddress, string weight)
        {
            return $@"! 0 200 200 800 1
TEXT 4 0 30 40 KARGO ETIKETI
TEXT 4 0 30 80 Takip No: {trackingNumber}
BARCODE 128 1 1 50 50 120 {trackingNumber}
TEXT 4 0 30 200 GONDEREN:
TEXT 4 0 30 240 {fromAddress}
TEXT 4 0 30 320 ALICI:
TEXT 4 0 30 360 {toAddress}
TEXT 4 0 30 440 Agirlik: {weight}
PRINT
";
        }

        /// <summary>
        /// Generate inventory label with CPCL commands
        /// </summary>
        /// <param name="itemCode">Item code</param>
        /// <param name="itemName">Item name</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="location">Storage location</param>
        /// <returns>CPCL commands for inventory label</returns>
        public static string GenerateInventoryLabel(string itemCode, string itemName, int quantity, string location)
        {
            return $@"! 0 200 200 600 1
TEXT 4 0 30 40 ENVANTER ETIKETI
BARCODE 128 1 1 50 50 100 {itemCode}
TEXT 4 0 30 160 {itemName}
TEXT 4 0 30 200 Miktar: {quantity}
TEXT 4 0 30 240 Konum: {location}
PRINT
";
        }

        /// <summary>
        /// Generate custom label with multiple elements
        /// </summary>
        /// <param name="elements">List of CPCL elements to include</param>
        /// <param name="labelWidth">Label width in dots (default: 400)</param>
        /// <param name="labelLength">Label length in dots (default: 600)</param>
        /// <param name="feedLines">Number of feed lines (default: 5)</param>
        /// <returns>CPCL commands for custom label</returns>
        public static string GenerateCustomLabel(List<string> elements, int labelWidth = 400, int labelLength = 600, int feedLines = 5)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"! 0 200 200 {labelWidth} 1");
            
            foreach (var element in elements)
            {
                sb.AppendLine(element);
            }
            
            sb.AppendLine("FORM\nPRINT");
            sb.Append(GenerateFeedLines(feedLines));
            return sb.ToString();
        }

        /// <summary>
        /// Create CPCL text element
        /// </summary>
        /// <param name="text">Text content</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="font">Font number (default: 4)</param>
        /// <param name="fontSize">Font size (default: 0)</param>
        /// <returns>CPCL text command</returns>
        public static string CreateTextElement(string text, int x, int y, int font = 4, int fontSize = 0)
        {
            return $"TEXT {font} {fontSize} {x} {y} {text}";
        }

        /// <summary>
        /// Create CPCL barcode element
        /// </summary>
        /// <param name="data">Barcode data</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="barcodeType">Barcode type (default: 128)</param>
        /// <param name="height">Barcode height (default: 50)</param>
        /// <param name="width">Barcode width (default: 1)</param>
        /// <returns>CPCL barcode command</returns>
        public static string CreateBarcodeElement(string data, int x, int y, int barcodeType = 128, int height = 50, int width = 1)
        {
            return $"BARCODE {barcodeType} {width} {width} {height} {x} {y} {data}";
        }

        /// <summary>
        /// Create CPCL QR code element
        /// </summary>
        /// <param name="data">QR code data</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="size">QR code size (default: 2)</param>
        /// <param name="errorLevel">Error correction level (default: M)</param>
        /// <returns>CPCL QR code commands</returns>
        public static string CreateQrCodeElement(string data, int x, int y, int size = 2, string errorLevel = "M")
        {
            return $"B QR {x} {y} {errorLevel} {size} U 6\nMA,{data}\nENDQR";
        }

        /// <summary>
        /// Create CPCL line element
        /// </summary>
        /// <param name="x1">Start X position</param>
        /// <param name="y1">Start Y position</param>
        /// <param name="x2">End X position</param>
        /// <param name="y2">End Y position</param>
        /// <param name="thickness">Line thickness (default: 1)</param>
        /// <returns>CPCL line command</returns>
        public static string CreateLineElement(int x1, int y1, int x2, int y2, int thickness = 1)
        {
            return $"L {x1} {y1} {x2} {y2} {thickness}";
        }

        /// <summary>
        /// Create CPCL rectangle element
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="width">Rectangle width</param>
        /// <param name="height">Rectangle height</param>
        /// <param name="thickness">Border thickness (default: 1)</param>
        /// <returns>CPCL rectangle command</returns>
        public static string CreateRectangleElement(int x, int y, int width, int height, int thickness = 1)
        {
            return $"R {x} {y} {width} {height} {thickness}";
        }
    }
} 