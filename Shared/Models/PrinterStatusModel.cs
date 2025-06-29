namespace Maui.Bluetooth.Utils.Shared.Models
{
    /// <summary>
    /// Printer status information
    /// </summary>
    public class PrinterStatusModel
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
        /// Whether printer is waiting for head down
        /// </summary>
        public bool IsWaitingForHeadDown { get; set; }

        /// <summary>
        /// Error message if any
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Printer temperature
        /// </summary>
        public int Temperature { get; set; }

        /// <summary>
        /// Print head status
        /// </summary>
        public PrintHeadStatus PrintHeadStatus { get; set; }

        /// <summary>
        /// Media status
        /// </summary>
        public MediaStatus MediaStatus { get; set; }

        /// <summary>
        /// Ribbon status
        /// </summary>
        public RibbonStatus RibbonStatus { get; set; }
    }

    /// <summary>
    /// Printer settings information
    /// </summary>
    public class PrinterSettingsModel
    {
        /// <summary>
        /// Print darkness level (0-30)
        /// </summary>
        public int PrintDarkness { get; set; }

        /// <summary>
        /// Print speed (1-14)
        /// </summary>
        public int PrintSpeed { get; set; }

        /// <summary>
        /// Label width in dots
        /// </summary>
        public int LabelWidth { get; set; }

        /// <summary>
        /// Label length in dots
        /// </summary>
        public int LabelLength { get; set; }

        /// <summary>
        /// Media type
        /// </summary>
        public string MediaType { get; set; } = string.Empty;

        /// <summary>
        /// Sensor type
        /// </summary>
        public string SensorType { get; set; } = string.Empty;

        /// <summary>
        /// Print mode
        /// </summary>
        public PrintMode PrintMode { get; set; }

        /// <summary>
        /// Print orientation
        /// </summary>
        public PrintOrientation PrintOrientation { get; set; }
    }

    /// <summary>
    /// Print head status
    /// </summary>
    public enum PrintHeadStatus
    {
        /// <summary>
        /// Print head is up
        /// </summary>
        Up = 0,

        /// <summary>
        /// Print head is down
        /// </summary>
        Down = 1,

        /// <summary>
        /// Print head is moving
        /// </summary>
        Moving = 2,

        /// <summary>
        /// Print head error
        /// </summary>
        Error = 3
    }

    /// <summary>
    /// Media status
    /// </summary>
    public enum MediaStatus
    {
        /// <summary>
        /// Media is loaded
        /// </summary>
        Loaded = 0,

        /// <summary>
        /// Media is out
        /// </summary>
        Out = 1,

        /// <summary>
        /// Media is moving
        /// </summary>
        Moving = 2,

        /// <summary>
        /// Media error
        /// </summary>
        Error = 3
    }

    /// <summary>
    /// Ribbon status
    /// </summary>
    public enum RibbonStatus
    {
        /// <summary>
        /// Ribbon is loaded
        /// </summary>
        Loaded = 0,

        /// <summary>
        /// Ribbon is out
        /// </summary>
        Out = 1,

        /// <summary>
        /// Ribbon is moving
        /// </summary>
        Moving = 2,

        /// <summary>
        /// Ribbon error
        /// </summary>
        Error = 3
    }

    /// <summary>
    /// Print mode
    /// </summary>
    public enum PrintMode
    {
        /// <summary>
        /// Tear off mode
        /// </summary>
        TearOff = 0,

        /// <summary>
        /// Peel off mode
        /// </summary>
        PeelOff = 1,

        /// <summary>
        /// Rewind mode
        /// </summary>
        Rewind = 2,

        /// <summary>
        /// Applicator mode
        /// </summary>
        Applicator = 3
    }

    /// <summary>
    /// Print orientation
    /// </summary>
    public enum PrintOrientation
    {
        /// <summary>
        /// Normal orientation
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Inverted orientation
        /// </summary>
        Inverted = 1
    }
} 