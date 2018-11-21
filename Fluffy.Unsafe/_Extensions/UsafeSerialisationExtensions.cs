using System;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Fluffy.Unsafe
{
    public static class UsafeSerialisationExtensions
    {
        [System.Security.SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void SerializeTo(this Guid guid, byte[] target, int offset)
        {
            /*
             * Unsafe direct+Array.Clear() 365,2807
             * Unsafe direct 18,0869
             * Unsafe 48,2847
             * Conventional 528,8848
             */
            fixed (byte* b = target)
            {
                var point = b + offset;
                *((Guid*)point) = guid;
            }
        }
    }
}