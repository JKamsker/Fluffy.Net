using Fluffy.Net;
using Fluffy.Net.Packets.Modules.Formatted;

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;

namespace NetSocket
{
    internal class Program
    {
        private static FluffyServer _server;
        private static FluffyClient _client;

        private static void Main(string[] args)
        {
            _server = new FluffyServer(8090);
            _client = new FluffyClient(IPAddress.Loopback, 8090);

            _client.Connection.PacketHandler.On<MyAwesomeClass>().Do(Awesome);
            _server.PacketHandler.On<MyAwesomeClass>().Do(Awesome);

            _server.Start();
            _client.Connect();
            Console.WriteLine("Connected");
            TypedTest(_client.Connection);
            _client.Test();
            _client.Test();
            Console.WriteLine("Test sent");
            Console.ReadLine();
        }

        private static Stopwatch _sw;

        private static void TypedTest(ConnectionInfo connection)
        {
            var awesome = new MyAwesomeClass
            {
                AwesomeString = "AWESOME!!"
            };

            Thread.Sleep(1000);

            var srvEp = _server.Connections.First();

            var count = 0;

            _sw = Stopwatch.StartNew();
            while (true)
            {
                // awesome = srvEp.PacketHandler.Handle(awesome) as MyAwesomeClass;
                awesome = connection.Sender.Send<MyAwesomeClass>(awesome).Value;

                count++;

                if (awesome.Packets % 300 == 0)
                {
                    _sw.Stop();
                    Console.WriteLine($"AVG Delay: {(_sw.Elapsed.TotalMilliseconds / count)} ms {awesome.Packets}:  ({awesome.Packets * 2 / _sw.Elapsed.TotalMilliseconds})");
                    _sw.Start();
                }
            }
        }

        private static MyAwesomeClass Awesome(MyAwesomeClass awesome)
        {
            //if (_sw == null)
            //{
            //    _sw = Stopwatch.StartNew();
            //}

            //if (awesome.Packets % 300 == 0)
            //{
            //    Console.WriteLine($"{awesome.Packets}:  ({awesome.Packets / _sw.Elapsed.TotalMilliseconds})");
            //}

            awesome.Packets++;
            return awesome;
        }
    }
}