using System;
using System.Threading;
using System.Threading.Tasks;
using Fluffy.IO.Buffer;
using Fluffy.IO.Recycling;

namespace AllInOneExample_FullFramework
{
    public class AsyncLinkedStream : LinkedStream, IDisposable
    {
        private AsyncReadHelper _readHandle;

        #region Useless Constructors

        public AsyncLinkedStream() : base()
        {
        }

        public AsyncLinkedStream(int cacheSize) : base(cacheSize)
        {
        }

        public AsyncLinkedStream(Capacity capacity) : base(capacity)
        {
        }

        public AsyncLinkedStream(IObjectRecyclingFactory<LinkableBuffer> recyclingFactory) : base(recyclingFactory)
        {
        }

        #endregion Useless Constructors

        protected override long InternalLength
        {
            get => base.InternalLength;
            set
            {
                if (base.InternalLength == value)
                {
                    return;
                }
                base.InternalLength = value;

                DoReadAsync();
            }
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_readHandle != null)
            {
                throw new AggregateException("Cannot read two times at once from the stream");
            }

            if (Length >= count || IsDisposed)
            {
                var readCount = Read(buffer, offset, count);
                return Task.FromResult(readCount);
            }

            _readHandle = new AsyncReadHelper
            {
                Buffer = buffer,
                Count = count,
                Offset = offset,
                TaskCompletionSource = new TaskCompletionSource<int>(),
                CancellationToken = cancellationToken
            };

            return _readHandle.TaskCompletionSource.Task;
        }

        private void DoReadAsync(bool disposing = false)
        {
            if (_readHandle == null)
            {
                return;
            }

            if (Length >= _readHandle.Count || disposing)
            {
                var handle = Interlocked.Exchange(ref _readHandle, null);

                if (handle.CancellationToken.IsCancellationRequested)
                {
                    // handle.TaskCompletionSource.Task.

#if (NET46 || NET47)
                    //https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskcompletionsource-1.trysetcanceled?view=netframework-4.6
                    handle.TaskCompletionSource.TrySetCanceled(handle.CancellationToken);
#else
                    // >= 4.5
                    handle.TaskCompletionSource.TrySetCanceled();
#endif
                    return;
                }

                var (buffer, offset, count) = handle;
                var readCount = Read(buffer, offset, count);

                handle.TaskCompletionSource.SetResult(readCount);
            }
        }

        public override void Close()
        {
            DoReadAsync(true);
            base.Close();
        }

        private class AsyncReadHelper
        {
            public CancellationToken CancellationToken { get; set; }
            public TaskCompletionSource<int> TaskCompletionSource { get; set; }

            public byte[] Buffer { get; set; }
            public int Offset { get; set; }
            public int Count { get; set; }

            public void Deconstruct(out byte[] buffer, out int offset, out int count)
            {
                buffer = Buffer;
                offset = Offset;
                count = Count;
            }
        }
    }
}