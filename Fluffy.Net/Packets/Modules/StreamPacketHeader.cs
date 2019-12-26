using System;
using System.Runtime.InteropServices;
using Fluffy.Unsafe;

namespace Fluffy.Net.Packets.Modules
{
    [StructLayout(LayoutKind.Explicit, Size = 17)]
    internal struct StreamPacketHeader
    {
        public static readonly int Size = FluffyBitConverter.SizeOf<StreamPacketHeader>();

        [FieldOffset(0)] public byte HasFinished;

        [FieldOffset(1)] public Guid StreamId;
    }
}