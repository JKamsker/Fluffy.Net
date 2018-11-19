using Fluffy.IO.Buffer;
using Fluffy.Net.Packets.Raw;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Fluffy.Net
{
    public class FluffyClient : FluffySocket
    {
        public ConnectionInfo Connection { get; }

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

            Connection = new ConnectionInfo(Socket, this);

            // _connection.TypedPacketHandler.
        }

        public void Connect()
        {
            Socket.Blocking = true;
            Socket.Connect(_endPoint);
            Socket.Blocking = false;

            Connection.Receiver.Start();
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
            Connection.Sender.Send(PacketTypes.TestPacket, str);
        }
    }
}