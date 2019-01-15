using System.Collections.Generic;
using ChatSharedComps;
using Fluffy.Net;

namespace ChatClient.ViewModels
{
    internal class MainViewModel
    {
        private readonly FluffyClient _client;
        private readonly List<ChatUser> _chatUsers;

        public MainViewModel(FluffyClient client, List<ChatUser> chatUsers)
        {
            _client = client;
            _chatUsers = chatUsers;
        }
    }
}