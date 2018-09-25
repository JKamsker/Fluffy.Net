using System.Diagnostics;
using Fluffy.IO.Buffer;

namespace NetSocket
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var ls = new LinkedStream(8 * 1024))
            {
                var buf = FillBuf(new byte[32 * 1024 * 1024]);
                var dbuf = new byte[1024];

                ls.Write(buf, 0, buf.Length);

                var str = ls.ReadToLinkedStream(20);

                if (str.Length + ls.Length == buf.Length)
                {
                    Debugger.Break();
                }

                // var tread = ls.Read(dbuf, 0, 20 * 1024);
                Debugger.Break();
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