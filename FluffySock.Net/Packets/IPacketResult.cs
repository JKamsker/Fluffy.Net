using System.Threading.Tasks;

namespace Fluffy.Net.Packets
{
    public interface IPacketResult<TResult>
    {
        TResult Value { get; }
        Task<TResult> Task { get; }
    }
}