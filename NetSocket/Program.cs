using Fluffy.Net;
using Fluffy.Net.Packets.Modules.Formatted;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using Fluffy.IO.Buffer;
using Fluffy.IO.Recycling;
using Fluffy.Net.Packets.Modules.Streaming;

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

            _client.Connection.PacketHandler.On<StreamRegistration>().Do(x => StreamRegistration(x, _client.Connection));
            _server.PacketHandler.On<StreamRegistration>().Do(StreamRegistration);

            _server.Start();
            _server.OnNewConnection += OnNewConnection;

            _client.Connect();
            Console.WriteLine("Connected");
            TypedTest(_client.Connection);

            Console.WriteLine("Test sent");
            Console.ReadLine();
        }

        private static async void OnNewConnection(object sender, ConnectionInfo connection)
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

            var awesome = new MyAwesomeClass
            {
                AwesomeString = "AWESOME!!"
            };
            var streamFile = @"D:\File\FileArchive\Bibliothek\SciFi\Der Wüstenplanet - Die Enzyklopädie Bd. 1 2.epub";

            using (var ha = MD5.Create())
            using (var fs = File.OpenRead(streamFile))
            {
                var hash = ha.ComputeHash(fs);
                var stringHash = string.Concat(hash.Select(x => x.ToString("x2")));
                Console.WriteLine($"Hash is {stringHash}");
            }

            var registrationResult = await connection.Sender.Send<StreamRegistration>(new StreamRegistration());

            connection.Sender.SendStream(registrationResult.Guid, File.OpenRead(streamFile));
            // Console.ReadLine();

            Console.ReadLine();

            _sw = Stopwatch.StartNew();
            while (true)
            {
                //var tList = awesomeList.Select(x => connection.Sender.Send<MyAwesomeClass>(x)).ToList();

                //await Task.WhenAll(tList.Select(x => x.Task));
                //for (int i = 0; i < tList.Count; i++)
                //{
                //    awesomeList[i] = tList[i].Value;
                //}

                //if (awesomeList[0].Packets % 100 == 0)
                //{
                //    _sw.Stop();
                //    var count = awesomeList.Sum(x => x.Packets);

                //    Console.WriteLine($"AVG Delay: {(_sw.Elapsed.TotalMilliseconds / count)} ms " +
                //                      $"{count}:  ({count * 2 / _sw.Elapsed.TotalMilliseconds})");
                //    _sw.Start();
                //}

                //   awesome = srvEp.PacketHandler.Handle(awesome) as MyAwesomeClass;

                awesome = connection.Sender.Send<MyAwesomeClass>(awesome).Result;
                //Or  awesome = await connection.Sender.Send<MyAwesomeClass>(awesome);

                if (awesome.Packets % 3 == 0)
                {
                    _sw.Stop();
                    Console.WriteLine($"AVG Delay: {(_sw.Elapsed.TotalMilliseconds / awesome.Packets)} ms " +
                                      $"{awesome.Packets}:  ({awesome.Packets * 2 / _sw.Elapsed.TotalMilliseconds})");
                    _sw.Start();
                }

                //  await Task.Delay(100);
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

                    Console.WriteLine($"Hash is {stringHash}");
                    handler.Dispose();
                }
            };

            connection.StreamPacketHandler.RegisterStream(streamHandler);
            registration.StatusCode = StatusCode.Ok;
            return registration;
        }
    }

    public enum StatusCode
    {
        Default,
        Ok,
        Failure
    }

    [Serializable]
    public class StreamRegistration
    {
        public StatusCode StatusCode { get; set; }
        public Guid Guid { get; }

        public StreamRegistration() : this(Guid.NewGuid())
        {
        }

        public StreamRegistration(Guid guid)
        {
            Guid = guid;
        }
    }
}