using Fluffy.Fluent;
using Fluffy.Net.Collections;
using Fluffy.Net.Options;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Fluffy.Net.Packets.Modules.Raw;

namespace Fluffy.Net.Packets
{
    public class PacketRouter : TypeSwitch
    {
        private object _registerLock = new object();

        private readonly ConnectionInfo _connection;

        private Dictionary<int, BasePacket> _packetList;

        internal PacketRouter(ConnectionInfo connectionInfo)
        {
            _connection = connectionInfo;
            _packetList = new Dictionary<int, BasePacket>();
        }

        /// <summary>
        /// Registers a low-level packet
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        public void RegisterPacket<T>() where T : BasePacket, new()
        {
            var instance = new T
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
            if (_packetList.TryGetValue(packet.OpCode, out var handler))
            {
                //TODO: Remove
                packet.Options = ParallelismOptions.Sync;
                switch (packet.Options)
                {
                    case ParallelismOptions.Parallel:
                        Task.Run(() => HandleInternal(handler, packet));
                        break;

                    case ParallelismOptions.Sync:
                        HandleInternal(handler, packet);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleInternal(BasePacket handler, OnPacketReceiveEventArgs packet)
        {
            handler.Handle(packet.Body);
            packet.Body?.Dispose();
        }
    }
}