using Fluffy.IO.Buffer;
using Fluffy.IO.Extensions;
using Fluffy.IO.Recycling;

using System;
using System.Net.Sockets;

namespace Fluffy.Net
{
    internal class IOHandler : IDisposable
    {
        private readonly Socket _socket;
        private byte[] _buffer;
        private IOState _state;
        private LinkedStream _stream;
        private int _nextSegmentLength;

        private volatile bool _started;
        private volatile bool _disposed;

        private ObjectStorage<DynamicMethodDummy, Action<LinkedStream>> _actionStorage;
        private IObjectRecyclingFactory<LinkableBufferObject<byte>> _bufferWrapperFac;
        private LinkableBufferObject<byte> _bufferWrapper;

        private AsyncSender _asyncSender;

        internal IOHandler(Socket socket)
        {
            _socket = socket;

            _asyncSender = new AsyncSender(socket);
            _bufferWrapperFac = BufferRecyclingMetaFactory.Get(Capacity.Medium);
            _bufferWrapper = _bufferWrapperFac.Get();
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

        public IOHandler Start()
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
            var connectionInfo = ar.AsyncState as ConnectionInfo;
            if (connectionInfo == null)
            {
                Console.WriteLine($"The given type did not match the expected one " +
                                  $"(Received: {ar}; Expected: {nameof(ConnectionInfo)})");
                return;
            }
            int bytesRead = connectionInfo.Socket.EndReceive(ar);
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

    internal enum ParallelismOptions : byte
    {
        Parallel,
        Sync
    }
}