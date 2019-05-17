using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Permissions;

namespace Fluffy.Collections
{
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
#if (!NETSTANDARD2_0)
     [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true, Synchronization = true)]
#endif

    public class FluffyConcurrentStack<T> : ConcurrentStack<T> , IProducerConsumerCollection<T>
    {

    }
}