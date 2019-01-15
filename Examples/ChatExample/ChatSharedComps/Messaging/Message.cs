using System;

namespace ChatSharedComps.Messaging
{
    [Serializable]
    public class Message
    {
        public Guid Sender { get; set; }
        public string Text { get; set; }
    }
}