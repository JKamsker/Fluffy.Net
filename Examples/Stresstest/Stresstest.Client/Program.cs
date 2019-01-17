using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Fluffy.Net;
using Stresstest.SharedComps;

namespace Stresstest.Client
{
    internal class Program
    {
        private static FluffyClient _client;
        private static Stopwatch _sw;

        private static ConcurrentDictionary<Guid, PerformanceWrapper> _performanceWrappers;

        private static void Main(string[] args)
        {
            _performanceWrappers = new ConcurrentDictionary<Guid, PerformanceWrapper>();

            _client = new FluffyClient(IPAddress.Parse("37.59.53.54"), 8092);
            //_client = new FluffyClient(IPAddress.Loopback, 8092);
            _client.Connection.PacketHandler.On<Dummy>().Do(HandleDummy);
            _client.Connect();
            _sw = Stopwatch.StartNew();

            for (int i = 0; i < 100; i++)
            {
                var dummy = new Dummy();
                _performanceWrappers[dummy.Identifier] = new PerformanceWrapper
                {
                    Stopwatch = Stopwatch.StartNew()
                };
                _client.Connection.Sender.Send(dummy);
            }

            Console.WriteLine("Socket started");
            Console.ReadLine();
        }

        private static Dummy HandleDummy(Dummy arg1)
        {
            arg1.Counter++;
            if (arg1.Counter % 200 == 0)
            {
                if (!_performanceWrappers.TryGetValue(arg1.Identifier, out var performanceWrapper))
                {
                    return null;
                }

                var sw = performanceWrapper.Stopwatch;
                sw.Stop();
                performanceWrapper.LastPPS = arg1.Counter / sw.Elapsed.TotalSeconds;
                performanceWrapper.LastCount = arg1.Counter;

                Console.WriteLine($"{ performanceWrapper.LastPPS} pps\t{performanceWrapper.LastCount}p\t" +
                                  $"TOTAL {_performanceWrappers.Values.Sum(x => x.LastPPS)} pps {_performanceWrappers.Values.Sum(x => x.LastCount)}c");
                sw.Start();
            }
            return arg1;
        }
    }

    internal class PerformanceWrapper
    {
        public double LastPPS { get; set; }
        public int LastCount { get; set; }
        public Stopwatch Stopwatch { get; set; }
    }
}