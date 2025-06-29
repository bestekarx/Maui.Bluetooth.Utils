using System.Text;

namespace Maui.Bluetooth.Utils.Shared.Utils
{
    /// <summary>
    /// Zebra printer utility class for common ZPL operations
    /// </summary>
    public static class ZebraUtils
    {
        /// <summary>
        /// Generate complete ZPL label with common elements
        /// </summary>
        /// <param name="elements">ZPL elements to include in label</param>
        /// <returns>Complete ZPL label</returns>
        public static string GenerateLabel(params string[] elements)
        {
            var sb = new StringBuilder();
            sb.AppendLine("^XA"); // Start of label
            
            foreach (var element in elements)
            {
                sb.AppendLine(element);
            }
            
            sb.AppendLine("^XZ"); // End of label
            return sb.ToString();
        }

        /// <summary>
        /// Generate text element
        /// </summary>
        /// <param name="text">Text content</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="font">Font (0-9, A-Z)</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="rotation">Rotation (0, 90, 180, 270)</param>
        /// <returns>ZPL text element</returns>
        public static string Text(string text, int x, int y, string font = "0", int fontSize = 10, int rotation = 0)
        {
            return $"^FO{x},{y}^A{font}N,{fontSize},{fontSize}^FD{text}^FS";
        }

        /// <summary>
        /// Generate barcode element
        /// </summary>
        /// <param name="data">Barcode data</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="barcodeType">Barcode type (1=Code128, 2=Code39, etc.)</param>
        /// <param name="height">Barcode height</param>
        /// <param name="width">Barcode width</param>
        /// <param name="rotation">Rotation</param>
        /// <returns>ZPL barcode element</returns>
        public static string Barcode(string data, int x, int y, string barcodeType = "1", int height = 50, int width = 2, int rotation = 0)
        {
            return $"^FO{x},{y}^BY{width}^BCN,{height},Y,N,N^FD{data}^FS";
        }

        /// <summary>
        /// Generate QR code element
        /// </summary>
        /// <param name="data">QR code data</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="size">QR code size (1-10)</param>
        /// <param name="errorLevel">Error correction level (L, M, Q, H)</param>
        /// <param name="rotation">Rotation</param>
        /// <returns>ZPL QR code element</returns>
        public static string QrCode(string data, int x, int y, int size = 3, string errorLevel = "L", int rotation = 0)
        {
            return $"^FO{x},{y}^BQN,2,{size},{errorLevel}^FD{data}^FS";
        }

        /// <summary>
        /// Generate line element
        /// </summary>
        /// <param name="x1">Start X position</param>
        /// <param name="y1">Start Y position</param>
        /// <param name="x2">End X position</param>
        /// <param name="y2">End Y position</param>
        /// <param name="thickness">Line thickness</param>
        /// <returns>ZPL line element</returns>
        public static string Line(int x1, int y1, int x2, int y2, int thickness = 1)
        {
            return $"^FO{x1},{y1}^GB{x2 - x1},{y2 - y1},{thickness}^FS";
        }

        /// <summary>
        /// Generate rectangle element
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="width">Rectangle width</param>
        /// <param name="height">Rectangle height</param>
        /// <param name="thickness">Border thickness</param>
        /// <returns>ZPL rectangle element</returns>
        public static string Rectangle(int x, int y, int width, int height, int thickness = 1)
        {
            return $"^FO{x},{y}^GB{width},{height},{thickness}^FS";
        }

        /// <summary>
        /// Generate filled rectangle element
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="width">Rectangle width</param>
        /// <param name="height">Rectangle height</param>
        /// <returns>ZPL filled rectangle element</returns>
        public static string FilledRectangle(int x, int y, int width, int height)
        {
            return $"^FO{x},{y}^GB{width},{height},{width}^FS";
        }

        /// <summary>
        /// Generate circle element
        /// </summary>
        /// <param name="x">Center X position</param>
        /// <param name="y">Center Y position</param>
        /// <param name="radius">Circle radius</param>
        /// <param name="thickness">Border thickness</param>
        /// <returns>ZPL circle element</returns>
        public static string Circle(int x, int y, int radius, int thickness = 1)
        {
            return $"^FO{x},{y}^GC{radius},{thickness}^FS";
        }

        /// <summary>
        /// Generate image element
        /// </summary>
        /// <param name="imageName">Image name (stored in printer)</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <returns>ZPL image element</returns>
        public static string Image(string imageName, int x, int y, int width, int height)
        {
            return $"^FO{x},{y}^XG{imageName},{width},{height}^FS";
        }

        /// <summary>
        /// Generate field origin command
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <returns>ZPL field origin command</returns>
        public static string FieldOrigin(int x, int y)
        {
            return $"^FO{x},{y}";
        }

        /// <summary>
        /// Generate field separator command
        /// </summary>
        /// <param name="separator">Field separator character</param>
        /// <returns>ZPL field separator command</returns>
        public static string FieldSeparator(char separator)
        {
            return $"^FS{separator}";
        }

        /// <summary>
        /// Generate label home command
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <returns>ZPL label home command</returns>
        public static string LabelHome(int x, int y)
        {
            return $"^LH{x},{y}";
        }

        /// <summary>
        /// Generate print orientation command
        /// </summary>
        /// <param name="orientation">Orientation (N=Normal, I=Inverted, B=Bottom-up)</param>
        /// <returns>ZPL print orientation command</returns>
        public static string PrintOrientation(string orientation = "N")
        {
            return $"^FW{orientation}";
        }

        /// <summary>
        /// Generate print speed command
        /// </summary>
        /// <param name="speed">Print speed (1-14)</param>
        /// <returns>ZPL print speed command</returns>
        public static string PrintSpeed(int speed)
        {
            return $"^PR{speed:D2}";
        }

        /// <summary>
        /// Generate print darkness command
        /// </summary>
        /// <param name="darkness">Print darkness (0-30)</param>
        /// <returns>ZPL print darkness command</returns>
        public static string PrintDarkness(int darkness)
        {
            return $"~SD{darkness:D2}";
        }

        /// <summary>
        /// Generate label width command
        /// </summary>
        /// <param name="width">Label width in dots</param>
        /// <returns>ZPL label width command</returns>
        public static string LabelWidth(int width)
        {
            return $"^PW{width}";
        }

        /// <summary>
        /// Generate label length command
        /// </summary>
        /// <param name="length">Label length in dots</param>
        /// <returns>ZPL label length command</returns>
        public static string LabelLength(int length)
        {
            return $"^LL{length}";
        }

        /// <summary>
        /// Generate media type command
        /// </summary>
        /// <param name="mediaType">Media type (continuous, die-cut, etc.)</param>
        /// <returns>ZPL media type command</returns>
        public static string MediaType(string mediaType)
        {
            return $"^MT{mediaType}";
        }

        /// <summary>
        /// Generate sensor type command
        /// </summary>
        /// <param name="sensorType">Sensor type (gap, black mark, etc.)</param>
        /// <returns>ZPL sensor type command</returns>
        public static string SensorType(string sensorType)
        {
            return $"^MN{sensorType}";
        }

        /// <summary>
        /// Generate print mode command
        /// </summary>
        /// <param name="mode">Print mode (T=Tear off, P=Peel off, R=Rewind, A=Applicator)</param>
        /// <returns>ZPL print mode command</returns>
        public static string PrintMode(string mode)
        {
            return $"^PM{mode}";
        }

        /// <summary>
        /// Generate quantity command
        /// </summary>
        /// <param name="quantity">Number of labels to print</param>
        /// <returns>ZPL quantity command</returns>
        public static string Quantity(int quantity)
        {
            return $"^PQ{quantity}";
        }

        /// <summary>
        /// Generate comment command
        /// </summary>
        /// <param name="comment">Comment text</param>
        /// <returns>ZPL comment command</returns>
        public static string Comment(string comment)
        {
            return $"^FX{comment}";
        }

        /// <summary>
        /// Generate test label
        /// </summary>
        /// <returns>Complete test label ZPL</returns>
        public static string GenerateTestLabel()
        {
            return GenerateLabel(
                Text("Zebra Test Label", 50, 50, "0", 50),
                Barcode("123456789", 50, 120, "1", 100, 3),
                Text($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", 50, 250, "0", 30),
                Text("Maui.Bluetooth.Utils", 50, 300, "0", 25),
                Rectangle(40, 40, 400, 300, 2)
            );
        }

        /// <summary>
        /// Generate simple text label
        /// </summary>
        /// <param name="text">Text to print</param>
        /// <param name="fontSize">Font size</param>
        /// <returns>Complete text label ZPL</returns>
        public static string GenerateTextLabel(string text, int fontSize = 30)
        {
            return GenerateLabel(
                Text(text, 50, 50, "0", fontSize),
                Rectangle(40, 40, 400, fontSize + 20, 1)
            );
        }

        /// <summary>
        /// Generate barcode label
        /// </summary>
        /// <param name="barcodeData">Barcode data</param>
        /// <param name="text">Text to display</param>
        /// <returns>Complete barcode label ZPL</returns>
        public static string GenerateBarcodeLabel(string barcodeData, string text = "")
        {
            var elements = new List<string>
            {
                Barcode(barcodeData, 50, 50, "1", 80, 3)
            };

            if (!string.IsNullOrEmpty(text))
            {
                elements.Add(Text(text, 50, 150, "0", 25));
            }

            elements.Add(Rectangle(40, 40, 400, 200, 1));

            return GenerateLabel(elements.ToArray());
        }

        /// <summary>
        /// Generate QR code label
        /// </summary>
        /// <param name="qrData">QR code data</param>
        /// <param name="text">Text to display</param>
        /// <returns>Complete QR code label ZPL</returns>
        public static string GenerateQrCodeLabel(string qrData, string text = "")
        {
            var elements = new List<string>
            {
                QrCode(qrData, 50, 50, 5, "M")
            };

            if (!string.IsNullOrEmpty(text))
            {
                elements.Add(Text(text, 50, 200, "0", 25));
            }

            elements.Add(Rectangle(40, 40, 300, 250, 1));

            return GenerateLabel(elements.ToArray());
        }
    }
} 