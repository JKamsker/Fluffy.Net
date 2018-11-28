using Fluffy.Extensions;
using Fluffy.IO.Buffer;
using Fluffy.Net.Async;
using Fluffy.Net.Options;
using Fluffy.Net.Packets.Modules.Formatted;
using Fluffy.Net.Packets.Modules.Raw;

using System;
using System.IO;

namespace Fluffy.Net.Packets.Modules
{
    public class Sender : IDisposable
    {
        private readonly ConnectionInfo _connection;
        private AsyncSender _asyncSender;
        private DuplexSender _duplexSender;

        public Sender(ConnectionInfo connection)
        {
            _connection = connection;
            _asyncSender = new AsyncSender(_connection.Socket, _connection.FluffySocket.QueueWorker);
            _duplexSender = new DuplexSender(_connection);
        }

        public void Send(object value)
        {
            var stream = new LinkedStream();
            value.Serialize(stream);
            Send(PacketTypes.FormattedPacket, stream);
        }

        /// <summary>
        /// TODO: Return result? Maybe return intermediate object which can return the result (may
        /// cause locking of thread) or a task<TResult> </summary> <typeparam
        /// name="TResult"></typeparam> <param name="value"></param> <returns></returns>
        public IPacketResult<TResult> Send<TResult>(object value)
        {
            return _duplexSender.Send<TResult>(value);
        }

        internal void Send<T>(T opCode, LinkedStream stream,
            ParallelismOptions parallelismOption = ParallelismOptions.Parallel)
        where T : Enum
        {
            var bOpCode = Convert.ToByte(opCode);
            Send(bOpCode, stream, parallelismOption);
        }

        internal void Send(byte opCode, LinkedStream stream,
            ParallelismOptions parallelismOption = ParallelismOptions.Parallel)
        {
            _asyncSender.Send(new StandardOutputPacket(opCode, parallelismOption, stream)
            {
                IsPrioritized = true
            });
        }

        public void SendStream(Guid streamId, Stream stream)
        {
            _asyncSender.Send(new StreamOutputPacket((byte)PacketTypes.StreamPacket, ParallelismOptions.Sync, streamId, stream));
        }

        public void Dispose()
        {
            _asyncSender.Dispose();
        }
    }
}