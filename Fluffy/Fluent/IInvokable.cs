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
}