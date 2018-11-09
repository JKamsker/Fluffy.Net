using Fluffy.IO.Buffer;

using System;

namespace Fluffy.IO.Recycling
{
    public enum Capacity
    {
        /// <summary>
        /// 1 KB
        /// </summary>
        ExtraSmall,

        /// <summary>
        /// 4KB
        /// </summary>
        Small,

        /// <summary>
        /// 4 KB
        /// </summary>
        Medium,

        /// <summary>
        /// 8 KB
        /// </summary>
        Big
    }

    /// <summary>
    /// Provides an easy way to access a pool of shared buffer objects
    /// </summary>
    public static class BufferRecyclingMetaFactory
    {
        public static IObjectRecyclingFactory<LinkableBufferObject<byte>> Get(Capacity capacity)
        {
            int size = 32;
            switch (capacity)
            {
                case Capacity.ExtraSmall:
                    size = 1 * 1024;
                    break;

                case Capacity.Small:
                    size = 2 * 1024;
                    break;

                case Capacity.Medium:
                    size = 4 * 1024;

                    break;

                case Capacity.Big:
                    size = 8 * 1024;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(capacity), capacity, null);
            }
            return new BufferRecyclingFactory<LinkableBufferObject<byte>>(size);
        }
    }
}