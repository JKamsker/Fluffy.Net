using System;
using System.Net.Sockets;

namespace Fluffy.Net
{
    internal class ConnectionInfo : IDisposable
    {
        public Socket Socket { get; set; }

        public FluffySocket FluffySocket { get; set; }

        internal IOHandler IOHandler { get; set; }

        public ConnectionInfo(Socket socket, FluffySocket fluffySocket)
        {
            Socket = socket;
            FluffySocket = fluffySocket;

            IOHandler = new IOHandler(socket).Start();
        }

        public void Dispose()
        {
            Socket?.Dispose();
        }
    }
}