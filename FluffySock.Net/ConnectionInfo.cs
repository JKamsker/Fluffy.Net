using Fluffy.Net.Packets;

using System;
using System.Net.Sockets;

namespace Fluffy.Net
{
    public class ConnectionInfo : IDisposable
    {
        public EventHandler<ConnectionInfo> OnDisposing;
        public Socket Socket { get; set; }

        public FluffySocket FluffySocket { get; set; }

        internal Receiver Receiver { get; private set; }
        internal Sender Sender { get; private set; }
        public PacketHandler PacketHandler { get; private set; }

        public ConnectionInfo(FluffySocket fluffySocket)
            : this(fluffySocket.Socket, fluffySocket)
        {
        }

        public ConnectionInfo(Socket socket, FluffySocket fluffySocket)
        {
            Socket = socket;
            FluffySocket = fluffySocket;

            Receiver = new Receiver(socket);
            Sender = new Sender(this);
            PacketHandler = new PacketHandler(this);

            Receiver.OnReceive += PacketHandler.Handle;

            PacketHandler.RegisterPacket<DummyPacket>();
        }

        public void Dispose()
        {
            Socket?.Dispose();
            // ReSharper disable once DelegateSubtraction
            Receiver.OnReceive -= PacketHandler.Handle;
            OnDisposing?.Invoke(this, this);
        }
    }
}