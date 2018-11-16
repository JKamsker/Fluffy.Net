using Fluffy.Extensions;
using Fluffy.IO.Buffer;
using Fluffy.Net.Async;
using Fluffy.Net.Options;

using System;
using Fluffy.Net.Packets.Raw;

namespace Fluffy.Net.Packets
{
    internal class Sender : IDisposable
    {
        private readonly ConnectionInfo _connection;
        private AsyncSender _asyncSender;

        public Sender(ConnectionInfo connection)
        {
            _connection = connection;
            _asyncSender = new AsyncSender(_connection.Socket, _connection.FluffySocket.QueueWorker);
        }

        public void Send(object value)
        {
            var stream = new LinkedStream();
            value.Serialize(stream);
            Send(PacketTypes.FormattedPacket, stream);
        }

        /// <summary>
        /// TODO: Return result?
        /// Maybe return intermediate object which can return the result (may cause locking of thread) or a task<TResult>
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public TResult Send<TResult>(object value)
        {
            return default;
        }

        public void Send<T>(T opCode, LinkedStream stream,
            ParallelismOptions parallelismOption = ParallelismOptions.Parallel)
        where T : Enum
        {
            var bOpCode = Convert.ToByte(opCode);
            Send(bOpCode, stream, parallelismOption);
        }

        public void Send(byte opCode, LinkedStream stream,
            ParallelismOptions parallelismOption = ParallelismOptions.Parallel)
        {
            //Length 4 Byte
            //DynamicMethodDummy 1 Byte
            //ParallelismOptions 1 Byte

            var lengthBytes = BitConverter.GetBytes(stream.Length + 2);

            var metaData = new byte[4 + 1 + 1];
            Array.Copy(lengthBytes, metaData, 4);
            metaData[4] = (byte)parallelismOption;
            metaData[5] = (byte)opCode;

            var fake = new FakeLinkableBuffer(metaData);
            stream.EnqueueHead(fake);
            _asyncSender.Send(stream);
        }

        public void Dispose()
        {
            _asyncSender.Dispose();
        }
    }
}