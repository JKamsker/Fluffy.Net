using System;
using System.Collections.Concurrent;
using Fluffy.Extensions;
using Fluffy.IO.Buffer;
using Fluffy.IO.Recycling;
using Fluffy.Net.Packets.Modules.Raw;

namespace Fluffy.Net.Packets.Modules.Streaming
{
    public class StreamPacketHandler : BasePacketHandler, IDisposable
    {
        private byte[] _buffer;
        private RecyclableBuffer _flufBuf;
        private bool _disposed;

        public bool AllowUnknownStreams { get; set; }
        public EventHandler<DefaultStreamHandler> OnNewStream;

        /// <summary>
        /// Doesn't have to be Thread safe
        /// </summary>
        public ConcurrentDictionary<Guid, IStreamHandler> _streamDictionary;

        public StreamPacketHandler() : base()
        {
            _flufBuf = BufferRecyclingMetaFactory<RecyclableBuffer>.MakeFactory(Capacity.Medium).GetBuffer();
            _buffer = _flufBuf.Value;

            _streamDictionary = new ConcurrentDictionary<Guid, IStreamHandler>();
        }

        public override byte OpCode => (int)PacketTypes.StreamPacket;

        public override void Handle(LinkedStream stream)
        {
            var guidBuffer = new byte[16];
            var endOfFile = stream.ReadByte();

            var read = stream.Read(guidBuffer, 0, guidBuffer.Length);
            if (read == guidBuffer.Length)
            {
                var guid = new Guid(guidBuffer);
                if (!_streamDictionary.TryGetValue(guid, out var inputStream))
                {
                    if (AllowUnknownStreams)
                    {
                        //Stream not registered
                        inputStream = new DefaultStreamHandler(guid);
                        _streamDictionary[guid] = inputStream;
                        OnNewStream?.Invoke(this, (DefaultStreamHandler)inputStream);
                    }
                    else
                    {
                        //Ignore
                        return;
                    }
                }

                while ((read = stream.Read(_buffer, 0, _buffer.Length)) != 0)
                {
                    inputStream.Write(_buffer, 0, read);
                }
                stream.Dispose();

                if (endOfFile == 1)
                {
                    inputStream.NotifyEndOfFile();
                }
            }
        }

        public THandler RegisterStream<THandler>(THandler handler) where THandler : IStreamHandler
        {
            if (_streamDictionary.ContainsKey(handler.StreamIdentifier))
            {
                throw new AccessViolationException("Packet already Exists");
            }

            _streamDictionary[handler.StreamIdentifier] = handler;
            return handler;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _flufBuf.Recycle();
            _buffer = null;

            _streamDictionary.Values.ForEach(x => x.Dispose());
        }
    }
}