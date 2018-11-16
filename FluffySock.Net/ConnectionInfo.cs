using Fluffy.Net.Packets;

using System;
using System.Diagnostics;
using System.Net.Sockets;
using Fluffy.Net.Packets.Modules.Formatted;
using Fluffy.Net.Packets.Raw;

namespace Fluffy.Net
{
    public class ConnectionInfo : IDisposable
    {
        public EventHandler<ConnectionInfo> OnDisposing;
        public Socket Socket { get; set; }

        public FluffySocket FluffySocket { get; set; }

        internal Receiver Receiver { get; private set; }
        internal Sender Sender { get; private set; }
        public RawPacketHandler PacketHandler { get; private set; }
        public TypedPacketHandler TypedPacketHandler { get; private set; }

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
            PacketHandler = new RawPacketHandler(this);
            TypedPacketHandler = new TypedPacketHandler(this);

            Receiver.OnReceive += PacketHandler.Handle;
#if DEBUG

            PacketHandler.RegisterPacket<DummyPacket>();
#endif
            PacketHandler.RegisterPacket<FormattedPacket>();

            TypedPacketHandler
                .On<MyAwesomeClass>().Do(Awesome)
                .On<ConnectionInfo>().Do(x => Console.Write($"You are awesome :3"))
                .Default(() => Console.Write($"Lol, Default"));
        }

        private Stopwatch _sw;

        private MyAwesomeClass Awesome(MyAwesomeClass awesome)
        {
            if (_sw == null)
            {
                _sw = Stopwatch.StartNew();
            }

            if (awesome.Packets % 300 == 0)
            {
                Console.WriteLine($"{awesome.Packets}:  ({awesome.Packets / _sw.Elapsed.TotalMilliseconds})");
                //  await Task.Delay(TimeSpan.FromSeconds(0.1));
            }

            awesome.Packets++;
            return awesome;
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