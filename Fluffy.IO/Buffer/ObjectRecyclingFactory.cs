using System;
using System.Collections.Generic;

namespace Fluffy.IO.Buffer
{
    internal class ObjectRecyclingFactory<T>
    {
        private readonly Func<T> _creationFunc;

        private Stack<T> _bufferStack;

        public ObjectRecyclingFactory(Func<T> creationFunc)
        {
            _creationFunc = creationFunc;
            _bufferStack = new Stack<T>();
        }

        public T Get()
        {
            if (_bufferStack.Count == 0)
            {
                return _creationFunc();
            }

            return _bufferStack.Pop();
        }

        public void Recycle(T @object)
        {
            _bufferStack.Push(@object);
        }
    }
}