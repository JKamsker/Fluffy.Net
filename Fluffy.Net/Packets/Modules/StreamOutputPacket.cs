using Fluffy.Net.Options;
using Fluffy.Unsafe;

using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace Fluffy.Net.Packets.Modules
{


    internal class StreamOutputPacket : IOutputPacket
    {
        private const int PacketHeaderLengthSize = 4;
        private const int ParallelismOptionsSize = 1;
        private const int OpCodeSize = 1;
        private const int EndOfFileSize = 1;
        private const int GuidSize = 16;
        private const int FirstLevelHeader = PacketHeaderLengthSize;
        private const int SecondLevelHeader = ParallelismOptionsSize + OpCodeSize;
        private const int ThirdLevelHeader = EndOfFileSize + GuidSize;

        private static readonly int HeaderSize = PacketHeader.Size + StreamPacketHeader.Size;

        private Guid _streamId;
        private Stream _stream;

        public bool IsPrioritized => false;
        public bool CanBreak => true;
        public bool HasFinished { get; private set; }
        public byte OpCode { get; }
        public ParallelismOptions ParallelismOptions { get; }

        public StreamOutputPacket(byte opCode, ParallelismOptions parallelismOption, Guid streamId, Stream stream)
        {
            OpCode = opCode;
            ParallelismOptions = parallelismOption;
            _streamId = streamId;
            _stream = stream;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            var realCount = count;
            var maxCount = buffer.Length - offset;

            realCount = Math.Min(realCount, maxCount);

            if (realCount - PacketHeader.Size - 128 <= 0)
            {
                return -1;
            }

            if ((_stream.Length - _stream.Position) < 0)
            {
                throw new AggregateException("Stream length cannot be less than 0");
            }

            // int read = Header; //1x sizeof(int) - 3x sizeof(byte) - 1x sizeof(Guid) = 22
            //var blockLength = count - offset - 4; //ex buffer.Length

            //var bodyRead = _stream.Read(buffer, offset + Header, realCount - Header);


            var bodyRead = _stream.Read(buffer, offset + HeaderSize, realCount - HeaderSize);
            if (_stream == null || _stream.Length - _stream.Position == 0)
            {
                HasFinished = true;
#if DEBUG
                Console.WriteLine($"[StreamOutputPacket][Read] Disposed");
#endif

                Dispose();
            }

            if (bodyRead == 0)
            {
                return 0;
            }

            var packetHeader = new PacketHeader
            {
                PacketLength = bodyRead + SecondLevelHeader + ThirdLevelHeader,
                ParallelismOptions = (byte)ParallelismOptions,
                OpCode = this.OpCode,

            };

            var streamPacketHeader = new StreamPacketHeader
            {
                HasFinished = (byte)(HasFinished ? 1 : 0),
                StreamId = _streamId
            };

            offset += FluffyBitConverter.Serialize(packetHeader, buffer, offset);
            offset += FluffyBitConverter.Serialize(streamPacketHeader, buffer, offset);

            return bodyRead + offset;
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}
