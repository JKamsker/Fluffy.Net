using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AllInOneExample_FullFramework.Models;
using Fluffy.IO.Buffer;
using Fluffy.IO.Recycling;
using Fluffy.Net;
using Fluffy.Net.Packets.Modules.Streaming;

namespace AllInOneExample_FullFramework
{
    public class StreamExample
    {
        private const string TestFilePath = @"C:\Users\Weirdo\Downloads\SciFiEbooks\Der Wüstenplanet - Die Enzyklopädie Bd. 1 2.epub";

        private bool _initialized;
        private readonly FluffyServer _server;
        private readonly FluffyClient _client;

        public StreamExample(FluffyServer server, FluffyClient client)
        {
            _server = server;
            _client = client;
        }

        public StreamExample Initialize()
        {
            if (_initialized)
            {
                return this;
            }

            _initialized = true;

            _client.Connection.PacketHandler.On<StreamRegistration>().Do(x => StreamRegistration(x, _client.Connection));
            _server.PacketHandler.On<StreamRegistration>().Do(StreamRegistration);

            _server.OnNewConnection += OnNewConnection;
            return this;
        }

        private async void OnNewConnection(object sender, ConnectionInfo connection)
        {
            await SendFileExampleAsync(connection);
        }

        private static async Task SendFileExampleAsync(ConnectionInfo connection)
        {
            var awesome = new MyAwesomeClass
            {
                AwesomeString = "AWESOME!!"
            };

            var registration = await connection.Sender.Send<StreamRegistration>(new StreamRegistration());
            Console.WriteLine($"Stream id is: {registration.StreamIdentifier}");

            using (var ha = MD5.Create())
            using (var fs = File.OpenRead(TestFilePath))
            {
                var hash = ha.ComputeHash(fs);
                var stringHash = string.Concat(hash.Select(x => x.ToString("x2")));
                Console.WriteLine($"[Sender] Hash is {stringHash}");
            }

            connection.Sender.SendStream(registration.StreamIdentifier, File.OpenRead(TestFilePath));

            while (true)
            {
                awesome = await connection.Sender.Send<MyAwesomeClass>(awesome);
                Console.WriteLine($"{awesome.Packets} Result: {awesome.AwesomeString}");
                await Task.Delay(10);
            }
        }

        private static StreamRegistration StreamRegistration(StreamRegistration registration, ConnectionInfo connection)
        {
            var hashAlgorithm = MD5.Create();
            var buffer = BufferRecyclingMetaFactory<RecyclableBuffer>.MakeFactory(Capacity.Medium).GetBuffer();

            var streamHandler = new DefaultStreamHandler(registration.StreamIdentifier)
            {
                StreamNotificationThreshold = 1024 * 1024 //1KB
            };

            streamHandler.OnReceived += (handler, stream) =>
            {
                int read;
                while ((read = stream.Read(buffer.Value, 0, buffer.Value.Length)) != 0)
                {
                    hashAlgorithm.TransformBlock(buffer.Value, 0, read, null, 0);
                }

                if (handler.HasFinished)
                {
                    hashAlgorithm.TransformFinalBlock(buffer.Value, 0, 0);
                    var stringHash = string.Concat(hashAlgorithm.Hash.Select(x => x.ToString("x2")));

                    Console.WriteLine($"[Receiver] Hash is {stringHash}");
                    buffer.Recycle();
                }
            };

            connection.StreamPacketHandler.RegisterStream(streamHandler);
            registration.StatusCode = StatusCode.Ok;
            return registration;
        }
    }
}