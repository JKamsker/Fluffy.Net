using Fluffy.Net.Options;
using Fluffy.Unsafe;

using System;
using System.IO;

namespace Fluffy.Net.Packets.Modules
{
    internal class StreamOutputPacket : IOutputPacket
    {
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

            if (realCount <= 0)
            {
                return 0;
            }

            if (count > streamLength)
            {
                count = streamLength;
            }

            if (_stream?.Length < 0)
            {
                throw new AggregateException("Stream length cannot be less than 0");
            }

            if (_stream == null || streamLength == 0)
            {
                HasFinished = true;
                Console.WriteLine($"Disposed");
                Dispose();
                return 0;
            }

            int read = 4 + 2 + 16; //1x sizeof(int) - 2x sizeof(byte) - 1x sizeof(Guid)
            //var blockLength = count - offset - 4; //ex buffer.Length

            //Length 4 Byte
            //DynamicMethodDummy 1 Byte
            //ParallelismOptions 1 Byte
            FluffyBitConverter.Serialize(count - 4, buffer, offset);
            offset += 4;
            buffer[offset++] = (byte)ParallelismOptions;
            buffer[offset++] = OpCode;
            //Injected body
            FluffyBitConverter.Serialize(_streamId, buffer, offset);
            offset += 16;

            read += _stream.Read(buffer, offset, count - offset);
            if (_stream == null || _stream.Length - _stream.Position == 0)
            {
                HasFinished = true;
                Console.WriteLine($"Disposed");
                Dispose();
            }

            //if (read != buffer.Length)
            //{
            //    Debugger.Break();
            //}
            return read;
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}