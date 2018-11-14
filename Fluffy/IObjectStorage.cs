namespace Fluffy
{
    public interface IObjectStorage<out TOutput>
    {
        TOutput GetDelegate(int opCode);
    }
}