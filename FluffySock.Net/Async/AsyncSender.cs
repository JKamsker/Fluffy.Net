using Fluffy.IO.Buffer;
using Fluffy.IO.Recycling;
using Fluffy.Net.Packets.Modules;

using System;
using System.Collections.Concurrent;
using System.Data;
using System.IO;
using System.Net.Sockets;

namespace Fluffy.Net.Async
{
    internal class AsyncSender : IDisposable
    {
        private readonly Socket _socket;

        private readonly IObjectRecyclingFactory<RecyclableBuffer> _bufferFactory;
        private RecyclableBuffer _bufferWrapper;
        private byte[] _buffer;

        private Stream _stream;
        private ConcurrentQueue<Stream> _streamQueue;

        private IOutputPacket _packet;
        private ConcurrentQueue<IOutputPacket> _packetQueue;

        private volatile bool _sendingInProgress;
        private IAsyncResult _currentAsyncResult;

        public SendTaskRelay SendTaskRelay { get; }

        public AsyncSender(Socket socket, SharedOutputQueueWorker worker)
        {
            _streamQueue = new ConcurrentQueue<Stream>();
            _packetQueue = new ConcurrentQueue<IOutputPacket>();
            _socket = socket;

            SendTaskRelay = new SendTaskRelay(() => DoWork());
            _bufferFactory = BufferRecyclingMetaFactory<RecyclableBuffer>.MakeFactory(Capacity.Medium);
            _bufferWrapper = _bufferFactory.GetBuffer();
            _buffer = _bufferWrapper.Value;

            if (!worker.Running)
            {
                worker.StartWorker();
            }
            worker.AddTask(SendTaskRelay);
        }

        public void Send(IOutputPacket packet)
        {
            _packetQueue.Enqueue(packet);
            SendTaskRelay.WaitHandle.Set();
        }

        internal bool DoWork(bool ignoreProgressLock = false)
        {
            if (_sendingInProgress && !ignoreProgressLock)
            {
                return false;
            }
            _sendingInProgress = true;

            if (_packet == null || _packet.IsFinished)
            {
                _packet?.Dispose();
                if (_packetQueue.TryDequeue(out _packet))
                {
                    return DoWork(true);
                }
                else
                {
                    _sendingInProgress = false;
                    return false;
                }
            }

            var read = _packet.Read(_buffer, 0, _buffer.Length);
            if (read <= 0)
            {
                return DoWork(true);
            }

            _currentAsyncResult = _socket.BeginSend(_buffer, 0, read, SocketFlags.None, ar => Callback(ar, read), this);
            return true;
        }

        private void Callback(IAsyncResult ar, int messageSize)
        {
            _currentAsyncResult = null;

            if (_socket.EndSend(ar) != messageSize)
            {
                throw new DataException("Message size does not match the amount of bytes sent");
            }

            if (!DoWork(true))
            {
                _sendingInProgress = false;
            }
        }

        public void Dispose()
        {
            try
            {
                if (_currentAsyncResult != null)
                {
                    _socket?.EndSend(_currentAsyncResult);
                }
            }
            catch (Exception)
            {
                //Ignore
            }

            _bufferWrapper.Recycle();

            while (_streamQueue.TryDequeue(out _stream))
            {
                _stream.Dispose();
            }

            SendTaskRelay.Dispose();
        }
    }
}