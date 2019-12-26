using System.Runtime.InteropServices;
using Fluffy.Unsafe;

namespace Fluffy.Net.Packets.Modules
{
    [StructLayout(LayoutKind.Explicit, Size = 6)]
    internal struct PacketHeader
    {
        public static readonly int Size = FluffyBitConverter.SizeOf<PacketHeader>();

        [FieldOffset(0)] public int PacketLength;
        [FieldOffset(4)] public byte ParallelismOptions;
        [FieldOffset(5)] public byte OpCode;
        //[FieldOffset(6)] public byte HasFinished;

        //[FieldOffset(7)] public Guid StreamId;
    }
}