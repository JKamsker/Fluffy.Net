using System;
using System.Collections.Generic;
using System.Linq;
using Fluffy.Net;

namespace ChatServer
{
    internal class ChatPool
    {
        internal IReadOnlyCollection<ChatConnectionHandler> Connections => _connections;
        private List<ChatConnectionHandler> _connections;
        private object _synchronizationObject;

        public ChatPool()
        {
            _connections = new List<ChatConnectionHandler>();
            _synchronizationObject = new object();
        }

        public void Add(ConnectionInfo connection)
        {
            lock (_synchronizationObject)
            {
                _connections.Add(new ChatConnectionHandler(connection, this));
            }
        }

        public void Remove(ConnectionInfo connection)
        {
            lock (_synchronizationObject)
            {
                _connections.RemoveAll(x => x.Identifier == connection.Identifier);
            }
        }

        public void SendMessage(object message, Predicate<ConnectionInfo> predicate = null)
        {
            foreach (var connection in _connections.Select(x => x.ConnectionInfo))
            {
                if (predicate?.Invoke(connection) ?? true)
                {
                    connection.Sender.Send(message);
                }
            }
        }
    }
}