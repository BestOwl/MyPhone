using System;
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
        public const ushort _sdpServiceNameAttributeId = 0x100;

        // The SDP Type of the Service Name SDP attribute.
        // The first byte in the SDP Attribute encodes the SDP Attribute Type as follows :
        //    -  the Attribute Type size in the least significant 3 bits,
        //    -  the SDP Attribute Type value in the most significant 5 bits.
        public const byte _sdpServiceNameAttributeType = (4 << 3) | 5;

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
            // This should return a list of uncached Bluetooth services 
            // (so if the server was not active when paired, it will still be detected by this call
            RfcommDeviceServicesResult result = await _device.GetRfcommServicesForIdAsync(
                RfcommServiceId.FromUuid(ServiceUuid), BluetoothCacheMode.Uncached
                );
            if (result.Services.Count <= 0)
            {
                throw new BluetoothObexSessionException($"The remote bluetooth device does not support service: {ServiceUuid}");
            }

            _service = result.Services[0];
            DeviceAccessStatus accessStatus = await _service.RequestAccessAsync();
            if (accessStatus != DeviceAccessStatus.Allowed)
            {
                throw new BluetoothObexSessionException($"Not allowed to access this Rfcomm service. Reason: {accessStatus}");
            }

            #region Read SDP Service Name
            var attributes = await _service.GetSdpRawAttributesAsync();
            if (attributes.ContainsKey(_sdpServiceNameAttributeType))
            {
                var attributeReader = DataReader.FromBuffer(attributes[_sdpServiceNameAttributeId]);
                var attributeType = attributeReader.ReadByte();
                if (attributeType != _sdpServiceNameAttributeType)
                {
                    Console.WriteLine("The bluetooth service is using an unexpected format for the Service Name attribute. ");
                }
                else
                {
                    var serviceNameLength = attributeReader.ReadByte();

                    // The Service Name attribute requires UTF-8 encoding.
                    attributeReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;

                    var serviceName = attributeReader.ReadString(serviceNameLength);
                    Console.WriteLine($"Service: {serviceName},  Device: {_device.Name}");
                }
            }
            else
            {
                Console.WriteLine("The bluetooth service is not advertising the Service Name attribute (attribute id=0x100). ");
            }
            #endregion

            StreamSocket socket = new StreamSocket();
            try
            {
                await socket.ConnectAsync(_service.ConnectionHostName, _service.ConnectionServiceName
                , SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);
            }
            catch (Exception ex) when ((uint)ex.HResult == 0x80072740) // WSAEADDRINUSE
            {
                throw new BluetoothObexSessionException("Please verify that there is no other RFCOMM connection to the same device.", ex);
            }


            ObexClient = CreateObexClient(socket);
            try
            {
                await ObexClient.Connect(TargetObexService);
            }
            catch (ObexRequestException ex)
            {
                if (ex.Opcode == Opcode.OBEX_UNAUTHORIZED)
                {
                    throw new BluetoothObexSessionException($"Connected to OBEX server successfully, but the server refuse to provide service because you are not a authorized user.", ex);
                }
                throw new BluetoothObexSessionException($"Failed to connect to service: {ServiceUuid}. {ex.Message}", ex);
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
