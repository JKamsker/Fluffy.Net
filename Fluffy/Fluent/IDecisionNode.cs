using System;

namespace Fluffy.Fluent
{
    public interface IDecisionNode<out TValue>
    {
        IDecisionConfigurator Do(Action<TValue> action);

        IDecisionConfigurator Do(Func<TValue, object> func);
    }

    public interface IDecisionNode<out TValue, out TContext>
    {
        IDecisionConfigurator<TContext> Do(Action<TValue> action);

        IDecisionConfigurator<TContext> Do(Func<TValue, object> func);

        IDecisionConfigurator<TContext> Do(Action<TValue, TContext> action);

        IDecisionConfigurator<TContext> Do(Func<TValue, TContext, object> func);
    }
}