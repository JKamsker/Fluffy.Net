using Fluffy.IO.Recycling;

namespace Fluffy.IO.Buffer
{
    public interface IObjectRecyclingFactory<T> : IRecycler<T>
    {
        T GetBuffer();
    }
}