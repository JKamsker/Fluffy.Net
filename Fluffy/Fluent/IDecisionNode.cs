using System;

namespace Fluffy.Fluent
{
    public interface IDecisionNode<out T>
    {
        IDecisionConfigurator Do(Action<T> action);
    }
}