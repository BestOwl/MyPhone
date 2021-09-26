using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace MyPhone.OBEX
{
    public abstract class BluetoothObexServerSession<T> : IDisposable where T : ObexServer
    {

        public Guid ServiceUuid { get; set; }

        public bool ServerStarted
        {
            get => _socketListener != null;
        }

        private StreamSocketListener? _socketListener;
        private RfcommServiceProvider? _serviceProvider;
        private Dictionary<HostName, T> _connections;
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
            _connections = new Dictionary<HostName, T>();
            _maxConnections = maxConnections;
        }

        public async Task StartServer()
        {
            if (ServerStarted)
            {
                throw new InvalidOperationException("The BluetoothObexServerSession is already started.");
            }

            StreamSocketListener socketListener = new StreamSocketListener();
            socketListener.ConnectionReceived += SocketListener_ConnectionReceived;

            _serviceProvider = await RfcommServiceProvider.CreateAsync(RfcommServiceId.FromUuid(ServiceUuid));
            _serviceProvider.StartAdvertising(socketListener);

            _socketListener = socketListener;
        }

        private void SocketListener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            if (_maxConnections > 0 && _connections.Count + 1 > _maxConnections)
            {
                _serviceProvider!.StopAdvertising();
            }
            _connections[args.Socket.Information.RemoteAddress] = StartObexServer(args.Socket);
        }

        protected abstract T StartObexServer(StreamSocket clientSocket);

        public void Dispose()
        {
            foreach (T obexServer in _connections.Values)
            {
                obexServer.StopServer();
            }
            if (_socketListener != null)
            {
                _socketListener.Dispose();
            }
        }
    }
}
