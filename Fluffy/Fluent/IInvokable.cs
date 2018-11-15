namespace Fluffy.Fluent
{
    public interface IInvokable
    {
        bool Invoke(object value);
    }

    public interface IInvokable<in T>
    {
        bool Invoke(T value);
    }
}