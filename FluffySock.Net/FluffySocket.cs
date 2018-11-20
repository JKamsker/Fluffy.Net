using Fluffy.Net.Async;

using System.Net.Sockets;

namespace Fluffy.Net
{
    public abstract class FluffySocket
    {
        protected internal Socket Socket;
        internal SharedOutputQueueWorker QueueWorker { get; private set; }

        protected FluffySocket(string socketName = "")
        {
            QueueWorker = new SharedOutputQueueWorker(socketName);
        }
    }
}