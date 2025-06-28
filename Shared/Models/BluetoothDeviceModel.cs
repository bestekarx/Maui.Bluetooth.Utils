namespace Maui.Bluetooth.Utils.Shared.Models
{
    /// <summary>
    /// Represents a Bluetooth device information
    /// </summary>
    public class BluetoothDeviceModel
    {
        /// <summary>
        /// Device name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Device address/MAC address
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Device ID (platform specific)
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Device type (ESC/POS, Zebra, etc.)
        /// </summary>
        public PrinterType PrinterType { get; set; } = PrinterType.Unknown;

        /// <summary>
        /// Connection state
        /// </summary>
        public ConnectionState State { get; set; } = ConnectionState.Disconnected;

        /// <summary>
        /// Signal strength (RSSI)
        /// </summary>
        public int Rssi { get; set; }

        /// <summary>
        /// Whether device is paired/bonded
        /// </summary>
        public bool IsPaired { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Address}) - {PrinterType}";
        }
    }

    /// <summary>
    /// Printer types supported by the library
    /// </summary>
    public enum PrinterType
    {
        /// <summary>
        /// Unknown printer type
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// ESC/POS compatible printers
        /// </summary>
        EscPos = 1,

        /// <summary>
        /// Zebra printers (ZPL)
        /// </summary>
        Zebra = 2,

        /// <summary>
        /// Generic Bluetooth printer
        /// </summary>
        Generic = 3
    }

    /// <summary>
    /// Bluetooth connection states
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>
        /// Not connected
        /// </summary>
        Disconnected = 0,

        /// <summary>
        /// Connecting to device
        /// </summary>
        Connecting = 1,

        /// <summary>
        /// Connected to device
        /// </summary>
        Connected = 2,

        /// <summary>
        /// Connection failed
        /// </summary>
        Failed = 3,

        /// <summary>
        /// Disconnecting from device
        /// </summary>
        Disconnecting = 4
    }
} 