using Fluffy.IO.Buffer;

using System;
using System.IO;

namespace Fluffy.Net.Packets.Modules.Raw
{
    public class DummyPacketHandler : BasePacketHandler
    {
        public override byte OpCode => (int)PacketTypes.TestPacket;

        public override void Handle(LinkedStream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                Console.WriteLine(sr.ReadToEnd());
            }
        }
    }
}