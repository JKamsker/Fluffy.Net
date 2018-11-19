using System;

namespace Fluffy.Fluent
{
    public interface IDecisionConfigurator
    {
        IDecisionConfigurator Default(Action<object> action);

        IDecisionNode<T> On<T>();

        IDecisionNode<T> On<T>(Predicate<T> condition);
    }

    public interface IDecisionConfigurator<out TContext>
    {
        IDecisionConfigurator<TContext> Default(Action<object> action);

        IDecisionNode<T, TContext> On<T>();

        IDecisionNode<T, TContext> On<T>(Predicate<T> condition);
    }
}