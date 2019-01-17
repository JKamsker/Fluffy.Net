using Fluffy.IO.Buffer;
using Fluffy.IO.Recycling;
using Fluffy.Net.Packets.Modules;

using System;
using System.Collections.Concurrent;
using System.Data;
using System.Net.Sockets;

namespace Fluffy.Net.Async
{
    internal class AsyncSender : IDisposable
    {
        private readonly Socket _socket;

        private readonly IObjectRecyclingFactory<FluffyBuffer> _bufferFactory;
        private FluffyBuffer _bufferWrapper;
        private byte[] _buffer;

        private IOutputPacket _packet;
        private IOutputPacket _unprioritizedPacket;

        private ConcurrentQueue<IOutputPacket> _packetQueue;
        private ConcurrentQueue<IOutputPacket> _priorityPacketQueue;

        private volatile bool _sendingInProgress;
        private IAsyncResult _currentAsyncResult;

        public SendTaskRelay SendTaskRelay { get; }

        public AsyncSender(Socket socket, SharedOutputQueueWorker worker)
        {
            _packetQueue = new ConcurrentQueue<IOutputPacket>();
            _priorityPacketQueue = new ConcurrentQueue<IOutputPacket>();

            _socket = socket;

            SendTaskRelay = new SendTaskRelay(() => DoWork());
            _bufferFactory = BufferRecyclingMetaFactory<FluffyBuffer>.MakeFactory(Capacity.Medium);
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
            if (packet.IsPrioritized)
            {
                _priorityPacketQueue.Enqueue(packet);
            }
            else
            {
                _packetQueue.Enqueue(packet);
            }
            SendTaskRelay.WaitHandle.Set();
        }

        internal bool DoWork(bool ignoreProgressLock = false, int offset = 0)
        {
            if (_sendingInProgress && !ignoreProgressLock)
            {
                return false;
            }

            _sendingInProgress = true;

            EvaluatePacket();

            if (_packet == null)
            {
                if (offset == 0)
                {
                    _sendingInProgress = false;
                    return false;
                }
            }
            else
            {
                var read = _packet.Read(_buffer, offset, _buffer.Length);
                if (read != -1)
                {
                    offset += read;
                    if (offset <= _buffer.Length - 128)
                    {
                        return DoWork(true, offset);
                    }
                }
            }

            //var cperc = offset * 100 / _buffer.Length;
            //avg = ((avg * total) + cperc) / ++total;
            //Console.WriteLine($"Buffer Filled avg {avg} - {offset * 100 / _buffer.Length}%");
            _currentAsyncResult = _socket.BeginSend(_buffer, 0, offset, SocketFlags.None, ar => Callback(ar, offset), this);
            return true;
        }

        private void EvaluatePacket()
        {
            if (_packet?.HasFinished == true)
            {
                _packet.Dispose();
                _packet = null;
            }

            if (_packet != null)
            {
                if (!_packet.IsPrioritized && !_priorityPacketQueue.IsEmpty)
                {
                    _unprioritizedPacket = _packet;
                    _packet = null;
                }
            }

            if (_packet == null)
            {
                if (!_priorityPacketQueue.IsEmpty)
                {
                    if (!_priorityPacketQueue.TryDequeue(out _packet))
                    {
                        throw new AggregateException("Invalid state: priority queue is empty");
                    }
                }
                else
                {
                    if (_unprioritizedPacket != null)
                    {
                        _packet = _unprioritizedPacket;
                        _unprioritizedPacket = null;
                    }
                    else if (!_packetQueue.IsEmpty)
                    {
                        if (!_packetQueue.TryDequeue(out _packet))
                        {
                            throw new AggregateException("Invalid state: priority queue is empty");
                        }
                    }
                }
            }
        }

        private void Callback(IAsyncResult ar, int messageSize)
        {
            _currentAsyncResult = null;

            if (_socket.EndSend(ar, out var socketError) != messageSize && socketError == SocketError.Success)
            {
                throw new DataException("Message size does not match the amount of bytes sent");
            }

            if (socketError != SocketError.Success)
            {
                Console.WriteLine($"SocketError: {socketError}");
                Dispose();
                return;
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
                    _socket?.EndSend(_currentAsyncResult, out _);
                }
            }
            catch (Exception)
            {
                //Ignore
            }

            try
            {
                _socket?.Dispose();
            }
            catch (Exception e)
            {
                //Ignore
            }

            _bufferWrapper.Dispose();

            while (_packetQueue.TryDequeue(out _packet))
            {
                _packet.Dispose();
            }

            while (_priorityPacketQueue.TryDequeue(out _packet))
            {
                _packet.Dispose();
            }

            SendTaskRelay.Dispose();
        }
    }
}