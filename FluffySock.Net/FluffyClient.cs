using System.Net;
using System.Net.Sockets;
using Fluffy.IO.Buffer;
using Fluffy.Net.Options;

namespace Fluffy.Net
{
    public class FluffyClient : FluffySocket
    {
        private ConnectionInfo _connection;
        private IPEndPoint _endPoint;

        public FluffyClient(IPAddress address, int port)
        {
            _endPoint = new IPEndPoint(address, port);
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true,
                Blocking = false,
                ReceiveTimeout = int.MaxValue,
                SendTimeout = int.MaxValue,
            };

            _connection = new ConnectionInfo(Socket, this);
        }

        public void Connect()
        {
            Socket.Blocking = true;
            Socket.Connect(_endPoint);
            Socket.Blocking = false;

            _connection.Receiver.Start();
        }

        public void Test()
        {
            var str = new LinkedStream();
            str.Write(new byte[] { 1, 2, 3, 4 }, 0, 3);
            _connection.Sender.Send(DynamicMethodDummy.Test1, str, ParallelismOptions.Parallel);
        }
    }
}