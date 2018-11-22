using Fluffy.IO.Buffer;

using System;
using System.Linq;
using System.Security.Cryptography;

namespace Fluffy.Net.Packets.Modules.Raw
{
    public class StreamPacketHandler : BasePacketHandler
    {
        private HashAlgorithm _ha;
        private byte[] _buffer;

        public StreamPacketHandler() : base()
        {
            _ha = MD5.Create();
            _buffer = new byte[4096];
            //ha.tra
        }

        public override byte OpCode => (int)PacketTypes.StreamPacket;

        public override void Handle(LinkedStream stream)
        {
            var guidBuffer = new byte[16];
            var isEOF = stream.ReadByte();
            var read = stream.Read(guidBuffer, 0, guidBuffer.Length);
            if (read == guidBuffer.Length)
            {
                //Console.WriteLine($"Received stream packet. " +
                //                  $"Guid: {new Guid(guidBuffer)} " +
                //                  $"Length: {stream.Length}");
                while ((read = stream.Read(_buffer, 0, _buffer.Length)) != 0)
                {
                    _ha.TransformBlock(_buffer, 0, read, null, 0);
                }
                stream.Dispose();
                if (isEOF == 1)
                {
                    _ha.TransformFinalBlock(_buffer, 0, 0);
                    var stringHash = string.Concat(_ha.Hash.Select(x => x.ToString("x2")));

                    Console.WriteLine($"Hash is {stringHash}");
                    // Debugger.Break();
                }
            }
        }
    }
}