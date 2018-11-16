using System;
using System.IO;
using Fluffy.IO.Buffer;

namespace Fluffy.Net.Packets.Raw
{
    public class DummyPacket : BasePacket
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