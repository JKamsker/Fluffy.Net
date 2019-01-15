using System;

namespace ChatSharedComps
{
    [Serializable]
    public class ChatUser
    {
        public Guid Identifier { get; set; }
        public string UserName { get; set; }
    }

    [Serializable]
    public class ChatUserJoined : ChatUser
    {
    }

    [Serializable]
    public class ChatUserLeft : ChatUser
    {
    }
}