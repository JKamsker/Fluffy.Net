using System;
using System.Net.Sockets;
using Fluffy.IO.Buffer;
using Fluffy.IO.Recycling;

namespace Fluffy.Net
{
    public class ConnectionInfo : IDisposable
    {
        public EventHandler<ConnectionInfo> OnDisposing;
        public Socket Socket { get; set; }

        public FluffySocket FluffySocket { get; set; }

        internal Receiver Receiver { get; private set; }
        internal Sender Sender { get; private set; }

        public ConnectionInfo(Socket socket, FluffySocket fluffySocket)
        {
            Socket = socket;
            FluffySocket = fluffySocket;

            Receiver = new Receiver(socket).Start();
            Sender = new Sender(this);
        }

        public void Dispose()
        {
            Socket?.Dispose();
            OnDisposing?.Invoke(this, this);
        }
    }

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

    internal class SendTaskRelay : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public Func<bool> WorkFunc { get; }

        public SendTaskRelay(Func<bool> workFunc)
        {
            WorkFunc = workFunc;
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}