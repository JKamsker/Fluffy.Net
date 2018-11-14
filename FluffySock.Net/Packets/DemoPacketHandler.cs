using Fluffy.IO.Buffer;

namespace Fluffy.Net.Packets
{
    public abstract class BasePacketHandler
    {
        public abstract byte OpCode { get; }

        private protected ConnectionInfo _connection;

        protected BasePacketHandler(ConnectionInfo connection)
        {
            _connection = connection;
            OpCode = 0;
        }

        public abstract void Handle(LinkedStream stream);
    }
}