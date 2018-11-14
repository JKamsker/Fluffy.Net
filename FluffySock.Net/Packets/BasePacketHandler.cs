using Fluffy.IO.Buffer;

using System;

namespace Fluffy.Net.Packets
{
    public abstract class BasePacket
    {
        public abstract byte OpCode { get; }

        internal ConnectionInfo Connection { get; set; }

        public abstract void Handle(LinkedStream stream);
    }

    public class DummyPacket : BasePacket
    {
        public override byte OpCode => 1;

        public override void Handle(LinkedStream stream)
        {
            Console.WriteLine($"Heya :P");
        }
    }
}