using Fluffy.IO.Recycling;

namespace Fluffy.IO.Buffer
{
    public class FluffyBuffer : BufferObject<byte>
    {
        public FluffyBuffer() : base()
        {
        }

        public FluffyBuffer(byte[] value) : base(value)
        {
        }

        public FluffyBuffer(ICapacity cacheSize) : base(cacheSize)
        {
        }
    }
}