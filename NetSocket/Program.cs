using System;
using System.Collections.Generic;
using System.Diagnostics;
using Fluffy.IO.Buffer;
using Fluffy.IO.Recycling;

namespace NetSocket
{
    internal enum Foo
    {
        x,
        xa,
        ass,
        asf,
        ww,
        h
    }

    internal class Program
    {
        private static List<int> _test;

        private static int GetInt<T>(T tinput)
            where T : Enum
        {
            return tinput.GetHashCode();
        }

        private static void Main(string[] args)
        {
            var re = BufferRecyclingMetaFactory.Get(Capacity.Medium);
            var buffer = re.Get();

            using (var ls = new LinkedStream())
            {
                ls.Write(new byte[] { 4, 5, 6 }, 0, 3);
                ls.WriteHead(new byte[] { 1, 2, 3 }, 0, 3);

                var nb = new byte[6];
                ls.Read(nb, 0, 6);

                var buf = FillBuf(new byte[32 * 1024 * 1024]);
                var dbuf = new byte[1024];

                ls.Write(buf, 0, buf.Length);

                var str = ls.ReadToLinkedStream(20);

                if (str.Length + ls.Length == buf.Length)
                {
                    Debugger.Break();
                }

                int read = 0;
                while ((read = ls.Read(buf, 0, buf.Length)) != 0)
                {
                }

                // var tread = ls.Read(dbuf, 0, 20 * 1024);
            }
            Debugger.Break();
        }

        private static void ThreadWork()
        {
            while (true)
            {
                for (int i = 0; i < _test.Count; i++)
                {
                    if (_test[i] / 21 != i)
                    {
                        Debugger.Break();
                    }
                }
            }
        }

        private static byte[] FillBuf(byte[] buf)
        {
            for (int i = 0; i < buf.Length; i++)
            {
                buf[i] = (byte)(i % 255);
            }
            return buf;
        }
    }
}