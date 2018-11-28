using System;
using System.Threading;

namespace Fluffy.Net.Async
{
    internal class SendTaskRelay : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public Func<bool> WorkFunc { get; }
        public EventWaitHandle WaitHandle { get; set; }

        public SendTaskRelay(Func<bool> workFunc)
        {
            WorkFunc = workFunc;
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}