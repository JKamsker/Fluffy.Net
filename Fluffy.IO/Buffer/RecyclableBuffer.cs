using System;
using Fluffy.IO.Recycling;

namespace Fluffy.IO.Buffer
{
    public class RecyclableBuffer : FluffyBuffer
    {
        public IRecycler<byte[]> Recycler { get; set; }

        public RecyclableBuffer() : base()
        {
        }

        public RecyclableBuffer(ICapacity cacheSize) : base(cacheSize)
        {
        }

        public RecyclableBuffer(byte[] value) : base(value)
        {
        }

        public override void Initialize(ICapacity capacity)
        {
            if (capacity is IRecycler<byte[]> recycler)
            {
                Recycler = recycler;
            }
            else
            {
                base.Initialize(capacity);
            }
        }

        /// <summary>
        /// <exception cref="AggregateException"/>
        /// </summary>
        public virtual void Recycle()
        {
            if (!TryRecyle())
            {
                throw new AggregateException("Cannot recycle BufferObject");
            }
        }

        /// <summary>
        /// Calls <see cref="Reset"/> and returns buffer to the buffer pool
        /// </summary>
        /// <returns></returns>
        public virtual bool TryRecyle()
        {
            if (Recycler == null || Value == null)
            {
                return false;
            }

            Reset();

            Dispose(true);
            return true;
        }

        private bool _disposing;

        public virtual void Dispose(bool recycle)
        {
            if (_disposing)
            {
                return;
            }
            _disposing = true;

            if (Value != null && recycle)
            {
                Recycler?.Recycle(Value);
            }

            base.Dispose();
        }

        public override void Dispose()
        {
            Dispose(false);
        }
    }
}