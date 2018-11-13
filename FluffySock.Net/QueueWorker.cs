using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Fluffy.Net
{
    internal class SharedOutputQueueWorker
    {
        private ConcurrentQueue<AsyncSender> _queue;
        private ConcurrentQueue<AsyncSender> _pushBackQueue;

        private Thread _worker;

        private AutoResetEvent _resetEvent = new AutoResetEvent(false);

        private volatile bool _running;
        public bool Running => _running;

        public SharedOutputQueueWorker()
        {
            _queue = new ConcurrentQueue<AsyncSender>();
            _pushBackQueue = new ConcurrentQueue<AsyncSender>();
            _worker = new Thread(ThreadWorker);
        }

        public SharedOutputQueueWorker StartWorker()
        {
            if (!_running)
            {
                _worker.Start();
            }
            return this;
        }

        internal void Add(AsyncSender action)
        {
            _queue.Enqueue(action);
        }

        private void ThreadWorker()
        {
            if (_running)
            {
                return;
            }

            _running = true;
            while (_running)
            {
                while (_queue.TryDequeue(out var sender))
                {
                    if (sender.IsDisposed)
                    {
                        continue;
                    }
                    sender.DoWork();
                    _pushBackQueue.Enqueue(sender);
                }

                var q1 = _queue;
                var q2 = _pushBackQueue;
                _queue = q2;
                _pushBackQueue = q1;

                _resetEvent.WaitOne(TimeSpan.FromMilliseconds(10));
            }

            _running = false;
        }
    }
}