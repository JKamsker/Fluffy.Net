using Fluffy.Unsafe;

using System;
using System.Diagnostics;
using System.Threading;

namespace PlayGround
{
    internal class Program
    {
        private static unsafe void Main(string[] args)
        {
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

            const int loops = 1000000 * 10;
            var yarr = new byte[16];
            var sw = Stopwatch.StartNew();
            for (int j = 0; j < 10; j++)
            {
                for (int i = 0; i < loops; i++)
                {
                    FluffyBitConverter.GetBytes(value, bx, 0);
                }
                sw.Stop();
                Console.WriteLine($"Unsafe get bytes {sw.Elapsed.TotalMilliseconds}");
                sw.Restart();
            }

            Console.WriteLine("Switch");
            for (int j = 0; j < 10; j++)
            {
                for (int i = 0; i < loops; i++)
                {
                    bx = BitConverter.GetBytes(value);
                }
                sw.Stop();
                Console.WriteLine($"Unsafe get bytes {sw.Elapsed.TotalMilliseconds} {bx.Length}");
                sw.Restart();
            }

            Console.ReadLine();
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
                gu.SerializeTo(yarr, 0);
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