using Fluffy.Net;
using Fluffy.Net.Packets.Modules.Formatted;

using System;
using System.Diagnostics;
using System.Net;

namespace NetSocket
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var server = new FluffyServer(8090);
            var client = new FluffyClient(IPAddress.Loopback, 8090);

            client.Connection.PacketHandler.On<MyAwesomeClass>().Do(Awesome);
            server.PacketHandler.On<MyAwesomeClass>().Do(Awesome);

            server.Start();
            client.Connect();
            Console.WriteLine("Connected");
            TypedTest(client.Connection);
            client.Test();
            client.Test();
            Console.WriteLine("Test sent");
            Console.ReadLine();
        }

        private static Stopwatch _sw;

        private static void TypedTest(ConnectionInfo connection)
        {
            var obj = new MyAwesomeClass
            {
                AwesomeString = "AWESOME!!"
            };

            var res = connection.Sender.Send<MyAwesomeClass>(obj).Value;
            Debugger.Break();
        }

        private static MyAwesomeClass Awesome(MyAwesomeClass awesome)
        {
            if (_sw == null)
            {
                _sw = Stopwatch.StartNew();
            }

            if (awesome.Packets % 300 == 0)
            {
                Console.WriteLine($"{awesome.Packets}:  ({awesome.Packets / _sw.Elapsed.TotalMilliseconds})");
            }

            awesome.Packets++;
            return awesome;
        }
    }
}