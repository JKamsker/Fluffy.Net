using Fluffy.Net.Options;
using Fluffy.Unsafe;

using System;
using System.IO;

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
        private const int Header = FirstLevelHeader + SecondLevelHeader + ThirdLevelHeader;

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
            var streamLength = (int)(_stream.Length - _stream.Position);

            realCount = Math.Min(realCount, maxCount);

            if (realCount - Header - 128 <= 0)
            {
                return -1;
            }

            if (streamLength < 0)
            {
                throw new AggregateException("Stream length cannot be less than 0");
            }

            int read = Header; //1x sizeof(int) - 3x sizeof(byte) - 1x sizeof(Guid) = 22
                               //var blockLength = count - offset - 4; //ex buffer.Length

            var bodyRead = _stream.Read(buffer, offset + Header, realCount - Header);
            if (_stream == null || _stream.Length - _stream.Position == 0)
            {
                HasFinished = true;
                Console.WriteLine($"Disposed");
                Dispose();
            }

            if (bodyRead == 0)
            {
                return 0;
            }

            //Length 4 Byte
            //DynamicMethodDummy 1 Byte
            //ParallelismOptions 1 Byte
            FluffyBitConverter.Serialize(bodyRead + SecondLevelHeader + ThirdLevelHeader, buffer, offset);
            offset += 4;
            buffer[offset++] = (byte)ParallelismOptions;
            buffer[offset++] = OpCode;
            buffer[offset++] = (byte)(HasFinished ? 1 : 0);
            //Injected body
            FluffyBitConverter.Serialize(_streamId, buffer, offset);

            return bodyRead + Header;
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}