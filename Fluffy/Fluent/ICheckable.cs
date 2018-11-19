namespace Fluffy.Fluent
{
    public interface ICheckable
    {
        bool Check(object value);
    }

    public interface IContextAwareCheckable<TContext>
    {
        bool Check(TContext context, object value);
    }
}