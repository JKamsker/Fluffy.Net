using System;
using System.Security.Permissions;


namespace Playground.Core
{
    // [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true, Synchronization = true)]
    [Serializable]
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
