using Fluffy.IO.Buffer;

using System.Collections.Concurrent;

namespace Fluffy.IO.Recycling
{
    public class BufferRecyclingFactory<T> : IObjectRecyclingFactory<T>
        where T : IResettable, ICapacityInitiatable, new()
    {
        private readonly int _bufferSize;
        private ConcurrentStack<T> _bufferStack;

        public BufferRecyclingFactory(int bufferSize)
        {
            _bufferSize = bufferSize;
        }

        public T Get()
        {
            if (_bufferStack.TryPop(out var result))
            {
                return result;
            }

            result = new T();
            result.Initiate(_bufferSize);
            return result;
        }

        public void Recycle(T @object)
        {
            @object.Reset();
            _bufferStack.Push(@object);
        }

    }
}