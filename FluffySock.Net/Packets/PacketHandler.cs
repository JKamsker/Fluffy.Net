using Fluffy.Net.Collections;
using Fluffy.Net.Options;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fluffy.Net.Packets
{
    public class PacketHandler
    {
        private object _registerLock = new object();

        private readonly ConnectionInfo _connection;

        private Dictionary<int, BasePacket> _packetList;

        internal PacketHandler(ConnectionInfo connectionInfo)
        {
            _connection = connectionInfo;
            _packetList = new Dictionary<int, BasePacket>();
        }

        public void RegisterPacket<T>() where T : BasePacket, new()
        {
            var instance = new T()
            {
                Connection = _connection
            };

            if (instance.OpCode == 0)
            {
                throw new NotImplementedException($"{nameof(T)} is not implemented correctly (Invalid OpCode)");
            }

            lock (_registerLock)
            {
                if (!_packetList.ContainsKey(instance.OpCode))
                {
                    _packetList[instance.OpCode] = instance;
                }
                else
                {
                    throw new AccessViolationException("Packet already defined");
                }
            }
        }

        internal void Handle(object sender, OnPacketReceiveEventArgs packet)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            var handler = _packetList[packet.OpCode];
            switch (packet.Options)
            {
                case ParallelismOptions.Parallel:
                    Task.Run(() => handler.Handle(packet.Body)).ContinueWith(x => packet.Body?.Dispose());
                    break;

                case ParallelismOptions.Sync:
                    handler.Handle(packet.Body);
                    packet.Body?.Dispose();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}