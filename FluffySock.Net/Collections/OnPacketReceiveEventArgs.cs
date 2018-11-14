using Fluffy.IO.Buffer;
using Fluffy.Net.Options;

namespace Fluffy.Net.Collections
{
    internal struct OnPacketReceiveEventArgs
    {
        public byte OpCode;
        public ParallelismOptions Options;
        public LinkedStream Body;

        public OnPacketReceiveEventArgs(byte opCode, ParallelismOptions options, LinkedStream body)
        {
            Options = options;
            OpCode = opCode;
            Body = body;
        }
    }
}