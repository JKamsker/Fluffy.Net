using Fluffy.Net;
using Fluffy.Net.Packets.Modules.Formatted;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

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
            _server.OnNewConnection += OnNewConnection;

            _client.Connect();
            Console.WriteLine("Connected");
            TypedTest(_client.Connection);
            //_client.Test();
            //_client.Test();
            Console.WriteLine("Test sent");
            Console.ReadLine();
        }

        private static async void OnNewConnection(object sender, ConnectionInfo connection)
        {
            var cid = 0;

            var awesomeList = new List<MyAwesomeClass>
            {
                new MyAwesomeClass
                {
                    Id = cid++,
                    AwesomeString = "AWESOME!!"
                },
                new MyAwesomeClass
                {
                    Id = cid++,
                    AwesomeString = "AWESOME!!"
                },
                new MyAwesomeClass
                {
                    Id = cid++,
                    AwesomeString = "AWESOME!!"
                },
                new MyAwesomeClass
                {
                    Id = cid++,
                    AwesomeString = "AWESOME!!"
                },
                new MyAwesomeClass
                {
                    Id = cid++,
                    AwesomeString = "AWESOME!!"
                },
                new MyAwesomeClass
                {
                    Id = cid++,
                    AwesomeString = "AWESOME!!"
                },
                new MyAwesomeClass
                {
                    Id = cid++,
                    AwesomeString = "AWESOME!!"
                },
            };

            var awesome = new MyAwesomeClass
            {
                AwesomeString = "AWESOME!!"
            };

            //using (var ha = MD5.Create())
            //using (var fs = File.OpenRead(@"C:\Users\BEKO\Downloads\AP.Server.Host.7z"))
            //{
            //    var hash = ha.ComputeHash(fs);
            //    var stringHash = string.Concat(hash.Select(x => x.ToString("x2")));
            //    Console.WriteLine($"Hash is {stringHash}");
            //}

            // connection.Sender.SendStream(Guid.NewGuid(), File.OpenRead(@"C:\Users\BEKO\Downloads\AP.Server.Host.7z"));
            // Console.ReadLine();

            _sw = Stopwatch.StartNew();
            while (true)
            {
                //var tList = awesomeList.Select(x => connection.Sender.Send<MyAwesomeClass>(x)).ToList();

                //await Task.WhenAll(tList.Select(x => x.Task));
                //for (int i = 0; i < tList.Count; i++)
                //{
                //    awesomeList[i] = tList[i].Value;
                //}

                //if (awesomeList[0].Packets % 500 == 0)
                //{
                //    _sw.Stop();
                //    var count = awesomeList.Sum(x => x.Packets);

                //    Console.WriteLine($"AVG Delay: {(_sw.Elapsed.TotalMilliseconds / count)} ms " +
                //                      $"{count}:  ({count * 2 / _sw.Elapsed.TotalMilliseconds})");
                //    _sw.Start();
                //}
                //    _client.Connection.

                //   awesome = srvEp.PacketHandler.Handle(awesome) as MyAwesomeClass;
                //    awesome = await connection.Sender.Send<MyAwesomeClass>(awesome).Task;

                awesome = connection.Sender.Send<MyAwesomeClass>(awesome).Value;

                if (awesome.Packets % 500 == 0)
                {
                    _sw.Stop();
                    Console.WriteLine($"AVG Delay: {(_sw.Elapsed.TotalMilliseconds / awesome.Packets)} ms " +
                                      $"{awesome.Packets}:  ({awesome.Packets * 2 / _sw.Elapsed.TotalMilliseconds})");
                    _sw.Start();
                }
            }
        }

        private static Stopwatch _sw;

        private static void TypedTest(ConnectionInfo connection)
        {
        }

        private static MyAwesomeClass Awesome(MyAwesomeClass awesome)
        {
            awesome.Packets++;
            return awesome;
        }
    }
}