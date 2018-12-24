using Fluffy.Fluent;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Fluffy.Net
{
    public class FluffyServer : FluffySocket
    {
        public IReadOnlyCollection<ConnectionInfo> Connections => _connections;
        private List<ConnectionInfo> _connections;

        public ContextAwareTypeSwitch<ConnectionInfo> PacketHandler { get; private set; }

        public EventHandler<ConnectionInfo> OnNewConnection;

        public FluffyServer(int port)
            : this(IPAddress.Any, port)
        {
        }

        public FluffyServer(IPAddress address, int port) : base(nameof(FluffyServer))
        {
            PacketHandler = new ContextAwareTypeSwitch<ConnectionInfo>(null);
            _connections = new List<ConnectionInfo>();
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                Blocking = false,
                NoDelay = true,
                ReceiveTimeout = int.MaxValue,
                SendTimeout = int.MaxValue
            };

            Socket.Bind(new IPEndPoint(address, port));
            Socket.Listen((int)SocketOptionName.MaxConnections);
            Socket.LingerState = new LingerOption(true, 1);
        }

        public FluffyServer Start()
        {
            for (int i = 0; i < 15; i++)
            {
                Socket.BeginAccept(AcceptCallback, null);
            }

            return this;
        }

        private void AcceptCallback(IAsyncResult result)
        {
            try
            {
                var socket = Socket.EndAccept(result);
                var connectionInfo = new ConnectionInfo(socket, this);
                connectionInfo.OnDisposing += (_, x) =>
                {
                    _connections.Remove(connectionInfo);
                };
                connectionInfo.Receiver.Start();
                connectionInfo.PacketHandler.Default(x => PacketHandler.Handle(x, connectionInfo));
                _connections.Add(connectionInfo);

                OnNewConnection?.BeginInvoke(this, connectionInfo, null, null);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            Socket.BeginAccept(AcceptCallback, null);
        }
    }
}