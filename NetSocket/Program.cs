using System.Diagnostics;
using System.IO;
using Fluffy.IO.Buffer;

namespace NetSocket
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var ls = new LinkedStream())
            using (var bw = new StreamWriter(ls) { AutoFlush = true })
            using (var br = new StreamReader(ls))
            {
                bw.Write("abc");
                var xx = br.ReadToEnd();
                Debugger.Break();
            }
        }
    }
}