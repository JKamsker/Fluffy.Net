using System;

namespace Fluffy.Fluent
{
    public interface IConfigurable
    {
        void Default(Action action);
        ICanDo<T> On<T>();
        ICanDo<T> On<T>(Predicate<T> condition);
    }
}