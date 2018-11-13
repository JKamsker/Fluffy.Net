using System;
using System.Net.Sockets;
using Fluffy.IO.Buffer;
using Fluffy.IO.Extensions;
using Fluffy.IO.Recycling;

namespace Fluffy.Net
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

        private ObjectStorage<DynamicMethodDummy, Action<LinkedStream>> _actionStorage;
        private IObjectRecyclingFactory<LinkableBuffer> _bufferWrapperFac;
        private LinkableBuffer _bufferWrapper;

        internal Receiver(Socket socket)
        {
            _socket = socket;

            _bufferWrapperFac = BufferRecyclingMetaFactory.Get(Capacity.Medium);
            _bufferWrapper = _bufferWrapperFac.GetBuffer();
            _buffer = _bufferWrapper.Value;

            _stream = new LinkedStream();
            _actionStorage = new ObjectStorage<DynamicMethodDummy, Action<LinkedStream>>();

            _actionStorage.SetAction(DynamicMethodDummy.Test1, stream =>
            {
                Console.WriteLine("Hey1");
            });

            _actionStorage.SetAction(DynamicMethodDummy.Test2, stream =>
            {
                Console.WriteLine("Hey2");
            });

            _actionStorage.SetAction(DynamicMethodDummy.Test3, stream =>
            {
                Console.WriteLine("Hey3");
            });
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

            _socket.BeginReceive(_buffer, 0, _nextSegmentLength, SocketFlags.None, ReceiveCallback, null);
        }

        private void HandleStream()
        {
            switch (_state)
            {
                case IOState.HeaderLen:
                    if (_stream.Length >= 4)
                    {
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
                        var opCode = (DynamicMethodDummy)_stream.ReadByte();
                        _nextSegmentLength -= -2;

                        var body = _stream.ReadToLinkedStream(_nextSegmentLength);

                        var handler = _actionStorage.GetDelegate(opCode);

                        switch (options)
                        {
                            case ParallelismOptions.Parallel:
                            //Todo: Parallel Handling
                            case ParallelismOptions.Sync:
                                handler(body);
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    _nextSegmentLength = 4;
                    _state = IOState.HeaderLen;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // ReSharper disable once InconsistentNaming
        private enum IOState
        {
            HeaderLen,
            BodyBytes,
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _socket?.Dispose();
            _stream?.Dispose();
            _bufferWrapperFac.Recycle(_bufferWrapper);
        }
    }
}