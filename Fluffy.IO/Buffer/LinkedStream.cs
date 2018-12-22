using Fluffy.IO.Recycling;

using System;
using System.IO;

namespace Fluffy.IO.Buffer
{
    public class LinkedStream : Stream, IDisposable
    {
        public int CacheSize => _cacheSize;

        public override long Length => InternalLength;
        protected virtual long InternalLength { get; set; }

        private LinkableBuffer _head;
        private LinkableBuffer _body;

        private IObjectRecyclingFactory<LinkableBuffer> _recyclingFactory;
        private readonly int _cacheSize;

        public LinkedStream(int cacheSize)
            : this(new BufferRecyclingFactory<LinkableBuffer>(cacheSize))
        {
            _cacheSize = cacheSize;
        }

        public LinkedStream()
            : this(Capacity.Medium)
        {
        }

        public LinkedStream(Capacity capacity)
            : this(BufferRecyclingMetaFactory<LinkableBuffer>.MakeFactory(capacity))
        {
        }

        public LinkedStream(IObjectRecyclingFactory<LinkableBuffer> recyclingFactory)
        {
            _recyclingFactory = recyclingFactory;
            var buffer = _recyclingFactory.GetBuffer();

            _head = buffer;
            _body = buffer;
        }

        /// <summary>
        /// Pushes the buffer onto the top of the stream
        /// </summary>
        /// <param name="buffer">
        /// </param>
        public void EnqueueHead(LinkableBuffer buffer)
        {
            buffer.Next = _head;
            _head = buffer;
            InternalLength += buffer.Length;
        }

        /// <summary>
        /// Writes onto the top of the stream.
        /// </summary>
        /// <param name="buffer">
        /// </param>
        /// <param name="offset">
        /// </param>
        /// <param name="count">
        /// </param>
        public void WriteHead(byte[] buffer, int offset, int count)
        {
            if (count == 0)
            {
                return;
            }
            int written = 0;
            while (written < count)
            {
                var targetBuffer = _recyclingFactory.GetBuffer();
                written += targetBuffer.Write(buffer, written, count - written);
                targetBuffer.Next = _head;
                _head = targetBuffer;
            }

            InternalLength += written;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count == 0)
            {
                return;
            }

            int written = 0;
            while (written < count)
            {
                written += _body.Write(buffer, offset + written, count - written);
                if (written < count)
                {
                    var nb = _recyclingFactory.GetBuffer();
                    _body.Next = nb;
                    _body = nb;
                }
            }

            InternalLength += written;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            if (count > buffer.Length)
            {
                count = buffer.Length;
            }

            int readBytes = 0;
            while (readBytes < count)
            {
                if (_head.Length == 0)
                {
                    if (_head == _body || _head.Next == null)
                    {
                        break;
                    }

                    if (!TryMoveNext())
                    {
                        throw new AggregateException("Head is NULL (maybe an threading issue)");
                    }
                }

                readBytes += _head.Read(buffer, offset + readBytes, count - readBytes);
            }

            InternalLength -= readBytes;
            return readBytes;
        }

        public LinkedStream ReadToLinkedStream(int count)
        {
            int read = 0;
            int totalRead = 0;

            var tempBuffer = _recyclingFactory.GetBuffer();
            var buffer = tempBuffer.Value;

            var targetStream = new LinkedStream(_recyclingFactory);

            while ((read = Read(buffer, 0, count - totalRead)) != 0)
            {
                targetStream.Write(buffer, 0, read);
                totalRead += read;
            }
            tempBuffer.Recycle();
            return targetStream;
        }

        private bool TryMoveNext()
        {
            if (_head == null)
            {
                return false;
            }

            var headBuffer = _head;
            _head = _head.Next;
            headBuffer.Recycle();
            return true;
        }

        public override void Close()
        {
            //Recycle loop
            while (TryMoveNext())
            {
            }

            base.Close();
        }

        public override void Flush()
        {
            // throw new System.NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;

        public override long Position
        {
            get => 0;
            set
            {
            }
        }
    }
}