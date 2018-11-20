using Fluffy.Net.Options;

using System;

namespace Fluffy.Net.Packets.Modules
{
    internal interface IOutputPacket : IDisposable
    {
        bool IsPrioritized { get; set; }
        bool CanBreak { get; }
        bool HasFinished { get; }

        byte OpCode { get; }
        ParallelismOptions ParallelismOptions { get; }

        int Read(byte[] buffer, int offset, int count);
    }
}