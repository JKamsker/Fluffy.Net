using System;
using Fluffy.IO.Buffer;

using System.Collections.Concurrent;

namespace Fluffy.IO.Recycling
{
    public class BufferRecyclingFactory<T> : IObjectRecyclingFactory<T>, ICapacity, IRecycler<T>
        where T : IResettable, ICapacityInitiatable, new()
    {
        private readonly int _bufferSize;
        private IProducerConsumerCollection<T> _bufferStack;

        public BufferRecyclingFactory(int bufferSize)
        {
            _bufferSize = bufferSize;
        }

        public int Capacity => _bufferSize;

        public BufferRecyclingFactory<T> Initialize<TCollectionType>()
            where TCollectionType : IProducerConsumerCollection<T>, new()
        {
            _bufferStack = new TCollectionType();
            return this;
        }

        public T Get()
        {
            if (_bufferStack == null)
            {
                throw new AggregateException("Factory is not initialized");
            }

            if (_bufferStack.TryTake(out var result))
            {
                return result;
            }

            result = new T();
            result.Initiate(this);
            return result;
        }

        public void Recycle(T @object)
        {
            if (_bufferStack == null)
            {
                throw new AggregateException("Factory is not initialized");
            }

            @object.Reset();
            _bufferStack.TryAdd(@object);
        }
    }

    public interface IRecycler<T>
    {
        void Recycle(T @object);
    }
}