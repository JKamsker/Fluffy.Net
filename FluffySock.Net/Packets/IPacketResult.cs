using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Fluffy.Net.Packets
{
    public interface IPacketResult<TResult>
    {
        TResult Result { get; }
        Task<TResult> Task { get; }

        TaskAwaiter<TResult> GetAwaiter();
    }
}