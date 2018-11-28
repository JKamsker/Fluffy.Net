using System;
using System.Diagnostics;
using System.IO;
using Fluffy.IO.Buffer;

namespace AllInOneExample_FullFramework.Models
{
    internal static class StreamPerformanceTest
    {
        public static void TestLinked()
        {
            using (var ls = new LinkedStream(8 * 1024))
            {
                TestStream(ls);

                Console.WriteLine($"Finished");
            }
        }

        public static void TestFifo()
        {
            //using (var ff = new FifoStream(new RingBuffer(10)))
            //{
            //    TestStream(ff);
            //}
        }

        private static Random _random = new Random();

        private static void TestStream(Stream stream)
        {
            var buf = FillBuf(new byte[32 * 1024 * 1024]);
            var dbuf = new byte[16 * 1024];

            int count = 0;
            int read = 0;
            long totalRead = 0;
            var sw = Stopwatch.StartNew();

            while (true)
            {
                stream.Write(buf, 0, buf.Length);
                while ((read = stream.Read(dbuf, 0, dbuf.Length)) != 0)
                {
                    totalRead += read;
                }

                if (++count % 100 == 0)
                {
                    var bpms = totalRead / sw.Elapsed.TotalMilliseconds;
                    var bps = totalRead / Math.Max(sw.Elapsed.TotalSeconds, 1);

                    var bpmsHr = ToHumanReadable(bpms);
                    var bpsHr = ToHumanReadable(bps);
                    Console.WriteLine($"Read: {totalRead} | {bpmsHr} /ms | {bpsHr}/s");
                }
            }
        }

        private static string ToHumanReadable(double len)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
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