using System;
using System.Net;
using System.Net.Sockets;

namespace Fluffy.Net
{
    public class FluffyServer : FluffySocket
    {
        public FluffyServer(int port)
            : this(IPAddress.Any, port)
        {
        }

        public FluffyServer(IPAddress address, int port)
        {
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
                Socket.BeginAccept(AcceptCallback, null);

            return this;
        }

        private void AcceptCallback(IAsyncResult result)
        {
            try
            {
                var socket = Socket.EndAccept(result);

                var connectionInfo = new ConnectionInfo(socket, this);

                //connectionInfo.IOHandler.BeginReceive(4, ReceiveCallback, connectionInfo);
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

    internal class ConnectionInfo : IDisposable
    {
        public Socket Socket { get; set; }

        public FluffySocket FluffySocket { get; set; }

        internal IOHandler IOHandler { get; set; }

        public ConnectionInfo(Socket socket, FluffySocket fluffySocket)
        {
            Socket = socket;
            FluffySocket = fluffySocket;

            IOHandler = new IOHandler(socket);
        }

        public void Dispose()
        {
            Socket?.Dispose();
        }
    }

    // ReSharper disable once InconsistentNaming
    internal class IOHandler
    {
        private readonly Socket _socket;
        private byte[] _buffer;
        private IOState _state;
        private volatile bool _started;

        public IOHandler(Socket socket)
        {
            _socket = socket;
            _buffer = new byte[8 * 1024];
        }

        public IOHandler Start()
        {
            if (_started)
            {
                throw new AggregateException($"Handler already started");
            }

            _started = true;
            _socket.BeginReceive(_buffer, 0, 4, SocketFlags.None, ReceiveCallback, null);
            return this;
        }

        private protected void ReceiveCallback(IAsyncResult ar)
        {
            var connectionInfo = ar.AsyncState as ConnectionInfo;
            if (connectionInfo == null)
            {
                Console.WriteLine($"The given type did not match the expected one " +
                                  $"(Received: {ar}; Expected: {nameof(ConnectionInfo)})");
                return;
            }
            int bytesRead = connectionInfo.Socket.EndReceive(ar);
            if (bytesRead == 0)
            {
                Console.WriteLine($"Received 0 bytes, closing Socket!");
                return;
            }
        }

        // ReSharper disable once InconsistentNaming
        private enum IOState
        {
            HeaderLen,
            BodyBytes,
        }
    }
}