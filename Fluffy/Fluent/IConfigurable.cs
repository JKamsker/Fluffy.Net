using System;

namespace Fluffy.Fluent
{
    public interface IDecisionConfigurator
    {
        IDecisionConfigurator Default(Action<object> action);

        IDecisionNode<T> On<T>();

        IDecisionNode<T> On<T>(Predicate<T> condition);
    }
}