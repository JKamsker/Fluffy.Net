using Fluffy.IO.Buffer;
using Fluffy.IO.Recycling;

using System;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;

namespace Fluffy.Net
{
    internal class AsyncSender : IDisposable
    {
        private readonly Socket _socket;

        private readonly IObjectRecyclingFactory<RecyclableBuffer> _bufferFactory;
        private RecyclableBuffer _bufferWrapper;
        private byte[] _buffer;

        private Stream _stream;
        private ConcurrentQueue<Stream> _streamQueue;

        private volatile bool _sendingInProgress;
        private IAsyncResult _currentAsyncResult;

        public SendTaskRelay SendTaskRelay { get; }

        public AsyncSender(Socket socket, SharedOutputQueueWorker worker)
        {
            _streamQueue = new ConcurrentQueue<Stream>();
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

            if (_stream?.Length < 0)
            {
                Debugger.Break();
            }

            if (_stream == null || _stream.Length <= 0)
            {
                _stream?.Dispose();
                if (_streamQueue.TryDequeue(out _stream))
                {
                    return DoWork(true);
                }
                else
                {
                    return false;
                }
            }

            var read = _stream.Read(_buffer, 0, _buffer.Length);
            if (read <= 0)
            {
                return DoWork(true);
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