using Fluffy.Net.Packets;
using Fluffy.Net.Packets.Modules;
using Fluffy.Net.Packets.Modules.Raw;

using System;
using System.Net.Sockets;
using Fluffy.Net.Packets.Modules.Streaming;

namespace Fluffy.Net
{
#if NET40

    public class ConnectionInfo : EventArgs, IDisposable
#else

    public class ConnectionInfo : IDisposable
#endif

    {
        public EventHandler<ConnectionInfo> OnDisposing;
        public Socket Socket { get; set; }

        public FluffySocket FluffySocket { get; set; }

        internal Receiver Receiver { get; private set; }
        public Sender Sender { get; private set; }

        public PacketRouter PacketHandler { get; private set; }
        public StreamPacketHandler StreamPacketHandler { get; }

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

            PacketHandler.RegisterPacket<DummyPacketHandler>();
#endif
            PacketHandler.RegisterPacket<FormattedPacketHandler>();
            StreamPacketHandler = PacketHandler.RegisterPacket<StreamPacketHandler>();

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