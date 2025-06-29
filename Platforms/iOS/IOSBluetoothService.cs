using CoreBluetooth;
using Foundation;
using Maui.Bluetooth.Utils.Shared.Interfaces;
using Maui.Bluetooth.Utils.Shared.Models;
using Maui.Bluetooth.Utils.Shared.Services;

namespace Maui.Bluetooth.Utils.Platforms.iOS
{
    public class IOSBluetoothService : NSObject, IBluetoothService, ICBCentralManagerDelegate, ICBPeripheralDelegate
    {
        private CBCentralManager? _centralManager;
        private CBPeripheral? _connectedPeripheral;
        private CBCharacteristic? _writeCharacteristic;
        private CBCharacteristic? _readCharacteristic;
        private BluetoothDeviceModel? _connectedDevice;
        private ConnectionState _state = ConnectionState.Disconnected;
        private CancellationTokenSource? _scanCts;
        private List<CBPeripheral> _discoveredPeripherals = new();

        public event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;
        public event EventHandler<BluetoothDeviceModel>? DeviceDiscovered;

        public IOSBluetoothService()
        {
            _centralManager = new CBCentralManager(this, null);
        }

        public Task<bool> IsBluetoothAvailableAsync()
        {
            return Task.FromResult(_centralManager?.State == CBManagerState.PoweredOn);
        }

        public async Task<bool> RequestPermissionsAsync()
        {
            // iOS'ta Bluetooth izinleri Info.plist'te tanımlanmalı
            // Burada sadece Bluetooth'un açık olup olmadığını kontrol ediyoruz
            return await IsBluetoothAvailableAsync();
        }

        public async Task<List<BluetoothDeviceModel>> ScanForDevicesAsync(CancellationToken cancellationToken = default)
        {
            var devices = new List<BluetoothDeviceModel>();
            
            if (_centralManager?.State != CBManagerState.PoweredOn)
                return devices;

            _scanCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _discoveredPeripherals.Clear();

            // ESC/POS ve Zebra printer servislerini arıyoruz
            var serviceUuids = new CBUUID[]
            {
                CBUUID.FromString("00001101-0000-1000-8000-00805F9B34FB"), // Serial Port Profile
                CBUUID.FromString("000018F0-0000-1000-8000-00805F9B34FB"), // ESC/POS Service
                CBUUID.FromString("000018F1-0000-1000-8000-00805F9B34FB")  // Zebra Service
            };

            _centralManager.ScanForPeripherals(serviceUuids, new PeripheralScanningOptions());

            // 10 saniye tarama yap
            await Task.Delay(10000, _scanCts.Token);
            
            _centralManager.StopScan();

            // Keşfedilen cihazları dönüştür
            foreach (var peripheral in _discoveredPeripherals)
            {
                var model = new BluetoothDeviceModel
                {
                    Name = peripheral.Name ?? "Unknown Device",
                    Address = peripheral.Identifier.AsString(),
                    Id = peripheral.Identifier.AsString(),
                    PrinterType = BluetoothPrinterManager.AutoDetectPrinterType(peripheral.Name ?? ""),
                    State = ConnectionState.Disconnected,
                    IsPaired = false,
                    Rssi = 0
                };
                devices.Add(model);
                DeviceDiscovered?.Invoke(this, model);
            }

            return devices;
        }

        public Task<List<BluetoothDeviceModel>> GetPairedDevicesAsync()
        {
            var devices = new List<BluetoothDeviceModel>();
            
            if (_centralManager?.State != CBManagerState.PoweredOn)
                return Task.FromResult(devices);

            // iOS'ta eşleşmiş cihazları almak için farklı bir yaklaşım gerekir
            // Bu basit bir implementasyon
            return Task.FromResult(devices);
        }

        public async Task<bool> ConnectAsync(BluetoothDeviceModel device)
        {
            if (_centralManager?.State != CBManagerState.PoweredOn || device == null)
                return false;

            try
            {
                _state = ConnectionState.Connecting;
                ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(ConnectionState.Disconnected, ConnectionState.Connecting, device));

                // Cihazı bul
                var peripheral = _discoveredPeripherals.FirstOrDefault(p => p.Identifier.AsString() == device.Id);
                if (peripheral == null)
                {
                    _state = ConnectionState.Failed;
                    ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(ConnectionState.Connecting, ConnectionState.Failed, device, "Device not found"));
                    return false;
                }

                _connectedPeripheral = peripheral;
                _connectedPeripheral.Delegate = this;

                // Cihaza bağlan
                _centralManager.ConnectPeripheral(peripheral, new PeripheralConnectionOptions());

                // Bağlantı için bekle
                var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                while (_state == ConnectionState.Connecting && !timeoutCts.Token.IsCancellationRequested)
                {
                    await Task.Delay(100);
                }

                if (_state == ConnectionState.Connected)
                {
                    _connectedDevice = device;
                    return true;
                }
                else
                {
                    _state = ConnectionState.Failed;
                    ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(ConnectionState.Connecting, ConnectionState.Failed, device, "Connection timeout"));
                    return false;
                }
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
                if (_connectedPeripheral != null && _centralManager != null)
                {
                    _centralManager.CancelPeripheralConnection(_connectedPeripheral);
                }

                _connectedPeripheral = null;
                _writeCharacteristic = null;
                _readCharacteristic = null;
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
            if (_connectedPeripheral == null || _writeCharacteristic == null || data == null)
                return false;

            try
            {
                var nsData = NSData.FromArray(data);
                _connectedPeripheral.WriteValue(nsData, _writeCharacteristic, CBCharacteristicWriteType.WithResponse);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<byte[]> ReadDataAsync(int timeout = 1000)
        {
            if (_connectedPeripheral == null || _readCharacteristic == null)
                return Array.Empty<byte>();
            try
            {
                var tcs = new TaskCompletionSource<byte[]>();
                EventHandler<CBCharacteristicEventArgs> handler = (s, e) =>
                {
                    if (e.Characteristic == _readCharacteristic && e.Characteristic.Value != null)
                        tcs.TrySetResult(e.Characteristic.Value.ToArray());
                };
                _connectedPeripheral.UpdatedCharacterteristicValue += handler;
                _connectedPeripheral.ReadValue(_readCharacteristic);
                var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(timeout));
                _connectedPeripheral.UpdatedCharacterteristicValue -= handler;
                if (completedTask == tcs.Task)
                    return tcs.Task.Result;
                return Array.Empty<byte>();
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }

        public ConnectionState GetConnectionState() => _state;
        public BluetoothDeviceModel? GetConnectedDevice() => _connectedDevice;

        // ICBCentralManagerDelegate implementation
        public void UpdatedState(CBCentralManager central)
        {
            // Bluetooth state değişikliklerini handle et
        }

        public void DiscoveredPeripheral(CBCentralManager central, CBPeripheral peripheral, NSDictionary advertisementData, NSNumber RSSI)
        {
            if (!_discoveredPeripherals.Any(p => p.Identifier.Equals(peripheral.Identifier)))
            {
                _discoveredPeripherals.Add(peripheral);
            }
        }

        public void ConnectedPeripheral(CBCentralManager central, CBPeripheral peripheral)
        {
            if (_connectedPeripheral?.Identifier.Equals(peripheral.Identifier) == true)
            {
                _state = ConnectionState.Connected;
                ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(ConnectionState.Connecting, ConnectionState.Connected, _connectedDevice));
                
                // Servisleri keşfet
                peripheral.DiscoverServices();
            }
        }

        public void FailedToConnectPeripheral(CBCentralManager central, CBPeripheral peripheral, NSError error)
        {
            if (_connectedPeripheral?.Identifier.Equals(peripheral.Identifier) == true)
            {
                _state = ConnectionState.Failed;
                ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(ConnectionState.Connecting, ConnectionState.Failed, _connectedDevice, error?.LocalizedDescription));
            }
        }

        public void DisconnectedPeripheral(CBCentralManager central, CBPeripheral peripheral, NSError error)
        {
            if (_connectedPeripheral?.Identifier.Equals(peripheral.Identifier) == true)
            {
                _state = ConnectionState.Disconnected;
                ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(ConnectionState.Connected, ConnectionState.Disconnected, _connectedDevice));
                
                _connectedPeripheral = null;
                _writeCharacteristic = null;
                _readCharacteristic = null;
                _connectedDevice = null;
            }
        }

        // ICBPeripheralDelegate implementation
        public void DiscoveredServices(CBPeripheral peripheral, NSError error)
        {
            if (error != null) return;

            foreach (var service in peripheral.Services)
            {
                // ESC/POS veya Zebra servislerini ara
                if (service.UUID.ToString().Contains("18F0") || service.UUID.ToString().Contains("18F1") || 
                    service.UUID.ToString().Contains("1101"))
                {
                    peripheral.DiscoverCharacteristics(service);
                }
            }
        }

        public void DiscoveredCharacteristics(CBPeripheral peripheral, CBService service, NSError error)
        {
            if (error != null) return;

            foreach (var characteristic in service.Characteristics)
            {
                // Write characteristic'ini bul
                if (characteristic.Properties.HasFlag(CBCharacteristicProperties.Write) || 
                    characteristic.Properties.HasFlag(CBCharacteristicProperties.WriteWithoutResponse))
                {
                    _writeCharacteristic = characteristic;
                }

                // Read characteristic'ini bul
                if (characteristic.Properties.HasFlag(CBCharacteristicProperties.Read) || 
                    characteristic.Properties.HasFlag(CBCharacteristicProperties.Notify))
                {
                    _readCharacteristic = characteristic;
                    
                    // Notify'ı aktifleştir
                    if (characteristic.Properties.HasFlag(CBCharacteristicProperties.Notify))
                    {
                        peripheral.SetNotifyValue(true, characteristic);
                    }
                }
            }
        }

        public void UpdatedCharacterteristicValue(CBPeripheral peripheral, CBCharacteristic characteristic, NSError error)
        {
            if (error != null) return;

            // Gelen veriyi handle et
            var data = characteristic.Value?.ToArray();
            if (data != null)
            {
                // Burada gelen veriyi işleyebilirsiniz
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _scanCts?.Cancel();
                _scanCts?.Dispose();
                _centralManager?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
} 