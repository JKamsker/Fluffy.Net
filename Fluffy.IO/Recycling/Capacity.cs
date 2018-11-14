using System;

namespace Fluffy.IO.Recycling
{
    public enum Capacity
    {
        /// <summary>
        /// 512 B
        /// </summary>
        ExtraSmall,
        /// <summary>
        /// 1 KB
        /// </summary>
        Smaller,

        /// <summary>
        /// 2KB
        /// </summary>
        Small,

        /// <summary>
        /// 4 KB
        /// </summary>
        Medium,

        /// <summary>
        /// 8 KB
        /// </summary>
        Big,

        /// <summary>
        /// 16 KB
        /// </summary>
        ExtraBig
    }

    public static class CapacityExtension
    {
        public static int ToInt(this Capacity capacity)
        {
            int size = 32;

            switch (capacity)
            {
                case Capacity.ExtraSmall:
                    size = 512;
                    break;

                case Capacity.Smaller:
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

                case Capacity.ExtraBig:
                    size = 16 * 1024;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(capacity), capacity, null);
            }

            return size;
        }
    }
}