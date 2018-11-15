using System;

namespace Fluffy.Fluent
{
    public interface ICanDo<out T>
    {
        IConfigurable Do(Action<T> action);
    }
}