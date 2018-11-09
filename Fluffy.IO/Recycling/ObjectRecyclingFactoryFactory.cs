using Fluffy.IO.Buffer;

using System;
using System.Linq;

namespace Fluffy.IO.Recycling
{
    /// <summary>
    /// Provides an easy way to access a pool of shared buffer objects
    /// </summary>
    public static class BufferRecyclingMetaFactory
    {
        private static BufferRecyclingFactory<LinkableBufferObject<byte>>[] _factories;

        static BufferRecyclingMetaFactory()
        {
            var max = (int)Enum.GetValues(typeof(Capacity)).Cast<Capacity>().Max();
            _factories = new BufferRecyclingFactory<LinkableBufferObject<byte>>[max];
        }

        public static IObjectRecyclingFactory<LinkableBufferObject<byte>> Get(Capacity capacity)
        {
            int iCapacity = (int)capacity;

            if (_factories[iCapacity] == null)
            {
                _factories[iCapacity] = new BufferRecyclingFactory<LinkableBufferObject<byte>>(capacity.ToInt());
            }

            return _factories[iCapacity];
        }
    }
}