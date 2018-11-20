using Fluffy.IO.Buffer;

using System;

namespace Fluffy.Net.Packets.Modules.Raw
{
    public class StreamPacketHandler : BasePacketHandler
    {
        public override byte OpCode => (int)PacketTypes.StreamPacket;

        public override void Handle(LinkedStream stream)
        {
            throw new NotImplementedException();
        }
    }
}