using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BoostedLib;
using Fluffy.IO.Buffer;

namespace NetSocket
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var ls = new LinkedStream())
            using (var bw = new StreamWriter(ls) { AutoFlush = true })
            {
                var br = new BinaryReader(ls);
                var counter = 1;
                var showCounter = 0;
                while (true)
                {
                    for (int i = 0; i < 5000; i++)
                    {
                        var seq = GetRandomSequence(++counter);
                        ls.Write(seq, 0, seq.Length);
                    }

                    while (true)
                    {
                        try
                        {
                            if (CheckSequence(br))
                            {
                                Console.WriteLine($"Good {++showCounter}");
                            }
                            else
                            {
                                Debugger.Break();
                            }
                        }
                        catch (EndOfStreamException e)
                        {
                            br.Dispose();
                            br = new BinaryReader(ls);
                            break;
                        }
                    }
                }
                br.Dispose();
                Console.WriteLine($"Finished");
                Console.ReadLine();
            }
        }

        private static Random _random = new Random();

        private static bool CheckSequence(BinaryReader br)
        {
            var len = br.ReadInt32();
            var data = br.ReadBytes(len);
            var hash = br.ReadBytes(16);

            var calcHash = data.ToMD5Hash();

            return (hash.SequenceEqual(calcHash));
        }

        private static byte[] GetRandomSequence(int lenght)
        {
            var data = new byte[lenght];
            _random.NextBytes(data);

            var hash = data.ToMD5Hash();
            var length = BitConverter.GetBytes(data.Length);

            var seq1 = Enumerable.Concat(length, data);
            var seq2 = Enumerable.Concat(seq1, hash);

            return seq2.ToArray();
        }
    }
}