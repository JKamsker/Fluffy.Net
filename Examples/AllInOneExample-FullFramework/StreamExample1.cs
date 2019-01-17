using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AllInOneExample_FullFramework.Models;
using Fluffy.IO.Buffer;
using Fluffy.IO.Recycling;
using Fluffy.Net;

namespace AllInOneExample_FullFramework
{
    internal class StreamExample1
    {
        private const string TestFilePath = @"F:\File\Downloads\ChromeSetup.exe";

        private readonly FluffyServer _server;
        private readonly FluffyClient _client;
        private bool _initialized;

        public StreamExample1(FluffyServer server, FluffyClient client)
        {
            _server = server;
            _client = client;
        }

        public StreamExample1 Initialize()
        {
            if (_initialized)
            {
                return this;
            }

            _initialized = true;

            _client.Connection.PacketHandler.On<FileStreamRegistration>().Do(x => StreamRegistration(x, _client.Connection));
            _server.PacketHandler.On<FileStreamRegistration>().Do(StreamRegistration);

            _server.OnNewConnection += OnNewConnection;
            return this;
        }

        private async void OnNewConnection(object sender, ConnectionInfo connection)
        {
            await ReceiveFileExample(connection);
        }

        private static async Task ReceiveFileExample(ConnectionInfo connection)
        {
            var fileGuid = Guid.NewGuid(); //Here we want some file guid
            var registration = await connection.Sender.Send<FileStreamRegistration>(new FileStreamRegistration { FileIdentifier = fileGuid });
            if (registration.StatusCode == StatusCode.Ok)
            {
                int read;
                int total = 0;
                var buffer = BufferRecyclingMetaFactory<FluffyBuffer>.MakeFactory(Capacity.Medium).GetBuffer();

                var handler = connection.StreamPacketHandler.RegisterStream(new AsyncStreamHandler(registration.StreamIdentifier));

                Console.WriteLine($"Reading stream...");

                while ((read = await handler.Stream.ReadAsync(buffer.Value, 0, buffer.Value.Length, CancellationToken.None)) != 0)
                {
                    total += read;
                    Console.WriteLine($"Read {read} bytes / {total} total ...");
                }

                buffer.Dispose();
                Console.WriteLine("Finished reading from stream");
            }
            else
            {
                Console.WriteLine($"Something went wrong on the remote end :O");
            }
        }

        private static FileStreamRegistration StreamRegistration(FileStreamRegistration registration, ConnectionInfo connection)
        {
            //Do some logic with the guid
            registration.StreamIdentifier = Guid.NewGuid();
            connection.Sender.SendStream(registration.StreamIdentifier, File.OpenRead(TestFilePath));
            registration.StatusCode = StatusCode.Ok;
            return registration;
        }
    }
}