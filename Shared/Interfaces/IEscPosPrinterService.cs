using Maui.Bluetooth.Utils.Shared.Models;
using TextAlignment = Maui.Bluetooth.Utils.Shared.Models.TextAlignment;

namespace Maui.Bluetooth.Utils.Shared.Interfaces
{
    /// <summary>
    /// ESC/POS printer service interface
    /// </summary>
    public interface IEscPosPrinterService : IPrinterService
    {
        /// <summary>
        /// Initialize printer with ESC/POS commands
        /// </summary>
        /// <returns>True if initialization successful</returns>
        Task<bool> InitializeEscPosAsync();

        /// <summary>
        /// Set character code page
        /// </summary>
        /// <param name="codePage">Code page number</param>
        /// <returns>True if setting applied successfully</returns>
        Task<bool> SetCodePageAsync(int codePage);

        /// <summary>
        /// Set print density
        /// </summary>
        /// <param name="density">Print density value</param>
        /// <returns>True if setting applied successfully</returns>
        Task<bool> SetPrintDensityAsync(int density);

        /// <summary>
        /// Set print speed
        /// </summary>
        /// <param name="speed">Print speed value</param>
        /// <returns>True if setting applied successfully</returns>
        Task<bool> SetPrintSpeedAsync(int speed);

        /// <summary>
        /// Print double height text
        /// </summary>
        /// <param name="text">Text to print</param>
        /// <param name="alignment">Text alignment</param>
        /// <returns>True if print successful</returns>
        Task<bool> PrintDoubleHeightTextAsync(string text, TextAlignment alignment = TextAlignment.Left);

        /// <summary>
        /// Print double width text
        /// </summary>
        /// <param name="text">Text to print</param>
        /// <param name="alignment">Text alignment</param>
        /// <returns>True if print successful</returns>
        Task<bool> PrintDoubleWidthTextAsync(string text, TextAlignment alignment = TextAlignment.Left);

        /// <summary>
        /// Print double height and width text
        /// </summary>
        /// <param name="text">Text to print</param>
        /// <param name="alignment">Text alignment</param>
        /// <returns>True if print successful</returns>
        Task<bool> PrintDoubleSizeTextAsync(string text, TextAlignment alignment = TextAlignment.Left);

        /// <summary>
        /// Print inverted text
        /// </summary>
        /// <param name="text">Text to print</param>
        /// <param name="alignment">Text alignment</param>
        /// <returns>True if print successful</returns>
        Task<bool> PrintInvertedTextAsync(string text, TextAlignment alignment = TextAlignment.Left);

        /// <summary>
        /// Print upside down text
        /// </summary>
        /// <param name="text">Text to print</param>
        /// <param name="alignment">Text alignment</param>
        /// <returns>True if print successful</returns>
        Task<bool> PrintUpsideDownTextAsync(string text, TextAlignment alignment = TextAlignment.Left);

        /// <summary>
        /// Print text with custom font
        /// </summary>
        /// <param name="text">Text to print</param>
        /// <param name="font">Font number (0-9)</param>
        /// <param name="alignment">Text alignment</param>
        /// <returns>True if print successful</returns>
        Task<bool> PrintTextWithFontAsync(string text, int font, TextAlignment alignment = TextAlignment.Left);

        /// <summary>
        /// Print partial cut
        /// </summary>
        /// <returns>True if cut successful</returns>
        Task<bool> PartialCutAsync();

        /// <summary>
        /// Print full cut
        /// </summary>
        /// <returns>True if cut successful</returns>
        Task<bool> FullCutAsync();

        /// <summary>
        /// Feed paper forward
        /// </summary>
        /// <param name="lines">Number of lines to feed</param>
        /// <returns>True if feed successful</returns>
        Task<bool> FeedPaperAsync(int lines = 1);

        /// <summary>
        /// Feed paper backward
        /// </summary>
        /// <param name="lines">Number of lines to feed</param>
        /// <returns>True if feed successful</returns>
        Task<bool> FeedPaperBackwardAsync(int lines = 1);

        /// <summary>
        /// Print bitmap with custom width
        /// </summary>
        /// <param name="imageData">Image data</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="mode">Print mode</param>
        /// <returns>True if print successful</returns>
        Task<bool> PrintBitmapAsync(byte[] imageData, int width, int height, int mode = 0);
    }
} 