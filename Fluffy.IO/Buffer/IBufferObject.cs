namespace Fluffy.IO.Buffer
{
    public interface IBufferObject<T>
    {
        int High { get; }
        int Length { get; }
        int Low { get; }
        int RemainingCapacity { get; }
        T[] Value { get; }

        int Read(T[] destBuffer, int destOffset, int count = -1);

        void Reset();

        int Write(T[] sourceBuffer, int sourceOffset, int count = -1);
    }
}