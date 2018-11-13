using Fluffy.IO.Recycling;

namespace Fluffy.IO.Buffer
{
    public class LinkableBuffer : RecyclableBuffer
    {
        public LinkableBuffer Next { get; internal set; }

        public LinkableBuffer() : base()
        {
        }

        public LinkableBuffer(ICapacity cacheSize) : base(cacheSize)
        {
        }

        public LinkableBuffer(byte[] value) : base(value)
        {
        }

        public override void Reset()
        {
            Next = null;
            base.Reset();
        }
    }
}