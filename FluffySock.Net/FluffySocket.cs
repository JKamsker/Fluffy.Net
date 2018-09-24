using System.Net.Sockets;

namespace Fluffy.Net
{
    public abstract class FluffySocket
    {
        private protected Socket Socket;
    }

    public class FluffyClient : FluffySocket
    {
    }
}