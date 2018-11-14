using Fluffy.IO.Buffer;

using System;
using System.IO;

namespace Fluffy.Net.Packets
{
    public enum Packet : byte
    {
        DummyPacket = 1,
        XaXa
    }

    public abstract class BasePacket
    {
        public abstract byte OpCode { get; }

        internal ConnectionInfo Connection { get; set; }

        public abstract void Handle(LinkedStream stream);
    }

    public class DummyPacket : BasePacket
    {
        public override byte OpCode => (int)Packet.DummyPacket;

        public override void Handle(LinkedStream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                Console.WriteLine(sr.ReadToEnd());
            }
            //Console.WriteLine($"Heya :P");
        }
    }
}