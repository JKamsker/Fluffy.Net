using System;
using Fluffy.Delegate;
using Fluffy.IO.Buffer;

namespace Fluffy.Net.Packets.Modules.Streaming
{
    public class DefaultStreamHandler : IStreamHandler
    {
        public LinkedStream Stream { get; }

        public int StreamNotificationThreshold { get; set; }
        public EventHandler<DefaultStreamHandler, LinkedStream> OnReceived;

        public Guid StreamIdentifier { get; }

        public bool HasFinished { get; private set; }

        public DefaultStreamHandler(Guid identifier) : this(identifier, new LinkedStream())
        {
        }

        public DefaultStreamHandler(Guid identifier, LinkedStream stream)
        {
            Stream = stream;
            StreamIdentifier = identifier;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            Stream.Write(buffer, offset, count);
            if (Stream.Length >= StreamNotificationThreshold)
            {
                OnReceived?.Invoke(this, Stream);
            }
        }

        public void NotifyEndOfFile()
        {
            HasFinished = true;
            OnReceived?.Invoke(this, Stream);
        }

        public void Dispose()
        {
            Stream?.Dispose();
        }
    }
}