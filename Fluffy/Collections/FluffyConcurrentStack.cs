using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Permissions;

namespace Fluffy.Collections
{
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
#if (!NETCOREAPP2_2 && !NETSTANDARD21)
    [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true, Synchronization = true)]
#endif

    public class FluffyConcurrentStack<T> : ConcurrentStack<T> , IProducerConsumerCollection<T>
    {

    }
}