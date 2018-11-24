using Fluffy.IO.Buffer;

namespace Fluffy.Net.Packets.Modules.Raw
{
    public class StreamPacketHandler : BasePacketHandler
    {
        private byte[] _buffer;

        public StreamPacketHandler() : base()
        {
            _buffer = new byte[4096];
        }

        public override byte OpCode => (int)PacketTypes.StreamPacket;

        public override void Handle(LinkedStream stream)
        {
            var guidBuffer = new byte[16];
            var isEOF = stream.ReadByte();
            var read = stream.Read(guidBuffer, 0, guidBuffer.Length);
            if (read == guidBuffer.Length)
            {
                while ((read = stream.Read(_buffer, 0, _buffer.Length)) != 0)
                {
                }
                stream.Dispose();
                if (isEOF == 1)
                {
                }
            }
        }
    }
}