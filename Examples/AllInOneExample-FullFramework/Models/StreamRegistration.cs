using System;

namespace AllInOneExample_FullFramework.Models
{
    [Serializable]
    public class StreamRegistration
    {
        public StatusCode StatusCode { get; set; }
        public Guid Guid { get; }

        public StreamRegistration() : this(Guid.NewGuid())
        {
        }

        public StreamRegistration(Guid guid)
        {
            Guid = guid;
        }
    }
}