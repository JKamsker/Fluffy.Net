using System;
using Fluffy.Net.Packets.Modules.Streaming;

namespace AllInOneExample_FullFramework
{
    public class AsyncStreamHandler : IStreamHandler
    {
        public Guid StreamIdentifier { get; }
        public AsyncLinkedStream Stream { get; private set; }
        public bool HasFinished { get; private set; }

        public AsyncStreamHandler(Guid identifier)
        {
            Stream = new AsyncLinkedStream
            {
                ClearBufferOnDispose = false
            };

            StreamIdentifier = identifier;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            Stream.Write(buffer, offset, count);
        }

        public void NotifyEndOfFile()
        {
            HasFinished = true;
            Stream?.Dispose();
        }

        public void Dispose()
        {
            Stream.ClearBufferOnDispose = true;
            Stream?.Dispose();
        }
    }
}