using Fluffy.Net;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AllInOneExample_FullFramework.Models;
using Fluffy.IO.Buffer;
using Fluffy.IO.Recycling;
using Fluffy.Net.Packets.Modules.Streaming;

namespace AllInOneExample_FullFramework
{
    internal class Program
    {
        private static Stopwatch _sw;

        private static FluffyServer _server;
        private static FluffyClient _client;

        private static void Main(string[] args)
        {
            _server = new FluffyServer(8090);
            _client = new FluffyClient(IPAddress.Loopback, 8090);

            _client.Connection.PacketHandler.On<MyAwesomeClass>().Do(Awesome);
            _server.PacketHandler.On<MyAwesomeClass>().Do(Awesome);

            _client.Connection.PacketHandler.On<StreamRegistration>().Do(x => StreamRegistration(x, _client.Connection));
            _server.PacketHandler.On<StreamRegistration>().Do(StreamRegistration);

            _server.Start();
            _server.OnNewConnection += OnNewConnection;

            _client.Connect();
            Console.WriteLine("Connected");

            Console.WriteLine("Test sent");
            Console.ReadLine();
        }

        private static MyAwesomeClass Awesome(MyAwesomeClass awesome)
        {
            awesome.Packets++;
            return awesome;
        }

        private static StreamRegistration StreamRegistration(StreamRegistration registration, ConnectionInfo connection)
        {
            var hashAlgorithm = MD5.Create();
            var buffer = BufferRecyclingMetaFactory<RecyclableBuffer>.MakeFactory(Capacity.Medium).GetBuffer();

            var streamHandler = new DefaultStreamHandler(registration.Guid)
            {
                StreamNotificationThreshold = 1024 * 1024
            };
            //1KB
            streamHandler.OnReceived += (handler, stream) =>
            {
                int read; //= stream.Read(buffer.Value, 0, buffer.Value.Length);
                while ((read = stream.Read(buffer.Value, 0, buffer.Value.Length)) != 0)
                {
                    hashAlgorithm.TransformBlock(buffer.Value, 0, read, null, 0);
                }

                if (handler.HasFinished)
                {
                    hashAlgorithm.TransformFinalBlock(buffer.Value, 0, 0);
                    var stringHash = string.Concat(hashAlgorithm.Hash.Select(x => x.ToString("x2")));

                    Console.WriteLine($"[Receiver] Hash is {stringHash}");
                    handler.Dispose();
                }
            };

            connection.StreamPacketHandler.RegisterStream(streamHandler);
            registration.StatusCode = StatusCode.Ok;
            return registration;
        }

        private static async void OnNewConnection(object sender, ConnectionInfo connection)
        {
            await SendMultipleStresstestAsync(connection);
            await SendFileExampleAsync(connection);
        }

        private static async Task SendFileExampleAsync(ConnectionInfo connection)
        {
            var streamFile = @"C:\Users\Weirdo\Downloads\SciFiEbooks\Der Wüstenplanet - Die Enzyklopädie Bd. 1 2.epub";
            var awesome = new MyAwesomeClass
            {
                AwesomeString = "AWESOME!!"
            };

            var registration = await connection.Sender.Send<StreamRegistration>(new StreamRegistration());
            Console.WriteLine($"Stream id is: {registration.Guid}");

            using (var ha = MD5.Create())
            using (var fs = File.OpenRead(streamFile))
            {
                var hash = ha.ComputeHash(fs);
                var stringHash = string.Concat(hash.Select(x => x.ToString("x2")));
                Console.WriteLine($"[Sender] Hash is {stringHash}");
            }

            connection.Sender.SendStream(registration.Guid, File.OpenRead(streamFile));

            while (true)
            {
                awesome = await connection.Sender.Send<MyAwesomeClass>(awesome);
                Console.WriteLine($"{awesome.Packets} Result: {awesome.AwesomeString}");
                await Task.Delay(10);
            }
        }

        public static async Task SendMultipleStresstestAsync(ConnectionInfo connection)
        {
            int exitCounter = 0;
            var dummy = GenerateDummyData();
            _sw = Stopwatch.StartNew();

            while (true)
            {
                var tList = dummy.Select(x => connection.Sender.Send<MyAwesomeClass>(x)).ToList();
                await Task.WhenAll(tList.Select(x => x.Task));
                for (int i = 0; i < tList.Count; i++)
                {
                    dummy[i] = tList[i].Result;
                }

                if (dummy[0].Packets % 100 == 0)
                {
                    _sw.Stop();
                    var count = dummy.Sum(x => x.Packets);

                    Console.WriteLine($"AVG Delay: {(_sw.Elapsed.TotalMilliseconds / count)} ms " +
                                      $"{count}:  ({count * 2 / _sw.Elapsed.TotalMilliseconds} p/ms)");
                    exitCounter++;
                    if (exitCounter >= 1000)
                    {
                        Console.WriteLine($"Test Finished, exiting");
                        break;
                    }
                    _sw.Start();
                }
            }
        }

        private static List<MyAwesomeClass> GenerateDummyData()
        {
            var cid = 0;

            var awesomeList = new List<MyAwesomeClass>();
            for (int i = 0; i < 100; i++)
            {
                awesomeList.Add(new MyAwesomeClass
                {
                    Id = cid++,
                    AwesomeString = "AWESOME!!"
                });
            }

            return awesomeList;
        }
    }
}