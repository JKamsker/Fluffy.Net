using Fluffy.IO.Buffer;
using Fluffy.IO.Recycling;

using System;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Fluffy.Net
{
    internal class AsyncSender : IDisposable
    {
        private readonly Socket _socket;
        private static SharedOutputQueueWorker _queueWorker;
        internal bool IsDisposed { get; set; }

        private readonly IObjectRecyclingFactory<LinkableBufferObject<byte>> _bufferFactory;
        private LinkableBufferObject<byte> _bufferWrapper;
        private byte[] _buffer;

        private LinkedStream _stream;
        private ConcurrentQueue<LinkedStream> _streamQueue;

        private volatile bool _sendingInProgress;
        private IAsyncResult _currentAsyncResult;

        static AsyncSender()
        {
            _queueWorker = new SharedOutputQueueWorker();
        }

        public AsyncSender(Socket socket)
        {
            _streamQueue = new ConcurrentQueue<LinkedStream>();
            _socket = socket;

            _bufferFactory = BufferRecyclingMetaFactory.Get(Capacity.Medium);
            _bufferWrapper = _bufferFactory.Get();

            if (!_queueWorker.Running)
            {
                _queueWorker.StartWorker();
            }
            _queueWorker.Add(this);
        }

        public void Send(LinkedStream stream)
        {
            _streamQueue.Enqueue(stream);
        }

        internal void DoWork(bool ignoreProgressLock = false)
        {
            if (_sendingInProgress && !ignoreProgressLock)
            {
                return;
            }

            if (_stream == null || _stream.Length == 0)
            {
                if (_streamQueue.TryDequeue(out _stream))
                {
                    DoWork();
                    return;
                }
            }

            var read = _stream.Read(_buffer, 0, _buffer.Length);
            if (read <= 0)
            {
                return;
            }

            _sendingInProgress = true;
            _currentAsyncResult = _socket.BeginSend(_buffer, 0, read, SocketFlags.None, Callback, this);
        }

        private void Callback(IAsyncResult ar)
        {
            _currentAsyncResult = null;
            _ = _socket.EndSend(ar);
            _sendingInProgress = false;
        }

        public void Dispose()
        {
            try
            {
                _socket?.EndSend(_currentAsyncResult);
            }
            catch (Exception)
            {
                //Ignore
            }
            IsDisposed = true;
            _bufferFactory.Recycle(_bufferWrapper);
        }
    }
}