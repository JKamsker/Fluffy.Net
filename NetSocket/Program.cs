using Fluffy;
using Fluffy.IO.Buffer;
using Fluffy.IO.Recycling;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

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

    internal class Stuff
    {
        public int Result = 555;
    }

    internal class MCL
    {
        public int Lol { get; set; }
    }

    internal class Program
    {
        private static List<int> _test;

        private static int GetInt<T>(T tinput)
            where T : Enum
        {
            return tinput.GetHashCode();
        }

        private static object Load(Stuff stuff)
        {
            return stuff.Result;
        }

        private static async Task TaskWithoutResult()
        {
            var tsk = TaskWithResult();
            await tsk;
            await tsk;
            //await Task.Delay(100000);
            // return await TaskWithResult();
        }

        private static async Task<int> TaskWithResult()
        {
            // await Task.Delay(10000);
            return 50;
        }

        private static void Main(string[] args)
        {
            Task res1 = TaskWithoutResult();
            Task res2 = TaskWithResult();

            // var res = TaskUtility.GetResultCached(res2);

            var sw = Stopwatch.StartNew();

            //var instance = new Stuff();
            //var field = typeof(Stuff).GetField("Result");// BindingFlags.NonPublic | BindingFlags.Instance
            //var getter = TaskUtility.CreateGetter<Stuff>(field);
            //var res = getter(instance);

            var field = res2.GetType().GetField("m_result", BindingFlags.NonPublic | BindingFlags.Instance);//
            var getter = TaskUtility.CreateGetter<Task>(field);
            var res = getter(res2);

            Debugger.Break();

            //for (int j = 0; j < 10; j++)
            //{
            //    for (int i = 0; i < 1000000; i++)
            //    {
            //        TaskUtility.GetResult(res2);
            //    }
            //    sw.Stop();
            //    Console.WriteLine(sw.Elapsed.TotalMilliseconds);
            //    sw.Restart();
            //}

            //Console.WriteLine();
            //sw.Restart();
            //for (int j = 0; j < 10; j++)
            //{
            //    for (int i = 0; i < 1000000; i++)
            //    {
            //        TaskUtility.GetResultCached(res2);
            //    }
            //    sw.Stop();
            //    Console.WriteLine(sw.Elapsed.TotalMilliseconds);
            //    sw.Restart();
            //}

            //Console.WriteLine();
            //sw.Restart();
            for (int j = 0; j < 10; j++)
            {
                for (int i = 0; i < 1000000; i++)
                {
                    TaskUtility.GetResultDynamic(res2);
                }
                sw.Stop();
                Console.WriteLine(sw.Elapsed.TotalMilliseconds);
                sw.Restart();
            }

            Console.ReadLine();

            var stuff = new Stuff();
            var result = Load(stuff);
            Console.WriteLine(result);

            //var resultx = res2.Result;

            //var allresType = typeof(Task<object>).GetProperty("Result");

            //var nres = allresType.GetGetMethod().Invoke(res2, null);

            //var type = res1.GetType().GetProperty("Result");
            //if (type != null)
            //{
            //    if (type.PropertyType.Name == "VoidTaskResult")
            //    {
            //        // Real Non-Generic Task
            //        Debugger.Break();
            //    }
            //    else
            //    {
            //    }
            //    var untypedResult = type.GetGetMethod().Invoke(res1, null);

            //    Debugger.Break();
            //}

            //var result = TaskWithResult().Result;

            //Console.WriteLine(result);
            Console.ReadLine();
            //TestDifFunc();
            //return;
            //var server = new FluffyServer(8090);
            //var client = new FluffyClient(IPAddress.Loopback, 8090);
            //server.Start();
            //client.Connect();
            //Console.WriteLine("Connected");
            //client.TypedTest();
            //client.Test();
            //client.Test();
            //Console.WriteLine("Test sent");
            //Console.ReadLine();

            //Debugger.Break();
        }

        private static void TestDifFunc()
        {
            for (int j = 0; j < 50; j++)
            {
                var sw = Stopwatch.StartNew();
                var totalsum = 0;
                for (int i = 0; i < 1000000; i++)
                {
                    bool res1 = Func1(out int lel);
                    totalsum += lel;
                }
                sw.Stop();
                Console.WriteLine($"1 Took {sw.Elapsed.TotalMilliseconds}");
                sw.Restart();
                for (int i = 0; i < 1000000; i++)
                {
                    var (res1, lel) = Func2();
                    totalsum += lel;
                }
                sw.Stop();
                Console.WriteLine($"2 Took {sw.Elapsed.TotalMilliseconds}");
                Console.WriteLine($"  Sum: {totalsum}");
            }
        }

        private static bool Func1(out int i)
        {
            i = 1 + 1;
            return true;
        }

        private static (bool, int) Func2()
        {
            return (true, 1 + 1);
        }

        private static void TestLinkedStream()
        {
            var capacity = Capacity.Small;
            Console.WriteLine("Testing without recycling");
            var size = capacity.ToInt();
            byte[] buffer1;
            for (int j = 0; j < 10; j++)
            {
                var sw = Stopwatch.StartNew();
                var list = new List<byte[]>();
                for (int i = 0; i < 1000000; i++)
                {
                    buffer1 = new byte[size];
                    list.Add(buffer1);
                    if (list.Count >= 300)
                    {
                        list.Clear();
                    }
                    //buffer1[1] = 123;
                    //  GC.Collect();
                }
                list = new List<byte[]>();

                GC.Collect();

                sw.Stop();

                Console.WriteLine($"Took {sw.Elapsed.TotalMilliseconds}");
            }

            Console.WriteLine();
            Console.WriteLine("Testing with recycling");
            //Console.ReadLine();
            var re = BufferRecyclingMetaFactory<LinkableBuffer>.MakeFactory(capacity);
            var ra = BufferRecyclingMetaFactory<FluffyBuffer>.MakeFactory(capacity);

            for (int j = 0; j < 10; j++)
            {
                var sw = Stopwatch.StartNew();
                var list = new List<LinkableBuffer>();
                for (int i = 0; i < 1000000; i++)
                {
                    var buffer = re.GetBuffer();
                    list.Add(buffer);
                    if (list.Count >= 300)
                    {
                        foreach (var buf in list)
                        {
                            buf.Recycle();
                        }
                        list.Clear();
                    }
                }
                sw.Stop();
                Console.WriteLine($"Took {sw.Elapsed.TotalMilliseconds}");
            }

            Console.ReadLine();
            var buf1 = new BufferRecyclingFactory<FluffyBuffer>(50);

            if (buf1 is IRecycler<FluffyBuffer>)
            {
                Debugger.Break();
            }

            if (buf1 is IRecycler<LinkableBuffer>)
            {
                Debugger.Break();
            }

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