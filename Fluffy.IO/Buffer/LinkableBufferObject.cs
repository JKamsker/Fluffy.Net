using System.IO;

namespace Fluffy.IO.Buffer
{
    public class LinkedStream : Stream
    {
        public LinkableBufferObject<byte> Head { get; set; }
        public LinkableBufferObject<byte> Body { get; set; }
        private ObjectRecyclingFactory<LinkableBufferObject<byte>> _recyclingFactory;
        private long _length;

        public LinkedStream() : this(8 * 1024)
        {
        }

        public LinkedStream(int cacheSize)
        {
            _recyclingFactory = new ObjectRecyclingFactory<LinkableBufferObject<byte>>(() => new LinkableBufferObject<byte>(cacheSize));
            var buffer = _recyclingFactory.Get();

            Head = buffer;
            Body = buffer;
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
                written += Body.Write(buffer, offset, count);
                if (written < count)
                {
                    Body = _recyclingFactory.Get();
                }
            }

            _length += written;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            int readBytes = 0;
            while (readBytes < count)
            {
                if (Head.Length == 0)
                {
                    if (Head == Body)
                    {
                        break;
                    }

                    var headBuffer = Head;
                    Head = Head.Next;
                    headBuffer.Reset();
                    _recyclingFactory.Recycle(headBuffer);
                }

                readBytes += Head.Read(buffer, offset, count);
            }
            _length -= readBytes;
            return readBytes;
        }

        #region NotImplemented

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

        public override long Length => _length;

        public override long Position
        {
            get => 0;
            set
            {
            }
        }

        #endregion NotImplemented
    }

    public class LinkableBufferObject<T> : BufferObject<T>
    {
        public LinkableBufferObject<T> Next { get; private set; }

        public LinkableBufferObject(int cacheSize)
        {
            Value = new T[cacheSize];
        }

        //public override int Read(T[] destBuffer, int destOffset, int count = -1)
        //{
        //    if (count == -1)
        //    {
        //        count = destBuffer.Length - destOffset;
        //    }

        //    var read = base.Read(destBuffer, destOffset, count);
        //    if (read != count && Next != null)
        //    {
        //        return read + Next.Read(destBuffer, read + destOffset, count - read);
        //    }
        //    return read;
        //}

        public int GetDepth()
        {
            return Next?.GetDepth() + 1 ?? 1;
        }

        public override void Reset()
        {
            Next = null;
            base.Reset();
        }
    }
}