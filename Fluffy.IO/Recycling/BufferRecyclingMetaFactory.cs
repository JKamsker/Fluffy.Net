using Fluffy.IO.Buffer;

using System;
using System.Linq;

namespace Fluffy.IO.Recycling
{
    public static class BufferRecyclingMetaFactory
    {
        internal static int MaxCapacity;
        internal static ByteArrayRecycler[] Recyclers;

        static BufferRecyclingMetaFactory()
        {
            MaxCapacity = (int)Enum.GetValues(typeof(Capacity)).Cast<Capacity>().Max() + 1;
            Recyclers = new ByteArrayRecycler[MaxCapacity];
        }

        public static IObjectRecyclingFactory<LinkableBuffer> Get(Capacity capacity)
        {
            return BufferRecyclingMetaFactory<LinkableBuffer>.MakeFactory(capacity);
        }
    }

    /// <summary>
    /// Provides an easy way to access a pool of shared buffer objects
    /// </summary>
    public static class BufferRecyclingMetaFactory<T>
        where T : FluffyBuffer, IResettable, ICapacityInitiatable, new()
    {
        private static BufferRecyclingFactory<T>[] _factories;
        private static ByteArrayRecycler[] _recyclers;

        static BufferRecyclingMetaFactory()
        {
            _factories = new BufferRecyclingFactory<T>[BufferRecyclingMetaFactory.MaxCapacity];
            _recyclers = BufferRecyclingMetaFactory.Recyclers;
        }

        public static IObjectRecyclingFactory<T> MakeFactory(Capacity capacity)
        {
            int iCapacity = (int)capacity;

            if (_recyclers[iCapacity] == null)
            {
                _recyclers[iCapacity] = new ByteArrayRecycler(capacity.ToInt());
            }

            if (_factories[iCapacity] == null)
            {
                _factories[iCapacity] = new BufferRecyclingFactory<T>(_recyclers[iCapacity]);
            }

            return _factories[iCapacity];
        }
    }
}