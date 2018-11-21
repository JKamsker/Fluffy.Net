using System;

namespace Fluffy.Unsafe
{
    public static class FluffyBitConverter
    {
        [System.Security.SecuritySafeCritical]  // auto-generated
        public static unsafe void GetBytes(long value, byte[] target, int offset)
        {
            if (target.Length + offset < 8)
            {
                throw new ArgumentException("Target not long enough");
            }

            fixed (byte* b = target)
            {
                *((long*)(b + offset)) = value;
            }
        }
    }
}