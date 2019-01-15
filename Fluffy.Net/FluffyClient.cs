using Fluffy.IO.Buffer;

using Fluffy.Net.Packets.Modules.Raw;

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

        public FluffyClient(IPAddress address, int port) : base(nameof(FluffyClient))
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
            return Socket.ConnectAsync(_endPoint).ContinueWith(x => Connection.Receiver.Start());
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