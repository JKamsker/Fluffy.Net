using System.Net.Sockets;

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