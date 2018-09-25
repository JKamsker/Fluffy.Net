using System;
using System.IO;

namespace Fluffy.IO.Extensions
{
    public static class StreamExtensions
    {
        public static int ReadInt32(this Stream ffs)
        {
            if (ffs.Length >= 4)
            {
                var nBuf = new byte[4];

                var read = ffs.Read(nBuf, 0, 4);
                if (read != 4)
                    throw new AggregateException();

                var res = nBuf[0] | (nBuf[1] << 8) | (nBuf[2] << 16) | (nBuf[3] << 24);

                return res;
            }
            return -1;
        }
    }
}