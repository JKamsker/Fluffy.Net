using Fluffy.IO.Buffer;
using Fluffy.IO.Recycling;
using Fluffy.Net.Async;
using Fluffy.Net.Options;

using System;

namespace Fluffy.Net.Packets
{
    internal class Sender : IDisposable
    {
        private readonly ConnectionInfo _connection;
        private AsyncSender _asyncSender;
        private FluffyBuffer _buffer;

        public Sender(ConnectionInfo connection)
        {
            _connection = connection;
            _buffer = BufferRecyclingMetaFactory<FluffyBuffer>.MakeFactory(Capacity.Medium).GetBuffer();
            _asyncSender = new AsyncSender(_connection.Socket, _connection.FluffySocket.QueueWorker);
        }

        public void Send(DynamicMethodDummy opcode, LinkedStream stream, ParallelismOptions parallelismOption = ParallelismOptions.Parallel)
        {
            //Length 4 Byte
            //DynamicMethodDummy 1 Byte
            //ParallelismOptions 1 Byte

            var lengthBytes = BitConverter.GetBytes(stream.Length + 2);

            var metadata = new byte[4 + 1 + 1];
            Array.Copy(lengthBytes, metadata, 4);
            metadata[4] = (byte)parallelismOption;
            metadata[5] = (byte)opcode;

            var fake = new FakeLinkableBuffer(metadata);
            stream.EnqueueHead(fake);
            _asyncSender.Send(stream);
        }

        public void Dispose()
        {
            _asyncSender.Dispose();
        }
    }
}