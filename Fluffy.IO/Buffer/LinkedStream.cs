﻿using System.IO;

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
                written += Body.Write(buffer, written, count - written);
                if (written < count)
                {
                    var nb = _recyclingFactory.Get();
                    Body.Next = nb;
                    Body = nb;
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
                    if (Head == Body || Head.Next == null)
                    {
                        break;
                    }

                    var headBuffer = Head;
                    Head = Head.Next;
                    headBuffer.Reset();
                    _recyclingFactory.Recycle(headBuffer);
                }

                readBytes += Head.Read(buffer, readBytes, count - readBytes);
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
}