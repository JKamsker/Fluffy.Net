namespace Fluffy.Fluent
{
    public interface IInvokable
    {
        bool Invoke(object value);

        bool Invoke(object value, out object result);
    }

    public interface IInvokable<in T>
    {
        bool Invoke(T value);

        bool Invoke(T value, out object result);
    }

    public interface IContextAwareInvokable<in TContext>
    {
        bool Invoke(object value, TContext context);

        bool Invoke(object value, TContext context, out object result);
    }

    public interface IContextAwareInvokable<in TValue, in TContext>
    {
        bool Invoke(TValue value, TContext context);

        bool Invoke(TValue value, TContext context, out object result);
    }
}