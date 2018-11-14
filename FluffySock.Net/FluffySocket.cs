using System.Net.Sockets;
using Fluffy.Net.Async;

namespace Fluffy.Net
{
    public abstract class FluffySocket
    {
        protected internal Socket Socket;
        internal SharedOutputQueueWorker QueueWorker { get; private set; }

        protected FluffySocket()
        {
            QueueWorker = new SharedOutputQueueWorker();
        }
    }
}