using System;
using System.Collections;
using Fluffy.IO.Buffer;

using System.Collections.Concurrent;

namespace Fluffy.IO.Recycling
{
    public class BufferRecyclingFactory<T> : IObjectRecyclingFactory<T>
        where T : IResettable, ICapacityInitiatable, new()
    {
        private readonly int _bufferSize;
        private IProducerConsumerCollection<T> _bufferStack;

        public BufferRecyclingFactory(int bufferSize)
        {
            _bufferSize = bufferSize;
        }

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
            result.Initiate(_bufferSize);
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
}