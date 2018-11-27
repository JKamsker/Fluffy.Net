using System;
using System.Linq;

namespace Fluffy.Delegate
{
    public class DelegateStorage<TInput, TOutput> : IObjectStorage<TOutput>
        where TInput : Enum
    {
        private TOutput[] _actions;

        public DelegateStorage()
        {
            var max = ((object)Enum.GetValues(typeof(TInput)).Cast<TInput>().Max());
            var imax = Convert.ToInt32(max);
            _actions = new TOutput[imax + 1];
        }

        public TOutput GetDelegate(TInput opcode)
        {
            var code = Convert.ToInt32(opcode);
            return GetDelegate(code);
        }

        public TOutput GetDelegate(int opCode)
        {
            if (opCode >= _actions.Length)
            {
                throw new AggregateException("Collection is to small");
            }
            return _actions[opCode];
        }

        public void SetAction(TInput opCode, TOutput @object)
        {
            var code = Convert.ToInt32(opCode);
            _actions[code] = @object;
        }
    }
}