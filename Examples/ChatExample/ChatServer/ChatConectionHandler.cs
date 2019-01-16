using System;
using System.Linq;
using ChatSharedComps;
using ChatSharedComps.Login;
using ChatSharedComps.Messaging;
using Fluffy.Net;

namespace ChatServer
{
    internal class ChatConnectionHandler
    {
        private readonly ChatPool _pool;

        public ConnectionInfo ConnectionInfo { get; }

        public bool LoggedIn { get; private set; }
        public string UserName { get; private set; }
        public Guid Identifier => ConnectionInfo.Identifier;

        public ChatConnectionHandler(ConnectionInfo connection, ChatPool pool)
        {
            _pool = pool;
            ConnectionInfo = connection;
            ConnectionInfo.PacketHandler.On<LoginRequest>().Do(DoLogin);
        }

        private LoginResponse DoLogin(LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                return new LoginResponse(false, "Invalid login data");
            }

            if (LoggedIn)
            {
                return new LoginResponse(false, "Already logged in");
            }

            LoggedIn = true;
            UserName = request.Name;
            ConnectionInfo.PacketHandler.On<MessageRequest>().Do(HandleMessage);

            var userNotification = new ChatUserJoined
            {
                Identifier = Identifier,
                UserName = UserName
            };

            var chatUsers = _pool.Connections
                 .Where(x => !string.IsNullOrEmpty(x.UserName))
                 .ToList();

            foreach (var chatUser in chatUsers)
            {
                chatUser.ConnectionInfo.Sender.Send(userNotification);
            }

            return new LoginResponse
            {
                Success = true,
                ChatUsers = chatUsers.Select(x => new ChatUser
                {
                    Identifier = x.Identifier,
                    UserName = x.UserName
                }).ToList()
            };
        }

        private BaseResponse HandleMessage(MessageRequest message)
        {
            if (message == null)
            {
                return new BaseResponse(false);
            }

            switch (message.MessageType)
            {
                case MessageType.BroadCast:
                    _pool.SendMessage(message.Message, x => x.Identifier != Identifier);
                    break;

                case MessageType.MultiCast:
                    if (message.Receipients?.Count >= 0)
                    {
                        _pool.SendMessage(message.Message, x => message.Receipients.Contains(x.Identifier));
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            return new BaseResponse(true);
        }
    }
}