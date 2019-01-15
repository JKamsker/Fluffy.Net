using System;
using System.Collections.Generic;

namespace ChatSharedComps.Messaging
{
    [Serializable]
    public class MessageRequest
    {
        public MessageType MessageType { get; set; }
        public List<Guid> Receipients { get; set; }
        public Message Message { get; set; }
    }
}