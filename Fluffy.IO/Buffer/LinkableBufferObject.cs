using Fluffy.IO.Recycling;

namespace Fluffy.IO.Buffer
{
    public class LinkableBuffer : FluffyBuffer
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

        public LinkableBuffer Last()
        {
            if (Next != null)
            {
                return Next.Last();
            }

            return this;
        }

        public virtual LinkableBuffer CreateShadowCopy()
        {
            var result = new LinkableBuffer
            {
                Initiated = this.Initiated,
                Value = this.Value,
                High = this.High,
                Low = this.Low,
                Next = Next?.CreateShadowCopy()
            };

            return result;

            void OnEventHandler(BufferObject<byte> sender, byte[] bytes)
            {
                OnDisposing -= OnEventHandler;
                result?.Dispose();
            }
        }
    }
}