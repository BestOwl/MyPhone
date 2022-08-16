using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace GoodTimeStudio.MyPhone.OBEX.Bluetooth
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

        public BluetoothDevice Device { get; }

        /// <summary>
        /// Raw SDP records of the RFComm service. Not null after calling <see cref="ConnectAsync"/>
        /// </summary>
        public IReadOnlyDictionary<uint, IReadOnlyCollection<byte>>? SdpRecords { get; private set; }

        private RfcommDeviceService? _service;
        private StreamSocket? _socket;

        public BluetoothObexClientSession(BluetoothDevice bluetoothDevice, Guid rfcommServiceUuid, ObexServiceUuid targetObexService)
        {
            ServiceUuid = rfcommServiceUuid;
            TargetObexService = targetObexService;
            Device = bluetoothDevice;
        }

        /// <summary>
        /// Establish bluetooth Rfcomm socket channel, and then initialize a ObexClient based on this socket.
        /// </summary>
        /// <exception cref="BluetoothObexSessionException">Failed to establish a bluetooth Rfcomm socket channel</exception>
        public async Task ConnectAsync()
        {
            RfcommDeviceServicesResult result = await Device.GetRfcommServicesAsync(BluetoothCacheMode.Uncached);

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
            var sdpRecords = new Dictionary<uint, IReadOnlyCollection<byte>>();
            foreach (KeyValuePair<uint, IBuffer> pair in await _service.GetSdpRawAttributesAsync())
            {
                sdpRecords[pair.Key] = pair.Value.ToArray();
            }
            SdpRecords = sdpRecords;
            if (!CheckFeaturesRequirementBySdpRecords())
            {
                throw new BluetoothServiceNotSupportedException($"The remote bluetooth device provided the required servic: {ServiceUuid}, but it does not meet the feature requirements.");
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
                await ObexClient.ConnectAsync(TargetObexService);
            }
            catch (ObexRequestException ex)
            {
                socket.Dispose();
                if (ex.Opcode.ObexOperation == ObexOperation.Unauthorized)
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
        public abstract T CreateObexClient(StreamSocket socket);

        /// <summary>
        /// Checking agasint SDP records of the RFComm service to test whether it provides necessary features.
        /// </summary>
        /// <remarks>You can assume <see cref="SdpRecords"/> is not null.</remarks>
        /// <returns>
        /// True if all features requirement (if any) are met. 
        /// Otherwise false, and then the connection request will be aborted (a <see cref="BluetoothServiceNotSupportedException"/> will be thrown).
        /// </returns>
        protected virtual bool CheckFeaturesRequirementBySdpRecords()
        {
            Debug.Assert(SdpRecords != null);
            return true;
        }

        public virtual void Dispose()
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
