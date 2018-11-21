using Fluffy.IO.Buffer;
using Fluffy.Net.Options;
using Fluffy.Unsafe;

using System;

namespace Fluffy.Net.Packets.Modules
{
    internal class StreamOutputPacket
        : IOutputPacket
    {
        private readonly LinkedStream _stream;
        private bool _isDisposed;

        public byte OpCode { get; set; }
        public ParallelismOptions ParallelismOptions { get; set; }

        /// <summary>
        /// Is True when send progress has finished
        /// </summary>
        public bool HasFinished { get; private set; }

        public bool IsPrioritized { get; set; }

        /// <summary>
        /// Defines wether the handler can switch to more important Packets safely
        /// </summary>
        public bool CanBreak { get; private set; }

        public bool HasSendHeaders { get; private set; }

        public StreamOutputPacket(byte opCode, ParallelismOptions parallelismOption, LinkedStream stream)
        {
            OpCode = opCode;
            ParallelismOptions = parallelismOption;
            _stream = stream;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            if (count > buffer.Length)
            {
                count = buffer.Length;
            }

            if (_stream?.Length < 0)
            {
                throw new AggregateException("Stream length cannot be less than 0");
            }

            if (_stream == null || _stream.Length == 0)
            {
                HasFinished = true;
                CanBreak = true;
                Dispose();
                return 0;
            }

            int read = 0;
            if (!HasSendHeaders)
            {
                //Length 4 Byte
                //DynamicMethodDummy 1 Byte
                //ParallelismOptions 1 Byte

                FluffyBitConverter.GetBytes(_stream.Length + 2, buffer, offset);

                //var lengthBytes = BitConverter.GetBytes(_stream.Length + 2);
                //Array.Copy(lengthBytes, 0, buffer, offset, 4);
                offset += 4;
                buffer[offset++] = (byte)ParallelismOptions;
                buffer[offset++] = OpCode;
                read = 6;
                HasSendHeaders = true;
            }

            read += _stream.Read(buffer, offset, count);
            if (_stream == null || _stream.Length == 0)
            {
                HasFinished = true;
                CanBreak = true;
                Dispose();
            }

            return read;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;

            _stream?.Dispose();
        }
    }
}