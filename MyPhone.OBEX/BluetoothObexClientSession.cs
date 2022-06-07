using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace MyPhone.OBEX
{
    public abstract class BluetoothObexClientSession<T> : IDisposable where T : ObexClient
    {
        public T? ObexClient { get; set; }

        public Guid ServiceUuid { get; set; }

        public ObexServiceUuid TargetObexService { get; set; }

        public bool Connected
        {
            get => _socket != null;
        }

        private BluetoothDevice _device;
        private RfcommDeviceService? _service;
        private StreamSocket? _socket;

        // The Id of the Service Name SDP attribute
        private const ushort _sdpServiceNameAttributeId = 0x100;

        // The SDP Type of the Service Name SDP attribute.
        // The first byte in the SDP Attribute encodes the SDP Attribute Type as follows :
        //    -  the Attribute Type size in the least significant 3 bits,
        //    -  the SDP Attribute Type value in the most significant 5 bits.
        private const byte _sdpServiceNameAttributeType = (4 << 3) | 5;

        public BluetoothObexClientSession(BluetoothDevice bluetoothDevice, Guid rfcommServiceUuid, ObexServiceUuid targetObexService)
        {
            ServiceUuid = rfcommServiceUuid;
            TargetObexService = targetObexService;
            _device = bluetoothDevice;
        }

        /// <summary>
        /// Establish bluetooth Rfcomm socket channel, and then initialize a ObexClient based on this socket.
        /// </summary>
        /// <exception cref="BluetoothObexSessionException">Failed to establish a bluetooth Rfcomm socket channel</exception>
        public async Task ConnectAsync()
        {
            RfcommDeviceServicesResult result = await _device.GetRfcommServicesAsync(BluetoothCacheMode.Uncached);

            if (result.Error != BluetoothError.Success)
            {
                throw new BluetoothObexSessionException(
                    $"BluetoothError: {result.Error}",
                    bluetoothError: result.Error);
            }

            if (result.Services.Count <= 0)
            {
                // TODO: improve remote device offline detection (maybe BLE?)
                throw new BluetoothDeviceNotAvailableException("Unable to connect to the remote Bluetooth device.");
            }
            RfcommDeviceService? service = result.Services.Where(rfs => rfs.ServiceId.Uuid == ServiceUuid).FirstOrDefault();
            if (service == null)
            {
                throw new BluetoothServiceNotSupportedException($"The remote bluetooth device does not support service: {ServiceUuid}");
            }
            _service = service;
            DeviceAccessStatus accessStatus = await _service.RequestAccessAsync();
            if (accessStatus != DeviceAccessStatus.Allowed)
            {
                throw new BluetoothObexSessionException($"The operating system does not allowed us to access this Rfcomm service. Reason: {accessStatus}");
            }

            StreamSocket socket = new StreamSocket();
            try
            {
                await socket.ConnectAsync(_service.ConnectionHostName, _service.ConnectionServiceName
                , SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);
            }
            catch (Exception ex)
            {
                socket.Dispose();

                SocketErrorStatus error = SocketError.GetStatus(ex.HResult);
                if (error != SocketErrorStatus.Unknown)
                {
                    throw new BluetoothObexSessionException(
                        $"Unable to connect to the remote RFCOMM service. Reason: {error}",
                        socketError: error);
                }
                else
                {
                    throw;
                }
            }


            ObexClient = CreateObexClient(socket);
            try
            {
                await ObexClient.Connect(TargetObexService);
            }
            catch (ObexRequestException ex)
            {
                socket.Dispose();
                if (ex.Opcode == Opcode.OBEX_UNAUTHORIZED)
                {
                    throw new BluetoothObexSessionException(
                        $"Connected to OBEX server successfully, but the server refuse to provide service. Reason: You are not an authorized user.", 
                        innerException: ex);
                }
                throw new BluetoothObexSessionException(
                    $"Connected to OBEX server successfully, but the server refuse to provide service. Reason: Got an unsuccessful response from server ({ex.Opcode})", 
                    innerException: ex);
            }
            _socket = socket;
        }

        /// <summary>
        /// Create a new ObexClient instance.
        /// </summary>
        /// <param name="socket">Stream socket</param>
        /// <returns>ObexClient</returns>
        protected abstract T CreateObexClient(StreamSocket socket);

        public void Dispose()
        {
            if (_socket != null)
            {
                _socket.Dispose();
            }
            if (_service != null)
            {
                _service.Dispose();
            }
        }
    }
}
