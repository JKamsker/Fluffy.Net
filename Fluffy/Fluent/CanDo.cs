using System;

namespace Fluffy.Fluent
{
    public class CanDo<T> : ICanDo<T>, IInvokable<T>, IInvokable, ICheckable
    {
        private readonly Predicate<T> _condition;
        private Action<T> _action;

        internal CanDo() : this(x => true)
        {
        }

        internal CanDo(Predicate<T> condition)
        {
            _condition = condition;
        }

        public void Do(Action<T> action)
        {
            _action = action;
        }

        public bool Invoke(T value)
        {
            if (_action == null)
            {
                return false;
            }
            _action.Invoke(value);
            return true;
        }

        public bool Check(object value)
        {
            return value is T typedValue && _condition(typedValue);
        }

        public bool Invoke(object value)
        {
            if (_action == null)
            {
                return false;
            }

            if (value is T invokeValue)
            {
                _action.Invoke(invokeValue);
                return true;
            }

            return false;
        }
    }
}