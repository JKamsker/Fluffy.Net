using System;
using System.Collections.Concurrent;
using Fluffy.IO.Buffer;

namespace Fluffy.IO.Recycling
{
    internal class ByteArrayRecycler : IObjectRecyclingFactory<byte[]>, ICapacity
    {
        public int Capacity { get; }
        private IProducerConsumerCollection<byte[]> _bufferStack;

        public ByteArrayRecycler(int bufferSize)
        {
            Capacity = bufferSize;
            _bufferStack = new ConcurrentStack<byte[]>();
        }

        public ByteArrayRecycler Initialize<TCollectionType>()
            where TCollectionType : IProducerConsumerCollection<byte[]>, new()
        {
            _bufferStack = new TCollectionType();
            return this;
        }

        public byte[] Get()
        {
            if (_bufferStack == null)
            {
                throw new AggregateException("Factory is not initialized");
            }
            if (_bufferStack.TryTake(out var result))
            {
                return result;
            }

            return new byte[Capacity];
        }

        public void Recycle(byte[] array)
        {
            if (_bufferStack == null)
            {
                throw new AggregateException("Factory is not initialized");
            }

            if (array?.Length != Capacity)
            {
                throw new AggregateException("Invalid object");
            }

            _bufferStack.TryAdd(array);
        }
    }
}