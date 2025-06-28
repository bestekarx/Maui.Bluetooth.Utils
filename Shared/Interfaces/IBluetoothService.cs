using Maui.Bluetooth.Utils.Shared.Models;

namespace Maui.Bluetooth.Utils.Shared.Interfaces
{
    /// <summary>
    /// Bluetooth service interface for device discovery and connection
    /// </summary>
    public interface IBluetoothService
    {
        /// <summary>
        /// Event fired when connection state changes
        /// </summary>
        event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

        /// <summary>
        /// Event fired when a new device is discovered
        /// </summary>
        event EventHandler<BluetoothDeviceModel>? DeviceDiscovered;

        /// <summary>
        /// Check if Bluetooth is available and enabled
        /// </summary>
        /// <returns>True if Bluetooth is available and enabled</returns>
        Task<bool> IsBluetoothAvailableAsync();

        /// <summary>
        /// Request Bluetooth permissions
        /// </summary>
        /// <returns>True if permissions are granted</returns>
        Task<bool> RequestPermissionsAsync();

        /// <summary>
        /// Start scanning for Bluetooth devices
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of discovered devices</returns>
        Task<List<BluetoothDeviceModel>> ScanForDevicesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get paired/bonded devices
        /// </summary>
        /// <returns>List of paired devices</returns>
        Task<List<BluetoothDeviceModel>> GetPairedDevicesAsync();

        /// <summary>
        /// Connect to a Bluetooth device
        /// </summary>
        /// <param name="device">Device to connect to</param>
        /// <returns>True if connection successful</returns>
        Task<bool> ConnectAsync(BluetoothDeviceModel device);

        /// <summary>
        /// Disconnect from current device
        /// </summary>
        /// <returns>True if disconnection successful</returns>
        Task<bool> DisconnectAsync();

        /// <summary>
        /// Send data to connected device
        /// </summary>
        /// <param name="data">Data to send</param>
        /// <returns>True if data sent successfully</returns>
        Task<bool> SendDataAsync(byte[] data);

        /// <summary>
        /// Get current connection state
        /// </summary>
        /// <returns>Current connection state</returns>
        ConnectionState GetConnectionState();

        /// <summary>
        /// Get currently connected device
        /// </summary>
        /// <returns>Connected device or null</returns>
        BluetoothDeviceModel? GetConnectedDevice();
    }

    /// <summary>
    /// Event arguments for connection state changes
    /// </summary>
    public class ConnectionStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Previous connection state
        /// </summary>
        public ConnectionState PreviousState { get; }

        /// <summary>
        /// Current connection state
        /// </summary>
        public ConnectionState CurrentState { get; }

        /// <summary>
        /// Device associated with the state change
        /// </summary>
        public BluetoothDeviceModel? Device { get; }

        /// <summary>
        /// Error message if connection failed
        /// </summary>
        public string? ErrorMessage { get; }

        public ConnectionStateChangedEventArgs(ConnectionState previousState, ConnectionState currentState, BluetoothDeviceModel? device = null, string? errorMessage = null)
        {
            PreviousState = previousState;
            CurrentState = currentState;
            Device = device;
            ErrorMessage = errorMessage;
        }
    }
} 