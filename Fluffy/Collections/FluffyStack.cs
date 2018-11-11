using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Fluffy.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    [ComVisible(true)]
    [Serializable]
    public class FluffyStack<T> : Stack<T>, IProducerConsumerCollection<T>
    {
        public bool TryAdd(T item)
        {
            base.Push(item);
            return true;
        }

        public bool TryTake(out T item)
        {
            if (base.Count == 0)
            {
                item = default;
                return false;
            }

            item = base.Pop();
            return true;
        }
    }
}
