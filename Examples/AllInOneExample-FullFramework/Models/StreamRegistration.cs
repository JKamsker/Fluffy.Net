using System;

namespace AllInOneExample_FullFramework.Models
{
    [Serializable]
    public class FileStreamRegistration : StreamRegistration
    {
        public Guid FileIdentifier { get; set; }

        public FileStreamRegistration() : base()
        {
        }

        public FileStreamRegistration(Guid guid) : base(guid)
        {
        }
    }

    [Serializable]
    public class StreamRegistration
    {
        public StatusCode StatusCode { get; set; }
        public Guid? StreamIdentifier { get; set; }

        public StreamRegistration() : this(Guid.NewGuid())
        {
        }

        public StreamRegistration(Guid guid)
        {
            StreamIdentifier = guid;
        }
    }
}