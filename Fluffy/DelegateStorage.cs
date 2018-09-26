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
            var max = Enum.GetValues(typeof(TInput)).Cast<int>().Max();

            _actions = new TOutput[max + 1];
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