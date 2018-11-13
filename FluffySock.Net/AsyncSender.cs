using Fluffy.IO.Buffer;
using Fluffy.IO.Recycling;

using System;
using System.Collections.Concurrent;
using System.Data;
using System.IO;
using System.Net.Sockets;

namespace Fluffy.Net
{
    internal class AsyncSender : IDisposable
    {
        private readonly Socket _socket;
        private static SharedOutputQueueWorker _queueWorker;
        internal bool IsDisposed { get; private set; }

        private readonly IObjectRecyclingFactory<LinkableBufferObject<byte>> _bufferFactory;
        private LinkableBufferObject<byte> _bufferWrapper;
        private byte[] _buffer;

        private Stream _stream;
        private ConcurrentQueue<Stream> _streamQueue;

        private volatile bool _sendingInProgress;
        private IAsyncResult _currentAsyncResult;

        static AsyncSender()
        {
            _queueWorker = new SharedOutputQueueWorker();
        }

        public AsyncSender(Socket socket)
        {
            _streamQueue = new ConcurrentQueue<Stream>();
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

        public void Send(Stream stream)
        {
            _streamQueue.Enqueue(stream);
        }

        internal bool DoWork(bool ignoreProgressLock = false)
        {
            if (_sendingInProgress && !ignoreProgressLock)
            {
                return false;
            }

            if (_stream == null || _stream.Length == 0)
            {
                _stream?.Dispose();
                if (_streamQueue.TryDequeue(out _stream))
                {
                    return DoWork();
                }
            }

            var read = _stream.Read(_buffer, 0, _buffer.Length);
            if (read <= 0)
            {
                return false;
            }

            _sendingInProgress = true;
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
                _socket?.EndSend(_currentAsyncResult);
            }
            catch (Exception)
            {
                //Ignore
            }
            IsDisposed = true;
            _bufferFactory.Recycle(_bufferWrapper);

            while (_streamQueue.TryDequeue(out _stream))
            {
                _stream.Dispose();
            }
        }
    }
}