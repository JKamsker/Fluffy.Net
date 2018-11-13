using System;
using System.Linq;

namespace Fluffy
{
    public class ObjectStorage<TInput, TOutput>
        where TInput : Enum
    {
        private TOutput[] _actions;

        public ObjectStorage()
        {
            var max = ((object)Enum.GetValues(typeof(TInput)).Cast<TInput>().Max());
            var imax = Convert.ToInt32(max);
            _actions = new TOutput[imax + 1];
        }

        public TOutput GetDelegate(TInput opcode)
        {
            var code = opcode.GetHashCode();
            if (code >= _actions.Length)
            {
                throw new AggregateException("Collection is to small");
            }
            return _actions[code];
        }

        public void SetAction(TInput opcode, TOutput @object)
        {
            var code = opcode.GetHashCode();
            _actions[code] = @object;
        }
    }
}