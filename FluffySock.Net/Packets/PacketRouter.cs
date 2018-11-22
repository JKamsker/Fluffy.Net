using Fluffy.Fluent;
using Fluffy.Net.Collections;
using Fluffy.Net.Options;
using Fluffy.Net.Packets.Modules.Raw;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Fluffy.Net.Packets
{
    public class PacketRouter : TypeSwitch
    {
        private object _registerLock = new object();

        private readonly ConnectionInfo _connection;

        private Dictionary<int, BasePacketHandler> _packetList;

        internal PacketRouter(ConnectionInfo connectionInfo)
        {
            _connection = connectionInfo;
            _packetList = new Dictionary<int, BasePacketHandler>();
        }

        /// <summary>
        /// Registers a low-level packet
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        public T RegisterPacket<T>() where T : BasePacketHandler, new()
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

            return instance;
        }

        internal void Handle(object sender, OnPacketReceiveEventArgs packet)
        {
            if (_packetList.TryGetValue(packet.OpCode, out var handler))
            {
                // TODO: Test! packet.Options = ParallelismOptions.Sync;
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

        public T GetInstance<T>() where T : BasePacketHandler, new()
        {
            return _packetList.Values.FirstOrDefault(x => x is T) as T;
        }

        public BasePacketHandler GetInstance<T>(T opCode) where T : Enum
        {
            return GetInstance(Convert.ToInt32(opCode));
        }

        public BasePacketHandler GetInstance(int opCode)
        {
            return _packetList[opCode];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleInternal(BasePacketHandler handler, OnPacketReceiveEventArgs packet)
        {
            handler.Handle(packet.Body);
            packet.Body?.Dispose();
        }
    }
}