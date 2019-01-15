using System;
using Fluffy.Net;

namespace ChatServer
{
    internal class Program
    {
        private static FluffyServer _server;

        private static ChatPool _chatPool;

        private static void Main(string[] args)
        {
            _chatPool = new ChatPool();
            _server = new FluffyServer(8001);
            _server.OnNewConnection += OnNewConnection;

            _server.Start();
            Console.WriteLine("Server started, press [EXIT] to exit");
            Console.ReadLine();
            Console.ReadLine();
        }

        private static void OnNewConnection(object sender, ConnectionInfo e)
        {
            _chatPool.Add(e);
            e.OnDisposing += OnClientDisconnecting;

            Console.WriteLine($"Client joined: {e.Identifier}/{e.Socket.RemoteEndPoint}");
        }

        private static void OnClientDisconnecting(object sender, ConnectionInfo e)
        {
            _chatPool.Remove(e);
            e.OnDisposing -= OnClientDisconnecting;

            Console.WriteLine($"Client left: {e.Identifier}/{e.Socket.RemoteEndPoint}");
        }
    }
}