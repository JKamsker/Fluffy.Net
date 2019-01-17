using Fluffy.IO.Recycling;

namespace Fluffy.IO.Buffer
{
    public interface IObjectRecyclingFactory<out T>
    {
        T GetBuffer();
    }
}