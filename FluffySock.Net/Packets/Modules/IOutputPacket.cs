using Fluffy.Net.Options;

using System;

namespace Fluffy.Net.Packets.Modules
{
    internal interface IOutputPacket : IDisposable
    {
        bool CanBreak { get; }
        bool HasSendHeaders { get; }
        bool IsFinished { get; }
        byte OpCode { get; }
        ParallelismOptions ParallelismOptions { get; }

        int Read(byte[] buffer, int offset, int count);
    }
}