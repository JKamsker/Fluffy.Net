using System;
using System.Threading;
using System.Threading.Tasks;
using Fluffy.IO.Buffer;
using Fluffy.IO.Recycling;

namespace AllInOneExample_FullFramework
{
    internal class AsyncLinkedStream : LinkedStream
    {
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

        /// <summary>
        /// Prevent event cascading
        /// </summary>
        private bool _lengthChangeNotifying;

        public EventHandler<long> LengthChanged;

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

                //Prevent event cascading
                if (_lengthChangeNotifying)
                {
                    return;
                }

                _lengthChangeNotifying = true;
                LengthChanged?.Invoke(this, value);
                _lengthChangeNotifying = false;
            }
        }

        // private override long InternalLength { get; set; }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            LengthChanged += (sender, l) =>
            {
                if (InternalLength < count)
                {
                    return;
                }
            };
            return base.ReadAsync(buffer, offset, count, cancellationToken);
        }
    }
}