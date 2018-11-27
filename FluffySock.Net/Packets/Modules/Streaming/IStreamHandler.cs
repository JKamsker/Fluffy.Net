using System;

namespace Fluffy.Net.Packets.Modules.Streaming
{
    public interface IStreamHandler : IDisposable
    {
        Guid StreamIdentifier { get; }

        void NotifyEndOfFile();

        void Write(byte[] buffer, int offset, int count);
    }
}