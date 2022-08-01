using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;

namespace GoodTimeStudio.MyPhone.OBEX.Bluetooth
{
    public class BluetoothObexServerSessionClientEventArgs<T> where T : ObexServer
    {
        public BluetoothClientInformation ClientInfo { get; set; }

        public T ObexServer { get; set; }

        public BluetoothObexServerSessionClientEventArgs(BluetoothClientInformation clientInformation, T obexServer)
        {
            ClientInfo = clientInformation ?? throw new ArgumentNullException(nameof(clientInformation));
            ObexServer = obexServer ?? throw new ArgumentNullException(nameof(obexServer));
        }
    }

    public class BluetoothObexServerSessionClientAcceptedEventArgs<T> : BluetoothObexServerSessionClientEventArgs<T> where T : ObexServer
    {
        public BluetoothObexServerSessionClientAcceptedEventArgs(BluetoothClientInformation clientInformation, T obexServer) : base(clientInformation, obexServer)
        {
        }
    }

    public class BluetoothObexServerSessionClientDisconnectedEventArgs<T> : BluetoothObexServerSessionClientEventArgs<T> where T : ObexServer
    {
        /// <summary>
        /// The <see cref="ObexException"/> that causes the client disconnected.
        /// </summary>
        public ObexException ObexServerException { get; }

        public BluetoothObexServerSessionClientDisconnectedEventArgs(
            BluetoothClientInformation clientInformation,
            T obexServer,
            ObexException obexException) : base(clientInformation, obexServer)
        {
            ObexServerException = obexException;
        }
    }

    public abstract class BluetoothObexServerSession<T> : IDisposable where T : ObexServer
    {
        public delegate void BluetoothObexServerSessionClientAcceptedEventHandler(
            BluetoothObexServerSession<T> sender, BluetoothObexServerSessionClientAcceptedEventArgs<T> e);
        public event BluetoothObexServerSessionClientAcceptedEventHandler? ClientAccepted;

        public delegate void BluetoothObexServerSessionClientDisconnectedEventHandler(
            BluetoothObexServerSession<T> sender, BluetoothObexServerSessionClientDisconnectedEventArgs<T> args);
        public event BluetoothObexServerSessionClientDisconnectedEventHandler? ClientDisconnected;

        public Guid ServiceUuid { get; set; }

        public bool ServerStarted
        {
            get => _socketListener != null;
        }

        private StreamSocketListener? _socketListener;
        private RfcommServiceProvider? _serviceProvider;
        private Dictionary<BluetoothClientInformation, T> _connections;
        private uint _maxConnections;

        public BluetoothObexServerSession(Guid serviceUuid) : this(serviceUuid, 0)
        { }

        /// <summary>
        /// Initialize BluetoothObexServerSession
        /// </summary>
        /// <param name="serviceUuid">The bluetooth service UUID</param>
        /// <param name="maxConnection">The maximum number of connections allowed. 0 means no limits.</param>
        public BluetoothObexServerSession(Guid serviceUuid, uint maxConnections)
        {
            ServiceUuid = serviceUuid;
            _connections = new Dictionary<BluetoothClientInformation, T>();
            _maxConnections = maxConnections;
        }

        public async Task StartServerAsync()
        {
            if (ServerStarted)
            {
                throw new InvalidOperationException("The BluetoothObexServerSession is already started.");
            }

            StreamSocketListener socketListener;
            try
            {
                socketListener = new StreamSocketListener();
                socketListener.ConnectionReceived += SocketListener_ConnectionReceived;

                _serviceProvider = await RfcommServiceProvider.CreateAsync(RfcommServiceId.FromUuid(ServiceUuid));
                await socketListener.BindServiceNameAsync(_serviceProvider.ServiceId.AsString(),
                    SocketProtectionLevel.BluetoothEncryptionWithAuthentication);
                _serviceProvider.StartAdvertising(socketListener);
            }
            catch (Exception ex)
            {
                SocketErrorStatus socketErrorStatus = SocketError.GetStatus(ex.HResult);
                if (socketErrorStatus != SocketErrorStatus.Unknown)
                {
                    throw new BluetoothObexSessionException(
                        "Unable to bind and start the OBEX server on Bluetooth socket",
                        socketError: socketErrorStatus);
                }
                else
                {
                    throw;
                }
            }

            _socketListener = socketListener;
        }

        private void SocketListener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            if (_maxConnections > 0 && _connections.Count >= _maxConnections)
            {
                return;
            }
            T obexServer = CreateObexServer(args.Socket);
            BluetoothClientInformation clientInformation = new(args.Socket.Information.RemoteAddress, args.Socket.Information.RemoteServiceName);
            _connections[clientInformation] = obexServer;
            ClientAccepted?.Invoke(this, new BluetoothObexServerSessionClientAcceptedEventArgs<T>(clientInformation, obexServer));
            Task.Run(async () => await RunObexServer(obexServer, clientInformation));
        }

        private async Task RunObexServer(T obexSerber, BluetoothClientInformation clientInformation)
        {
            try
            {
                await obexSerber.Run();
            }
            catch (ObexException ex)
            {
                obexSerber.StopServer();
                _connections.Remove(clientInformation);
                ClientDisconnected?.Invoke(this, new BluetoothObexServerSessionClientDisconnectedEventArgs<T>(
                    clientInformation, obexSerber, ex));
            }
        }

        protected abstract T CreateObexServer(StreamSocket clientSocket);

        public void Dispose()
        {
            _serviceProvider?.StopAdvertising();
            foreach (T obexServer in _connections.Values)
            {
                obexServer.StopServer();
            }
            _socketListener?.Dispose();
        }
    }
}
