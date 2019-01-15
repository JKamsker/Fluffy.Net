using Fluffy.IO.Buffer;
using Fluffy.IO.Extensions;
using Fluffy.IO.Recycling;
using Fluffy.Net.Collections;
using Fluffy.Net.Options;

using System;
using System.Net.Sockets;

namespace Fluffy.Net.Packets.Modules
{
    internal class Receiver : IDisposable
    {
        private readonly Socket _socket;
        private byte[] _buffer;
        private IOState _state;
        private LinkedStream _stream;
        private int _nextSegmentLength;

        private volatile bool _started;
        private volatile bool _disposed;

        private IObjectRecyclingFactory<LinkableBuffer> _bufferWrapperFac;
        private LinkableBuffer _bufferWrapper;

        public EventHandler<OnPacketReceiveEventArgs> OnReceive;
        public EventHandler<Receiver> OnDisposing;

        internal Receiver(Socket socket)
        {
            _socket = socket;

            _bufferWrapperFac = BufferRecyclingMetaFactory.Get(Capacity.Medium);
            _bufferWrapper = _bufferWrapperFac.GetBuffer();
            _buffer = _bufferWrapper.Value;

            _stream = new LinkedStream();
        }

        public Receiver Start()
        {
            if (_started)
            {
                throw new AggregateException($"Handler already started");
            }

            _started = true;
            _socket.BeginReceive(_buffer, 0, 4, SocketFlags.None, ReceiveCallback, null);
            return this;
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int bytesRead = _socket.EndReceive(ar);
                if (bytesRead == 0)
                {
                    Console.WriteLine($"Received 0 bytes, closing Socket!");
                    return;
                }

                _stream.Write(_buffer, 0, bytesRead);
                while (_stream.Length >= _nextSegmentLength)
                {
                    HandleStream();
                }

                _socket.BeginReceive(_buffer, 0, (int)(_nextSegmentLength - _stream.Length), SocketFlags.None, ReceiveCallback, null);
            }
            catch (SocketException e) when (e.ErrorCode == 10054)
            {
                //Connection was closed
                Dispose();
            }
        }

        private int headerLen = 0;

        private void HandleStream()
        {
            switch (_state)
            {
                case IOState.HeaderLen:
                    if (_stream.Length >= 4)
                    {
                        headerLen++;

                        var nextSegmentLength = _stream.ReadInt32();
                        if (nextSegmentLength == -1)
                        {
                            break;
                        }
                        else if (nextSegmentLength <= 0)
                        {
                            throw new AggregateException("Stream out of scope!");
                        }
                        _nextSegmentLength = nextSegmentLength;
                        _state = IOState.BodyBytes;
                    }
                    break;

                case IOState.BodyBytes:
                    if (_stream.Length >= _nextSegmentLength)
                    {
                        var options = (ParallelismOptions)_stream.ReadByte();
                        var opCode = (byte)_stream.ReadByte();
                        var body = _stream.ReadToLinkedStream(_nextSegmentLength - 2);

                        OnReceive?.Invoke(this, new OnPacketReceiveEventArgs(opCode, options, body));
                    }

                    _nextSegmentLength = 4;
                    _state = IOState.HeaderLen;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

#if DEBUG

        [Obsolete("Only for testing")]
        internal void OnReceiveHandler(byte opCode, ParallelismOptions options, LinkedStream body)
        {
            OnReceive?.Invoke(this, new OnPacketReceiveEventArgs(opCode, options, body));
        }

#endif

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            OnDisposing?.Invoke(this, this);
            _socket?.Dispose();
            _stream?.Dispose();
            _bufferWrapperFac.Recycle(_bufferWrapper);
        }
    }

    // ReSharper disable once InconsistentNaming
}