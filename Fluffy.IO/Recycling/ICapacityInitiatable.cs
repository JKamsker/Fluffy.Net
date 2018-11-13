namespace Fluffy.IO.Recycling
{
    public interface ICapacityInitiatable
    {
        void Initialize(ICapacity capacity);
    }

    public interface ICapacity
    {
        int Capacity { get; }
    }
}