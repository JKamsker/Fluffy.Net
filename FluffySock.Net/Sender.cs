using System;
using Fluffy.IO.Buffer;
using Fluffy.IO.Recycling;

namespace Fluffy.Net
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

        public void Send(DynamicMethodDummy opcode, ParallelismOptions parallelismOption, LinkedStream stream)
        {
            //Length 4 Byte
            //DynamicMethodDummy 1 Byte
            //ParallelismOptions 1 Byte

            var lengthBytes = BitConverter.GetBytes(stream.Length + 2);
            var metadata = new byte[4 + 1 + 1];
            Array.Copy(lengthBytes, metadata, 4);
            metadata[4] = (byte)parallelismOption;
            metadata[5] = (byte)opcode;
            stream.WriteHead(metadata, 0, metadata.Length);
            _asyncSender.Send(stream);
            //TODO:Send
        }

        public void Dispose()
        {
            _asyncSender.Dispose();
        }
    }
}