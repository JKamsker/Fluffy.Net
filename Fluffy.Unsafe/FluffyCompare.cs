using System;
using System.Runtime.InteropServices;

namespace Fluffy.Unsafe
{
    public static class FluffyCompare
    {
        /// <summary>
        /// Remark: Cannot test this bit of code because i don't know how to get an untrusted environment
        /// </summary>
        static FluffyCompare()
        {
            if (AppDomain.CurrentDomain.IsFullyTrusted)
            {
                ByteArrayCompare = UnsafeCompare;
            }
            else
            {
                ByteArrayCompare = SafeCompare;
            }
        }

        public static Func<byte[], byte[], bool> ByteArrayCompare { get; }

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int memcmp(byte[] b1, byte[] b2, long count);

        public static bool SafeCompare(byte[] b1, byte[] b2)
        {
            // Validate buffers are the same length. This also ensures that the count does not exceed
            // the length of either buffer.
            return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
        }

        // Copyright (c) 2008-2013 Hafthor Stefansson Distributed under the MIT/X11 software license
        // Ref: http://www.opensource.org/licenses/mit-license.php.
        [System.Security.SecuritySafeCritical]
        private static unsafe bool UnsafeCompare(byte[] a1, byte[] a2)
        {
            if (a1 == a2)
            {
                return true;
            }

            if (a1 == null || a2 == null || a1.Length != a2.Length)
            {
                return false;
            }

            fixed (byte* p1 = a1, p2 = a2)
            {
                byte* x1 = p1, x2 = p2;
                int l = a1.Length;
                for (int i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
                {
                    if (*((long*)x1) != *((long*)x2))
                    {
                        return false;
                    }
                }

                if ((l & 4) != 0)
                {
                    if (*((int*)x1) != *((int*)x2))
                    {
                        return false;
                    }
                    x1 += 4; x2 += 4;
                }

                if ((l & 2) != 0)
                {
                    if (*((short*)x1) != *((short*)x2))
                    {
                        return false;
                    }
                    x1 += 2; x2 += 2;
                }
                if ((l & 1) != 0)
                {
                    if (*((byte*)x1) != *((byte*)x2))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}