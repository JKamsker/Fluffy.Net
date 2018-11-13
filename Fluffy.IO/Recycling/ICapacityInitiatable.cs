namespace Fluffy.IO.Recycling
{
    public interface ICapacityInitiatable
    {
        void Initiate(ICapacity capacity);
    }

    public interface ICapacity
    {
        int Capacity { get; }
    }
}