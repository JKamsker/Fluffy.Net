using System;

namespace Fluffy.Fluent
{
    public interface ICanDo<out T>
    {
        void Do(Action<T> action);
    }
}