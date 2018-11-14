using System;
using System.Net.Sockets;
using System.Threading;

namespace Fluffy.Net
{
    public class ConnectionInfo : IDisposable
    {
        public EventHandler<ConnectionInfo> OnDisposing;
        public Socket Socket { get; set; }

        public FluffySocket FluffySocket { get; set; }

        internal Receiver Receiver { get; private set; }
        internal Sender Sender { get; private set; }

        public ConnectionInfo(FluffySocket fluffySocket) 
            : this(fluffySocket.Socket,fluffySocket)
        {
                
        }
        public ConnectionInfo(Socket socket, FluffySocket fluffySocket)
        {
            Socket = socket;
            FluffySocket = fluffySocket;

            Receiver = new Receiver(socket);
            Sender = new Sender(this);
        }

        public void Dispose()
        {
            Socket?.Dispose();
            OnDisposing?.Invoke(this, this);
        }
    }

    internal class SendTaskRelay : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public Func<bool> WorkFunc { get; }
        public EventWaitHandle WaitHandle { get; set; }

        public SendTaskRelay(Func<bool> workFunc)
        {
            WorkFunc = workFunc;
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}