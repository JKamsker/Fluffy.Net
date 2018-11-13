using System.Net.Sockets;

namespace Fluffy.Net
{
    public abstract class FluffySocket
    {
        private protected Socket Socket;
        internal SharedOutputQueueWorker QueueWorker { get; private set; }

        protected FluffySocket()
        {
            QueueWorker = new SharedOutputQueueWorker();
        }
    }
}