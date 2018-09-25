namespace Fluffy.IO.Buffer
{
    public interface IObjectRecyclingFactory<T>
    {
        T Get();

        void Recycle(T @object);
    }
}