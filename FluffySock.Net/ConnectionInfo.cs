using Fluffy.Net.Packets;
using Fluffy.Net.Packets.Modules;
using Fluffy.Net.Packets.Modules.Raw;

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
        public Sender Sender { get; private set; }

        public PacketRouter PacketHandler { get; private set; }

        public ConnectionInfo(FluffySocket fluffySocket)
            : this(fluffySocket.Socket, fluffySocket)
        {
        }

        public ConnectionInfo(Socket socket, FluffySocket fluffySocket)
        {
            Socket = socket;
            FluffySocket = fluffySocket;

            PacketHandler = new PacketRouter(this);

            Sender = new Sender(this);
            Receiver = new Receiver(socket);

            Receiver.OnReceive += PacketHandler.Handle;
#if DEBUG

            PacketHandler.RegisterPacket<DummyPacket>();
#endif
            PacketHandler.RegisterPacket<FormattedPacket>();

            PacketHandler
                .On<ConnectionInfo>().Do(x => Console.Write($"You are awesome :3"))
                .Default(x => Console.Write($"Lol, Default"));
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