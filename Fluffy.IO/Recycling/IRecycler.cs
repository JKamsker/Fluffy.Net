namespace Fluffy.IO.Recycling
{
    public interface IRecycler<in T>
    {
        void Recycle(T @object);
    }
}