using Fluffy.Extensions;
using Fluffy.IO.Buffer;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fluffy.Tests
{
    [TestFixture]
    public class LinkedStreamTests
    {
        private static Random _random = new Random();

        [Test]
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

        [Test]
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

        [Test]
        public void ShadowStreamTest()
        {
            using (var ls = new LinkedStream())
            {
                for (int i = 0; i < 1024; i++)
                {
                    var seq = GetRandomSequence(_random.Next(0, ls.CacheSize * 2));
                    ls.Write(seq, 0, seq.Length);
                }

                ls.Lock();

                using (var shadowCopy1 = new BinaryReader(ls.CreateShadowCopy()))
                using (var shadowCopy2 = new BinaryReader(ls.CreateShadowCopy()))
                {
                    while (true)
                    {
                        try
                        {
                            if (!CheckSequences(shadowCopy1, shadowCopy2))
                            {
                                Assert.Fail($"Checksum or ShadowStreams doesn't match");
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

            var calcHash = data.ToMd5Hash();

            return (hash.SequenceEqual(calcHash));
        }

        private static bool CheckSequences(params BinaryReader[] binaryReaders)
        {
            var bResults = new List<byte[]>();
            foreach (var br in binaryReaders)
            {
                var len = br.ReadInt32();
                var data = br.ReadBytes(len);
                var hash = br.ReadBytes(16);

                var calcHash = data.ToMd5Hash();

                if (!hash.SequenceEqual(calcHash))
                {
                    return false;
                }
                bResults.Add(calcHash);
            }

            byte[] sample = bResults.FirstOrDefault();
            if (sample == null)
            {
                throw new NullReferenceException($"Sample cannot be null");
            }
            foreach (var bResult in bResults)
            {
                if (!bResult.SequenceEqual(sample))
                {
                    return false;
                }
            }

            return true;
        }

        private static byte[] GetRandomSequence(int lenght)
        {
            var data = new byte[lenght];
            _random.NextBytes(data);

            var hash = data.ToMd5Hash();
            var length = BitConverter.GetBytes(data.Length);

            var seq1 = Enumerable.Concat(length, data);
            var seq2 = Enumerable.Concat(seq1, hash);

            return seq2.ToArray();
        }
    }
}