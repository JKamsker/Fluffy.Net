namespace Fluffy.IO.Buffer
{
    public class LinkableBufferObject<T> : BufferObject<T>
    {
        public LinkableBufferObject<T> Next { get; internal set; }

        public LinkableBufferObject(int cacheSize)
        {
            Value = new T[cacheSize];
        }

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