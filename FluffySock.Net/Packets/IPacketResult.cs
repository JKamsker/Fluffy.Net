using System.Threading.Tasks;

namespace Fluffy.Net.Packets
{
    public interface IPacketResult<TResult>
    {
        TResult Result { get; }
        Task<TResult> ResultTask { get; }
    }
}