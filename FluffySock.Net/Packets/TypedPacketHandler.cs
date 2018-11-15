namespace Fluffy.Net.Packets
{
    internal class TypedPacketHandler
    {
        private readonly ConnectionInfo _connectionInfo;

        public TypedPacketHandler(ConnectionInfo connectionInfo)
        {
            _connectionInfo = connectionInfo;
        }
    }
}