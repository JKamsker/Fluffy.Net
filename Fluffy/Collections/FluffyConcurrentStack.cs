using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Permissions;

namespace Fluffy.Collections
{
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true, Synchronization = true)]
    public class FluffyConcurrentStack<T> : ConcurrentStack<T> , IProducerConsumerCollection<T>
    {

    }
}