using Fluffy.IO.Buffer;

using System;
using System.Collections.Concurrent;

namespace Fluffy.IO.Recycling
{
    public class ConcurrentObjectRecyclingFactory<T> : IObjectRecyclingFactory<T>
    {
        private readonly Func<T> _creationFunc;

        private ConcurrentStack<T> _bufferStack;

        public ConcurrentObjectRecyclingFactory(Func<T> creationFunc)
        {
            _creationFunc = creationFunc;
            _bufferStack = new ConcurrentStack<T>();
        }

        public T Get()
        {
            if (_bufferStack.TryPop(out var result))
            {
                return result;
            }

            return _creationFunc();
        }

        public void Recycle(T @object)
        {
            _bufferStack.Push(@object);
        }
    }
}