using Fluffy.Net;

using System;
using System.Net;

namespace AllInOneExample_FullFramework
{
    internal class Program
    {
        private static FluffyServer _server;
        private static FluffyClient _client;

        private static PacketExample _packetExample;
        private static StreamExample _streamExample;
        private static StreamExample1 _streamExample1;

        private static void Main(string[] args)
        {
            _server = new FluffyServer(8090);
            _client = new FluffyClient(IPAddress.Loopback, 8090);
            _server.Start();

            //  _packetExample = new PacketExample(_server, _client).Initialize();
            // _streamExample = new StreamExample(_server, _client).Initialize();
           _streamExample1 = new StreamExample1(_server, _client).Initialize();

            _client.Connect();
            Console.WriteLine("Connected");
            Console.ReadLine();
        }
    }
}