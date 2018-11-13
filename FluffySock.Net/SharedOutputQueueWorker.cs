using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Fluffy.Net
{
    internal class SharedOutputQueueWorker
    {
        private ConcurrentQueue<SendTaskRelay> _queue;
        private ConcurrentQueue<SendTaskRelay> _pushBackQueue;

        private Thread _worker;

        private AutoResetEvent _resetEvent = new AutoResetEvent(false);

        private volatile bool _running;
        public bool Running => _running;

        public SharedOutputQueueWorker()
        {
            _queue = new ConcurrentQueue<SendTaskRelay>();
            _pushBackQueue = new ConcurrentQueue<SendTaskRelay>();
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

        internal void AddTask(SendTaskRelay action)
        {
            action.WaitHandle = _resetEvent;
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
                    sender.WorkFunc();
                    _pushBackQueue.Enqueue(sender);
                }

                var q1 = _queue;
                var q2 = _pushBackQueue;
                _queue = q2;
                _pushBackQueue = q1;

                _resetEvent.WaitOne(TimeSpan.FromMilliseconds(50));
            }

            _running = false;
        }
    }
}