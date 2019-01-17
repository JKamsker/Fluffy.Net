using System;
using Fluffy.Net;
using Stresstest.SharedComps;

namespace Stresstest.Server
{
    internal class Program
    {
        private static FluffyServer _server;

        private static void Main(string[] args)
        {
            _server = new FluffyServer(8092);
            _server.PacketHandler.On<Dummy>().Do(HandleDummy);
            _server.Start();
            Console.WriteLine("Socket started");
            Console.ReadLine();
        }

        private static Dummy HandleDummy(Dummy arg1, ConnectionInfo arg2)
        {
            arg1.Counter++;
            return arg1;
        }
    }
}