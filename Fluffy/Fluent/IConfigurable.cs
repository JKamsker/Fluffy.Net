using System;

namespace Fluffy.Fluent
{
    public interface IDecisionConfigurator
    {
        IDecisionConfigurator Default(Action action);

        IDecisionNode<T> On<T>();

        IDecisionNode<T> On<T>(Predicate<T> condition);
    }
}