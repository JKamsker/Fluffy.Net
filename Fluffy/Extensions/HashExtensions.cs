using System.Security.Cryptography;

namespace Fluffy.Extensions
{
    public static class HashExtensions
    {
        public static byte[] ToMd5Hash(this byte[] input)
        {
            using (var ha = MD5.Create())
            {
                return ha.ComputeHash(input);
            }
        }
    }
}