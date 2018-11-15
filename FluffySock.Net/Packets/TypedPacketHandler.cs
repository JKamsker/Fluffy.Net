using Fluffy.Fluent;

namespace Fluffy.Net.Packets
{
    public class TypedPacketHandler : TypeSwitch
    {
        private readonly ConnectionInfo _connectionInfo;

        public TypedPacketHandler(ConnectionInfo connectionInfo)
        {
            _connectionInfo = connectionInfo;
        }
    }
}