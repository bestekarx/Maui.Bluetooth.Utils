using Android.Bluetooth;
using Java.Util;
using Maui.Bluetooth.Utils.Shared.Interfaces;
using Maui.Bluetooth.Utils.Shared.Models;
using Maui.Bluetooth.Utils.Shared.Services;

namespace Maui.Bluetooth.Utils.Platforms.Android
{
    public class AndroidBluetoothService : IBluetoothService
    {
        private BluetoothAdapter? _adapter;
        private BluetoothSocket? _socket;
        private BluetoothDeviceModel? _connectedDevice;
        private ConnectionState _state = ConnectionState.Disconnected;
        private CancellationTokenSource? _scanCts;

        public event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;
        public event EventHandler<BluetoothDeviceModel>? DeviceDiscovered;

        public AndroidBluetoothService()
        {
            _adapter = BluetoothAdapter.DefaultAdapter;
        }

        public Task<bool> IsBluetoothAvailableAsync()
        {
            return Task.FromResult(_adapter != null && _adapter.IsEnabled);
        }

        public async Task<bool> RequestPermissionsAsync()
        {
            // Android 12+ için runtime izinleri burada kontrol edilmeli
            // Şimdilik true dönüyoruz (izin yönetimi uygulama tarafında yapılmalı)
            return await Task.FromResult(true);
        }

        public async Task<List<BluetoothDeviceModel>> ScanForDevicesAsync(CancellationToken cancellationToken = default)
        {
            var devices = new List<BluetoothDeviceModel>();
            if (_adapter == null)
                return devices;

            _scanCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _adapter.StartDiscovery();

            // Sadece eşleşmiş cihazları döndürüyoruz (daha gelişmiş discovery için BroadcastReceiver eklenebilir)
            foreach (var device in _adapter.BondedDevices)
            {
                var model = new BluetoothDeviceModel
                {
                    Name = device.Name,
                    Address = device.Address,
                    Id = device.Address,
                    PrinterType = BluetoothPrinterManager.AutoDetectPrinterType(device.Name),
                    State = ConnectionState.Disconnected,
                    IsPaired = true
                };
                devices.Add(model);
                DeviceDiscovered?.Invoke(this, model);
            }
            await Task.Delay(1000, _scanCts.Token); // Simüle discovery
            _adapter.CancelDiscovery();
            return devices;
        }

        public Task<List<BluetoothDeviceModel>> GetPairedDevicesAsync()
        {
            var devices = new List<BluetoothDeviceModel>();
            if (_adapter == null)
                return Task.FromResult(devices);
            foreach (var device in _adapter.BondedDevices)
            {
                devices.Add(new BluetoothDeviceModel
                {
                    Name = device.Name,
                    Address = device.Address,
                    Id = device.Address,
                    PrinterType = BluetoothPrinterManager.AutoDetectPrinterType(device.Name),
                    State = ConnectionState.Disconnected,
                    IsPaired = true
                });
            }
            return Task.FromResult(devices);
        }

        public async Task<bool> ConnectAsync(BluetoothDeviceModel device)
        {
            if (_adapter == null || device == null)
                return false;
            try
            {
                var btDevice = _adapter.GetRemoteDevice(device.Address);
                var uuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
                _socket = btDevice.CreateRfcommSocketToServiceRecord(uuid);
                _adapter.CancelDiscovery();
                await _socket.ConnectAsync();
                _connectedDevice = device;
                _state = ConnectionState.Connected;
                ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(ConnectionState.Connecting, ConnectionState.Connected, device));
                return true;
            }
            catch (Exception ex)
            {
                _state = ConnectionState.Failed;
                ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(ConnectionState.Connecting, ConnectionState.Failed, device, ex.Message));
                return false;
            }
        }

        public async Task<bool> DisconnectAsync()
        {
            try
            {
                if (_socket != null)
                {
                    _socket.Close();
                    _socket = null;
                }
                _state = ConnectionState.Disconnected;
                ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(ConnectionState.Connected, ConnectionState.Disconnected, _connectedDevice));
                _connectedDevice = null;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendDataAsync(byte[] data)
        {
            if (_socket == null || data == null)
                return false;
            try
            {
                await _socket.OutputStream.WriteAsync(data, 0, data.Length);
                await _socket.OutputStream.FlushAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public ConnectionState GetConnectionState() => _state;
        public BluetoothDeviceModel? GetConnectedDevice() => _connectedDevice;
    }
} 