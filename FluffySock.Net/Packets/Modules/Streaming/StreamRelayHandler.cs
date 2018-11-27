using System;
using System.IO;
using Fluffy.Delegate;

namespace Fluffy.Net.Packets.Modules.Streaming
{
    public class StreamRelayHandler : IStreamHandler
    {
        private readonly Stream _targetStream;

        public EventHandler<StreamRelayHandler, Stream> OnStreamFinished;

        public Guid StreamIdentifier { get; }

        public StreamRelayHandler(Guid identifier, Stream targetStream)
        {
            StreamIdentifier = identifier;
            _targetStream = targetStream;
        }

        public StreamRelayHandler OnFinished(Action<Stream> action)
        {
            OnStreamFinished += (sender, stream) => action(stream);
            return this;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _targetStream.Write(buffer, offset, count);
        }

        public void NotifyEndOfFile()
        {
            OnStreamFinished?.Invoke(this, _targetStream);
        }

        public void Dispose()
        {
            _targetStream?.Dispose();
        }
    }
}