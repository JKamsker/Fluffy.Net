namespace Fluffy.IO.Buffer
{
    public class LinkableBufferObject<T> : BufferObject<T>
    {
        public LinkableBufferObject<T> Next { get; internal set; }

        public LinkableBufferObject(int cacheSize) : base(cacheSize)
        {
        }

        public LinkableBufferObject() : base()
        {

        }

        public override void Reset()
        {
            Next = null;
            base.Reset();
        }
    }
}