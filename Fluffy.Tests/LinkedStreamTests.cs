using System;
using System.IO;
using System.Linq;
using BoostedLib;
using Fluffy.IO.Buffer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fluffy.Tests
{
    [TestClass]
    public class LinkedStreamTests
    {
        private static Random _random = new Random();

        [TestMethod]
        public void SequentialTests()
        {
            using (var ls = new LinkedStream(325))
            using (var br = new BinaryReader(ls))
            {
                var counter = 1;
                var showCounter = 0;

                for (int j = 0; j < 10; j++)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        var seq = GetRandomSequence(++counter);
                        ls.Write(seq, 0, seq.Length);
                    }

                    while (true)
                    {
                        try
                        {
                            if (!CheckSequence(br))
                            {
                                Assert.Fail($"Checksum doesn't match");
                            }

                            if (showCounter != 0 && showCounter % 3 == 0)
                            {
                                var seq = GetRandomSequence(_random.Next(0, ls.CacheSize * 2));
                                ls.Write(seq, 0, seq.Length);
                            }
                        }
                        catch (EndOfStreamException)
                        {
                            break;
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void RandomTest()
        {
            using (var ls = new LinkedStream(514))
            using (var br = new BinaryReader(ls))
            {
                var counter = 0;
                for (int j = 0; j < 10; j++)
                {
                    for (int i = 0; i < 866; i++)
                    {
                        var seq = GetRandomSequence(_random.Next(0, ls.CacheSize * 2));
                        ls.Write(seq, 0, seq.Length);
                    }

                    while (true)
                    {
                        try
                        {
                            if (!CheckSequence(br))
                            {
                                Assert.Fail($"Checksum doesn't match");
                            }

                            if (counter != 0 && counter % 3 == 0)
                            {
                                var seq = GetRandomSequence(_random.Next(0, ls.CacheSize * 2));
                                ls.Write(seq, 0, seq.Length);
                            }
                        }
                        catch (EndOfStreamException)
                        {
                            break;
                        }
                    }
                }
            }
        }

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