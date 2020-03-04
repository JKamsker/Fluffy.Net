using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fluffy.Unsafe
{
    public static class FluffyBitConverter
    {


#if !(NET35 || NET40)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe int Serialize<T>(T value, byte[] target, int offset) where T : unmanaged
        {
            var size = sizeof(T);
            if (target.Length + offset < size)
            {
                throw new ArgumentException("Target not long enough");
            }


            fixed (byte* b = target)
            {
                var point = b + offset;
                *((T*)point) = value;
            }

            return size;
        }

       

        public static unsafe int SizeOf<T>() where T : unmanaged
        {
            return sizeof(T);
        }

        //[System.Security.SecuritySafeCritical]  // auto-generated
        //[System.Security.SecuritySafeCritical]  // auto-generated
        //public static unsafe void Serialize(long value, byte[] target, int offset)
        //{
        //    if (target.Length + offset < 8)
        //    {
        //        throw new ArgumentException("Target not long enough");
        //    }

        //    fixed (byte* b = target)
        //    {
        //        *((long*)(b + offset)) = value;
        //    }
        //}

        //[System.Security.SecuritySafeCritical]  // auto-generated
        //public static unsafe void Serialize(int value, byte[] target, int offset)
        //{
        //    if (target.Length + offset < 4)
        //    {
        //        throw new ArgumentException("Target not long enough");
        //    }

        //    fixed (byte* b = target)
        //    {
        //        *((int*)(b + offset)) = value;
        //    }
        //}

        //        [System.Security.SecuritySafeCritical]
        //#if !(NET35 || NET40)
        //        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //#endif
        //public static unsafe void Serialize(Guid value, byte[] target, int offset)
        //{
        //    /* 1 Million runs:
        //     * Unsafe direct+Array.Clear() 365,2807
        //     * Unsafe direct 18,0869
        //     * Unsafe 48,2847
        //     * Conventional sequence.Equal 528,8848
        //     */
        //    if (target.Length + offset < 8)
        //    {
        //        throw new ArgumentException("Target not long enough");
        //    }

        //    fixed (byte* b = target)
        //    {
        //        var point = b + offset;
        //        *((Guid*)point) = value;
        //    }
        //}
        ///// <summary>
        ///// Reads in a block from a file and converts it to the struct
        ///// type specified by the template parameter
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="reader"></param>
        ///// <returns></returns>
        //public static T FromBytes<T>(byte[] bytes)
        //{
        //    // Pin the managed memory while, copy it out the data, then unpin it
        //    GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        //    T theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        //    handle.Free();

        //    return theStructure;
        //}


        //public static T ReadUsingMarshalUnsafe<T>(byte[] data) where T : struct
        //{
        //    unsafe
        //    {
        //        fixed (byte* p = &data[0])
        //        {
        //            return (T)Marshal.PtrToStructure(new IntPtr(p), typeof(T));
        //        }
        //    }
        //}

        //public unsafe static byte[] WriteUsingMarshalUnsafe<selectedT>(selectedT structure) where selectedT : struct
        //{
        //    byte[] byteArray = new byte[Marshal.SizeOf(structure)];
        //    fixed (byte* byteArrayPtr = byteArray)
        //    {
        //        Marshal.StructureToPtr(structure, (IntPtr)byteArrayPtr, true);
        //    }
        //    return byteArray;
        //}

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