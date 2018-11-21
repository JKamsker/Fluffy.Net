using Fluffy.IO.Buffer;

using System;

namespace Fluffy.Net.Packets.Modules.Raw
{
    public class StreamPacketHandler : BasePacketHandler
    {
        public override byte OpCode => (int)PacketTypes.StreamPacket;

        public override void Handle(LinkedStream stream)
        {
            var guidBuffer = new byte[16];
            var read = stream.Read(guidBuffer, 0, guidBuffer.Length);
            if (read == guidBuffer.Length)
            {
                Console.WriteLine($"Received stream packet. " +
                                  $"Guid: {new Guid(guidBuffer)} " +
                                  $"Length: {stream.Length}");
                stream.Dispose();
            }
        }
    }
}