using Fluffy.Extensions;
using Fluffy.IO.Buffer;
using Fluffy.Net.Packets;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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

        public Task ConnectAsync()
        {
            return Socket.ConnectAsync(_endPoint);
        }

        public void Test()
        {
            var str = new LinkedStream();
            var writeBuf = Encoding.UTF8.GetBytes("Hello World");
            str.Write(writeBuf, 0, writeBuf.Length);
            _connection.Sender.Send(Packet.TestPacket, str);
        }

        public void TypedTest()
        {
            var str = new LinkedStream();
            var obj = new MyAwesomeClass()
            {
                AwesomeString = "AWESOME!!"
            };

            var writeBuf = obj.Serialize();
            str.Write(writeBuf, 0, writeBuf.Length);
            _connection.Sender.Send(Packet.FormattedPacket, str);
        }
    }
}