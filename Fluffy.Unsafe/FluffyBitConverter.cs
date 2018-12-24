using System;
using System.Runtime.CompilerServices;

namespace Fluffy.Unsafe
{
    public static class FluffyBitConverter
    {
        [System.Security.SecuritySafeCritical]  // auto-generated
        public static unsafe void Serialize(long value, byte[] target, int offset)
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

        [System.Security.SecuritySafeCritical]  // auto-generated
        public static unsafe void Serialize(int value, byte[] target, int offset)
        {
            if (target.Length + offset < 4)
            {
                throw new ArgumentException("Target not long enough");
            }

            fixed (byte* b = target)
            {
                *((int*)(b + offset)) = value;
            }
        }

        [System.Security.SecuritySafeCritical]
#if !(NET35 || NET40)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe void Serialize(Guid value, byte[] target, int offset)
        {
            /* 1 Million runs:
             * Unsafe direct+Array.Clear() 365,2807
             * Unsafe direct 18,0869
             * Unsafe 48,2847
             * Conventional sequence.Equal 528,8848
             */
            if (target.Length + offset < 8)
            {
                throw new ArgumentException("Target not long enough");
            }

            fixed (byte* b = target)
            {
                var point = b + offset;
                *((Guid*)point) = value;
            }
        }

        //public static void Serialize2(Guid str, byte[] target, int offset)
        //{
        //    const int size = 16;//Marshal.SizeOf(str);
        //    //byte[] arr = new byte[size];

        // IntPtr ptr = Marshal.AllocHGlobal(size); Marshal.StructureToPtr(str, ptr, true);

        //    Marshal.Copy(ptr, target, offset, size);
        //    Marshal.FreeHGlobal(ptr);
        //}
    }
}