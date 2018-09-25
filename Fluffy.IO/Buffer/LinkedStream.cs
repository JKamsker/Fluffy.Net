using System.IO;

namespace Fluffy.IO.Buffer
{
    public class LinkedStream : Stream
    {
        public int CacheSize => _cacheSize;
        public IObjectRecyclingFactory<LinkableBufferObject<byte>> ObjectRecyclingFactory => _recyclingFactory;

        private LinkableBufferObject<byte> _head;
        private LinkableBufferObject<byte> _body;

        private IObjectRecyclingFactory<LinkableBufferObject<byte>> _recyclingFactory;
        private readonly int _cacheSize;
        private long _length;

        public LinkedStream() : this(8 * 1024)
        {
        }

        public LinkedStream(int cacheSize)
            : this(new ConcurrentObjectRecyclingFactory<LinkableBufferObject<byte>>(() => new LinkableBufferObject<byte>(cacheSize)))
        {
            _cacheSize = cacheSize;
        }

        public LinkedStream(IObjectRecyclingFactory<LinkableBufferObject<byte>> recyclingFactory)
        {
            _recyclingFactory = recyclingFactory;
            var buffer = _recyclingFactory.Get();

            _head = buffer;
            _body = buffer;
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
                written += _body.Write(buffer, written, count - written);
                if (written < count)
                {
                    var nb = _recyclingFactory.Get();
                    _body.Next = nb;
                    _body = nb;
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

                    var headBuffer = _head;
                    _head = _head.Next;
                    headBuffer.Reset();
                    _recyclingFactory.Recycle(headBuffer);
                }

                readBytes += _head.Read(buffer, readBytes, count - readBytes);
            }

            _length -= readBytes;
            return readBytes;
        }

        public LinkedStream ReadToLinkedStream(int count)
        {
            var tempBuffer = _recyclingFactory.Get();
            var buffer = tempBuffer.Value;

            int read = 0;
            int totalRead = 0;
            var targetStream = new LinkedStream(_recyclingFactory);

            while ((read = Read(buffer, 0, count - totalRead)) != 0)
            {
                targetStream.Write(buffer, 0, read);
                totalRead += read;
            }

            _recyclingFactory.Recycle(tempBuffer);
            return targetStream;
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