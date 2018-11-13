using System;
using Fluffy.IO.Recycling;

namespace Fluffy.IO.Buffer
{
    public class LinkableBufferObject<T> : BufferObject<T>
    {
        public IRecycler<LinkableBufferObject<T>> Recycler { get; private set; }

        public LinkableBufferObject<T> Next { get; internal set; }

        public LinkableBufferObject() : base()
        {
        }

        public LinkableBufferObject(ICapacity cacheSize) : base(cacheSize)
        {
        }

        public override void Initiate(ICapacity capacity)
        {
            if (capacity is IRecycler<LinkableBufferObject<T>> bufferFactory)
            {
                Recycler = bufferFactory;
            }

            base.Initiate(capacity);
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
            if (Recycler == null)
            {
                return false;
            }
            Reset();
            Recycler.Recycle(this);
            return true;
        }

        public override void Reset()
        {
            Next = null;
            base.Reset();
        }
    }
}