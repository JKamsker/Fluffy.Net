using Fluffy.IO.Buffer;

namespace Fluffy.Net.Packets.Raw
{
    public abstract class BasePacket
    {
        public abstract byte OpCode { get; }

        internal ConnectionInfo Connection { get; set; }

        public abstract void Handle(LinkedStream stream);
    }
}