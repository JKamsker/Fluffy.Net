using Fluffy.Unsafe;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace PlayGround
{
    //[StructLayout(LayoutKind.Explicit)]
    //struct ByteArray
    //{
    //    [FieldOffset(0)]
    //    public byte Byte1;
    //    [FieldOffset(1)]
    //    public byte Byte2;
    //    [FieldOffset(2)]
    //    public byte Byte3;
    //    [FieldOffset(3)]
    //    public byte Byte4;
    //    [FieldOffset(4)]
    //    public byte Byte5;
    //    [FieldOffset(5)]
    //    public byte Byte6;
    //    [FieldOffset(6)]
    //    public byte Byte7;
    //    [FieldOffset(7)]
    //    public byte Byte8;


    //    [FieldOffset(0)]
    //    public int Int1;
    //    [FieldOffset(4)]
    //    public int Int2;


    //    [FieldOffset(0)]
    //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
    //    public byte[] arr;
    //}
    //[StructLayout(LayoutKind.Explicit)]
    //public struct IndexStruct
    //{
    //    [FieldOffset(0)]
    //    public byte[16] data;

    //    [FieldOffset(16)]
    //    public short idx16;

    //    [FieldOffset(18)]
    //    public int idx32;
    //}



    static class SizeHelper
    {
        private static Dictionary<Type, int> sizes = new Dictionary<Type, int>();

        public static int SizeOf(Type type)
        {
            int size;
            if (sizes.TryGetValue(type, out size))
            {
                return size;
            }

            size = SizeOfType(type);
            sizes.Add(type, size);
            return size;
        }

        private static int SizeOfType(Type type)
        {
            var dm = new DynamicMethod("SizeOfType", typeof(int), new Type[] { });
            ILGenerator il = dm.GetILGenerator();
            il.Emit(OpCodes.Sizeof, type);
            il.Emit(OpCodes.Ret);
            return (int)dm.Invoke(null, null);
        }

        public static int SizeOfType2(Type type)
        {
            var dm = new DynamicMethod("SizeOfType", typeof(int), new Type[] { typeof(Type) });
            ILGenerator il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Sizeof);
            il.Emit(OpCodes.Ret);

            var delegate1 = (Func<Type, int>)dm.CreateDelegate(typeof(Func<Type, int>));
            return delegate1(type);
        }

        public static unsafe int MySizeOf<T>() where T : unmanaged
        {
            return sizeof(T);
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 23)]
    public struct PacketHeader
    {
        public static int Size = SizeHelper.MySizeOf<PacketHeader>();

        [FieldOffset(0)] public int PacketLength;
        [FieldOffset(4)] public byte ParallelismOptions;
        [FieldOffset(5)] public byte OpCode;
        [FieldOffset(6)] public byte HasFinished;

        [FieldOffset(7)] public Guid StreamId;
    }

    // public static class  Si

    public static class Repro
    {
        private const int PacketHeaderLengthSize = 4;
        private const int ParallelismOptionsSize = 1;
        private const int OpCodeSize = 1;
        private const int EndOfFileSize = 1;
        private const int GuidSize = 16;
        private const int FirstLevelHeader = PacketHeaderLengthSize;
        private const int SecondLevelHeader = ParallelismOptionsSize + OpCodeSize;
        private const int ThirdLevelHeader = EndOfFileSize + GuidSize;
        private const int Header = FirstLevelHeader + SecondLevelHeader + ThirdLevelHeader;


        public static bool Check()
        {
            var offset = 10;
            var reaCount = 100;
            
            var ok1 = (offset + Header) == offset + SizeHelper.MySizeOf<PacketHeader>();
            var ok2 = (reaCount - Header) == reaCount - SizeHelper.MySizeOf<PacketHeader>();

            return ok1 && ok2;
        }

        public static int TestPacketOld(byte[] buffer, int offset)
        {
            var bodyRead = 200;
            var hasFinished = true;

            byte opCode = 2;
            var parallelismOptions = 12;
            var streamId = Guid.Parse("81A5F4A1-EBCC-4C1E-A658-693AB96E03C2");
            //var packetLength = bodyRead + SecondLevelHeader + ThirdLevelHeader;
            var packetLength = int.MaxValue / 2;

            FluffyBitConverter.Serialize(packetLength, buffer, offset);
            offset += 4;

            buffer[offset++] = (byte)parallelismOptions;
            buffer[offset++] = opCode;
            buffer[offset++] = (byte)(hasFinished ? 1 : 0);
            //Injected body
            FluffyBitConverter.Serialize(streamId, buffer, offset);

            return bodyRead + Header;
        }
    }

    internal class Program
    {
        private const int loops = 10000000;



        private static unsafe void Main(string[] args)
        {
            var offset = 10;

            var header = new PacketHeader
            {
                PacketLength = int.MaxValue / 2,
                ParallelismOptions = 12,
                OpCode = 2,
                HasFinished = 1,
                StreamId = Guid.Parse("81A5F4A1-EBCC-4C1E-A658-693AB96E03C2")
            };

            var sizeX = SizeHelper.MySizeOf<PacketHeader>();

            var target1 = new byte[SizeHelper.MySizeOf<PacketHeader>() + offset];
            var target2 = new byte[SizeHelper.MySizeOf<PacketHeader>() + offset];

            FluffyBitConverter.Serialize(header, target1, offset);

            Repro.TestPacketOld(target2, offset);

            var ok = target1.SequenceEqual(target2);

            Repro.Check();

            //Debugger.Break();
            //FluffyBitConverter.SerializeGeneric(header, target, 10);



            var size = SizeHelper.MySizeOf<PacketHeader>();


            // var size = SizeHelper.SizeOfType2(typeof(PacketHeader));



            var sw = Stopwatch.StartNew();

            long value = 16;
            var bx = new byte[]
            {
                0xff,
                0xff,
                0xff,
                0xff,
                0xff,
                0xff,
                0xff,
                0xff,
            };

            //var arr1 = Enumerable.Range(0, 5000).Select(x => (byte)(x % 255)).ToArray();
            //var arr2 = Enumerable.Range(0, 5000).Select(x => (byte)(x % 255)).ToArray();
            //Console.WriteLine(arr1.Length);
            //for (int j = 0; j < 20; j++)
            //{
            //    sw.Restart();
            //    for (int i = 0; i < loops; i++)
            //    {
            //        // var equal = FluffyCompare.ByteArrayCompare(arr1, arr2); var eq = arr1.SequenceEqual(arr2);
            //        var equal = FluffyCompare.ByteArrayCompare(arr1, arr2);
            //    }
            //    sw.Stop();
            //    Console.WriteLine($"MemCmp: {sw.Elapsed.TotalMilliseconds}");
            //}

            //Console.ReadLine();
            //fixed (byte* b = bx)
            //{
            //    *((int*)(b + 1)) = value;
            //}

            // BitConverter.GetBytes(15);
            //var barr = gu.ToByteArray();

            //var fg = new FakeGuid(barr);
            //var fbarr = fg.ToByteArray();

            //var narr = new byte[16];
            //fg.SerializeTo(narr);

            //var xarr = new byte[16];
            //fg.SerializeToUnsafe(xarr, 0);

            Thread.Sleep(1000);

            var gu = Guid.NewGuid();
            var fg = new FakeGuid(gu.ToByteArray());

            var yarr = new byte[16];
            //sw.Restart();
            //for (int j = 0; j < 10; j++)
            //{
            //    for (int i = 0; i < loops; i++)
            //    {
            //        FluffyBitConverter.Serialize(value, bx, 0);
            //    }
            //    sw.Stop();
            //    Console.WriteLine($"Unsafe get bytes {sw.Elapsed.TotalMilliseconds}");
            //    sw.Restart();
            //}

            //Console.WriteLine("Switch");
            //for (int j = 0; j < 10; j++)
            //{
            //    for (int i = 0; i < loops; i++)
            //    {
            //        bx = BitConverter.GetBytes(value);
            //    }
            //    sw.Stop();
            //    Console.WriteLine($"Unsafe get bytes {sw.Elapsed.TotalMilliseconds} {bx.Length}");
            //    sw.Restart();
            //}

            Console.WriteLine($"------------- BYTE TO GUID ----------");
            Guid gnew = default;
            FluffyBitConverter.Serialize(gu, yarr, 0);
            gnew = FluffyBitConverter.FromBytes<Guid>(yarr);

            gnew = FluffyBitConverter.ReadUsingMarshalUnsafe<Guid>(yarr);
            Console.WriteLine(gnew.ToString());
            // Array.Clear(yarr, 0, yarr.Length);
            Console.WriteLine(gnew.ToString());

            sw.Restart();
            for (int i = 0; i < loops; i++)
            {
                gnew = FluffyBitConverter.FromBytes<Guid>(yarr);
            }
            sw.Stop();
            Console.WriteLine($"Marshalling: {sw.Elapsed.TotalMilliseconds}");
            Console.WriteLine(gnew.ToString());

            sw.Restart();
            for (int i = 0; i < loops; i++)
            {
                gnew = FluffyBitConverter.ReadUsingMarshalUnsafe<Guid>(yarr);
            }
            sw.Stop();
            Console.WriteLine($"Marshalling+unsafe: {sw.Elapsed.TotalMilliseconds}");
            Console.WriteLine(gnew.ToString());


            sw.Restart();
            for (int i = 0; i < loops; i++)
            {
                gnew = new Guid(yarr);
            }
            sw.Stop();
            Console.WriteLine($"New Guid: {sw.Elapsed.TotalMilliseconds}");
            Console.WriteLine(gnew.ToString());


            Console.WriteLine($"-------------GUID TO BYTE ----------");

            //Console.ReadLine();
            sw.Restart();
            for (int i = 0; i < loops; i++)
            {
                fixed (byte* b = yarr)
                {
                    *((Guid*)b) = gu;
                }
                Array.Clear(yarr, 0, 16);
            }
            sw.Stop();
            Console.WriteLine($"Unsafe direct+arr.clear {sw.Elapsed.TotalMilliseconds}");

            sw.Restart();
            for (int i = 0; i < loops; i++)
            {
                fixed (byte* b = yarr)
                {
                    *((Guid*)b) = gu;
                }
            }
            sw.Stop();
            Console.WriteLine($"Unsafe direct {sw.Elapsed.TotalMilliseconds}");

            sw.Restart();
            for (int i = 0; i < loops; i++)
            {
                FluffyBitConverter.Serialize(gu, yarr, 0);
            }
            sw.Stop();
            Console.WriteLine($"Unsafe {sw.Elapsed.TotalMilliseconds}");

            sw.Restart();
            for (int i = 0; i < loops; i++)
            {
                yarr = gu.ToByteArray();
                Array.Clear(yarr, 0, 16);
            }
            //GC.Collect();

            sw.Stop();
            Console.WriteLine($"Conventional {sw.Elapsed.TotalMilliseconds}");

            //for (int i = 0; i < loops; i++)
            //{
            //    fg.SerializeTo(yarr);
            //    Array.Clear(yarr, 0, 16);
            //}
            //sw.Stop();
            //Console.WriteLine(sw.Elapsed.TotalMilliseconds);
            //sw.Restart();

            //Console.WriteLine(barr.SequenceEqual(fbarr));
            //Console.WriteLine(barr.SequenceEqual(narr));
            //Console.WriteLine(barr.SequenceEqual(yarr));
            Console.ReadLine();
        }
    }

    public class FakeGuid
    {
        private int _a;
        private short _b;
        private short _c;
        private byte _d;
        private byte _e;
        private byte _f;
        private byte _g;
        private byte _h;
        private byte _i;
        private byte _j;
        private byte _k;

        public FakeGuid(byte[] b)
        {
            if (b == null)
                throw new ArgumentNullException(nameof(b));
            if (b.Length != 16)
                throw new ArgumentException("Expected length: 16");
            this._a = (int)b[3] << 24 | (int)b[2] << 16 | (int)b[1] << 8 | (int)b[0];
            this._b = (short)((int)b[5] << 8 | (int)b[4]);
            this._c = (short)((int)b[7] << 8 | (int)b[6]);
            this._d = b[8];
            this._e = b[9];
            this._f = b[10];
            this._g = b[11];
            this._h = b[12];
            this._i = b[13];
            this._j = b[14];
            this._k = b[15];
        }

        public void SerializeTo(byte[] target)
        {
            target[0] = (byte)_a;
            target[1] = (byte)(_a >> 8);
            target[2] = (byte)(_a >> 16);
            target[3] = (byte)(_a >> 24);

            target[4] = (byte)_b;
            target[5] = (byte)((uint)_b >> 8);

            target[6] = (byte)_c;
            target[7] = (byte)((uint)_c >> 8);

            target[8] = _d;
            target[9] = _e;
            target[10] = _f;
            target[11] = _g;
            target[12] = _h;
            target[13] = _i;
            target[14] = _j;
            target[15] = _k;
        }

        public unsafe void SerializeToUnsafe(byte[] target, int offset)
        {
            fixed (byte* b = target)
            {
                var pos1 = offset == 0 ? b : b + offset;
                *((int*)pos1) = _a;
                pos1 += 4;

                *((short*)pos1) = _b;
                pos1 += 2;

                *((short*)pos1) = _c;
                var x = (int)pos1;
            }
        }

        public byte[] ToByteArray()
        {
            return new byte[16]
            {
                (byte) this._a,
                (byte) (this._a >> 8),
                (byte) (this._a >> 16),
                (byte) (this._a >> 24),
                (byte) this._b,
                (byte) ((uint) this._b >> 8),
                (byte) this._c,
                (byte) ((uint) this._c >> 8),
                this._d,
                this._e,
                this._f,
                this._g,
                this._h,
                this._i,
                this._j,
                this._k
            };
        }
    }
}