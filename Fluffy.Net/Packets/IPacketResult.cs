#if !NET40

using System.Runtime.CompilerServices;

#endif

using System.Threading.Tasks;

namespace Fluffy.Net.Packets
{
    public interface IPacketResult<TResult>
    {
        TResult Result { get; }
        Task<TResult> Task { get; }

#if !NET40

        TaskAwaiter<TResult> GetAwaiter();

#endif
    }
}